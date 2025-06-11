using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gitmanik.Console;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject[] npcPrefabs;
    
    private int entityIDCounter = 0;
    public int GetEntityID() => ++entityIDCounter;
    public List<NPC> npcs = new List<NPC>();
    [HideInInspector] public bool LlmServerReady = false;
    [HideInInspector] public bool HistoryGenerated = false;
    
    private List<string> archetypes;
    public GeneratedHistoryDTO generatedHistory;
    public ConvertHistoryToBlocksDTO storyBlocks;

    public NPC murdererNPC;

    public GameConfig gameConfig { get; private set; }

    [SerializeField] private GameObject uiGameObject;
    [SerializeField] public GameObject MinimapGameObject;

    [SerializeField] private AudioSource musicAudioSource;

    [Header("DEBUG")]
    [Header("Config")]
    [SerializeField] private bool useCustomGameConfig = false;
    [SerializeField] private string customGameConfigJSON = "";
    
    [Header("Narrator")]
    [SerializeField] private bool DontGenerateHistory = false;
    [SerializeField] private string historyJSON = "";
    [SerializeField] private bool DontConvertHistoryToBlocks = false;
    [SerializeField] private string historyBlocksJSON = "";

    [Header("Decision Making")]
    public bool SkipRelevance = false;


    IEnumerator Start()
    {
        Debug.Log("GameManager: Start initialization");
        Instance = this;
        uiGameObject.SetActive(false);

        if (useCustomGameConfig)
            gameConfig = JsonUtility.FromJson<GameConfig>(customGameConfigJSON);
        else
            gameConfig = GameConfig.LoadGameConfig(Path.Combine(Application.dataPath, "game_config.json"));
        gameConfig.Models.ForEach(m => m.Name = m.Name.Substring(0, m.Name.LastIndexOf('.')));
        Debug.Log("GameManager: Config loaded");

        Screen.SetResolution(gameConfig.GameWindowWidth, gameConfig.GameWindowHeight, gameConfig.FullScreen);
        Application.targetFrameRate = 60;

        ApplySettings();

        LlmManager.Instance.Setup(gameConfig.LlmServerApi);
        Debug.Log("GameManager: LLM Setup started");

        // Wait for LLM connection and model loading
        yield return StartCoroutine(WaitForLlmConnection());
        
        archetypes = gameConfig.Models.FindAll(x => x.Id != gameConfig.NarratorModelId).Select(x => x.Name.Replace('_', ' ')).ToList();

        Debug.Log($"GameManager: Archetypes: {string.Join(',', archetypes)}");
        
        if (DontGenerateHistory)
            generatedHistory = JsonUtility.FromJson<GeneratedHistoryDTO>(historyJSON);
        else
            yield return StartCoroutine(GenerateHistory());
        
        if (DontConvertHistoryToBlocks)
            storyBlocks = JsonUtility.FromJson<ConvertHistoryToBlocksDTO>(historyBlocksJSON);
        else
            yield return StartCoroutine(ConvertHistoryToBlocks());

        MiniGameManager.Instance.Setup(storyBlocks.key_events, storyBlocks.false_events);

        MapGenerator.Instance.GenerateMap();
        
        List<GameObject> npcPrefabsList = npcPrefabs.ToList();

        if (LlmServerReady && MapGenerator.Instance.IsMapGenerated)
        {
            uiGameObject.SetActive(true);
            DayNightCycle.Instance.enableTimePass = true;
        }
        else
        {
            Debug.LogError(LlmServerReady ? "Game Manager: Map not generated yet." : "LLM Server not ready.");
        }

            
        Debug.Log("Game Manager: " + gameConfig.Npcs.Count + " NPCs to spawn");

        var localCharacters = generatedHistory.characters.ToList();
        var localStoryBlocks = storyBlocks.key_events.ToList();
        
        foreach (var npcConfig in gameConfig.Npcs)
        {
            Vector3 npcPosition = new Vector3(
                MapGenerator.Instance.transform.position.x - MapGenerator.Instance.mapWidth / 2 + Random.Range(0, MapGenerator.Instance.mapWidth),
                0,
                MapGenerator.Instance.transform.position.z - MapGenerator.Instance.mapLength / 2 + Random.Range(0, MapGenerator.Instance.mapLength)
            );

            var npcModel = gameConfig.Models.FirstOrDefault(m => m.Id == npcConfig.ModelId);
            if (npcModel == null)
            {
                Debug.LogError($"GameManager: NPC {npcConfig.Id} model not found (ModelId: {npcConfig.ModelId})");
                continue;
            }
            
            int npcVariant = Random.Range(0, npcPrefabsList.Count);
            
            GameObject newNpc = Instantiate(npcPrefabsList[npcVariant], npcPosition, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(newNpc, SceneManager.GetSceneByName("Game"));
            npcPrefabsList.RemoveAt(npcVariant);
            var npcComponent = newNpc.GetComponent<NPC>();

            var npcModelArchetype = npcModel.Name.Replace('_', ' ');
            
            var characterDTO = localCharacters.Find(x => archetypes[x.archetype - 1] == npcModelArchetype && !x.dead);
            localCharacters.Remove(characterDTO);
            
            var storyBlock = localStoryBlocks[Random.Range(0, localStoryBlocks.Count)];
            localStoryBlocks.Remove(storyBlock);
            
            IDecisionSystem system;
            if (string.IsNullOrEmpty(npcModel.Path))
            {
                Debug.LogError($"Model path not found for NPC with ID {npcConfig.ModelId}");
                system = new NullDecisionSystem();
            }
            else
            {
                system = new LlmDecisionMaker();
            }
            npcComponent.Setup(system, npcConfig.ModelId.ToString(), characterDTO);
            npcComponent.SystemPrompt += "VERY IMPORTANT (it plays a very big role to You): You know that at the day of murder " + storyBlock;

            
            HashSet<BuildingData.BuildingType> allowedTypes = new HashSet<BuildingData.BuildingType>()
            {
                BuildingData.BuildingType.House,
                BuildingData.BuildingType.Tavern,
                BuildingData.BuildingType.Blacksmith,
                BuildingData.BuildingType.Church
            };
            npcComponent.HisBuilding = MapGenerator.Instance.GetBuilding(allowedTypes);
            var buildingData = npcComponent.HisBuilding.GetComponent<BuildingData>();
            buildingData.HisNPC.Add(npcComponent);

            npcs.Add(npcComponent);

            if (characterDTO.murderer)
            {
                if (murdererNPC != null)
                {
                    Debug.LogError($"More than one Characters are the murderer!");
                }
                murdererNPC = npcComponent;
            }
        }

        //Clue spawn
        MapGenerator.Instance.GenerateClue();
        //Player spawn
        GameObject wallsRoot = GameObject.Find("WallsRoot");

        Transform gates = null;

        foreach (Transform child in wallsRoot.transform)
        {
            if (child.name == "GATE(Clone)")
            {
                gates = child;
                break;
            }
        }
        if (gates == null)
        {
            Debug.LogError("Nie znaleziono bramy (Gate(Clone))!");
            //return;
        }

        // Znajdź Entrance w _minnor_gates_02(Clone)
        Transform entrance = gates.Find("PlayerSpawner");
        if (entrance == null)
        {
            Debug.LogError("Nie znaleziono PlayerSpawner!");
            //return;
        }

        // Ustaw gracza w pozycji Entrance
        PlayerController.Instance.gameObject.transform.position = entrance.position;
        PlayerController.Instance.gameObject.transform.rotation = entrance.rotation;

        Debug.Log("Player ustawiony na spawn point Entrance.");
    }

    /// <summary>
    /// Coroutine that waits for a successful connection to the LLM server,
    /// registers all models used in the game, and sets the LlmServerReady flag when done.
    /// If the connection fails, it retries every 5 seconds.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    IEnumerator WaitForLlmConnection()
    {
        while (true)
        {
            bool isConnected = false;
            bool callbackCalled = false;

            LlmManager.Instance.Connect(result =>
            {
                isConnected = result;
                callbackCalled = true;
            });

            // Wait for the callback to be called
            while (!callbackCalled)
                yield return null;

            if (isConnected)
            {
                Debug.Log("GameManager: Connected to LLM Server");

                var usedModelIds = new HashSet<int>(
                    gameConfig.Npcs.Select(npc => npc.ModelId)
                    .Concat(new[] { gameConfig.NarratorModelId })
                );

                var usedModels = gameConfig.Models.Where(model => usedModelIds.Contains(model.Id)).ToList();

                // Register all models that are used in the game
                int modelsToRegister = usedModels.Count;
                bool[] registered = new bool[modelsToRegister];

                for (int i = 0; i < usedModels.Count; i++)
                {
                    Debug.Log($"GameManager: Registering model number {i+1} from path {usedModels[i].Path}");
                    int idx = i;
                    var model = usedModels[i];
                    LlmManager.Instance.Register(model.Id.ToString(), model.Path, (dto) =>
                    {
                        registered[idx] = true;
                        LlmManager.Instance.GenericComplete(dto);
                    }, Debug.LogError);
                }

                // Wait until all models are registered
                while (registered.Any(r => !r))
                    yield return null;

                Debug.Log("GameManager: All models registered, proceeding to load...");

                LlmServerReady = true;
                Debug.Log("GameManager: All models registered, LlmServerReady = TRUE");
                break;
            }
            else
            {
                Debug.LogError("GameManager: Failed to connect to LLM Server. Retrying in 5 seconds...");
                yield return new WaitForSeconds(5f);
            }
        }

    }

    /// <summary>
    /// Coroutine that generates a story using Narrator LLM Model,
    /// If the generation fails, it retries automatically.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator GenerateHistory()
    {
        string prompt = $"You are a creative writer. " +
                        $"Write ONLY a VALID JSON object with body specified below:\n\n" +
                        $"A short dark story set in a medieval village.\n" +
                        $"The story must include a murder, with the victim being one of generated characters.\n" +
                        $"Describe a mysterious situation with tension and uncertainty.\n\n" +
                        $"ALL {gameConfig.Npcs.Count + 1} characters MUST BE in JSON section and OVER 18 years old." +
                        $"Use a dark tone with rich sensory details (e.g., rain, silence, fear, time of day).\n" +
                        $"Use only the following locations:\n" +
                            $"The church\n" +
                            $"The well\n" +
                            $"The house of each character in the story (Do not use any other places.)\n\n" +
                        $"There must be exactly {gameConfig.Npcs.Count + 1} game characters, each with one of these personality types (archetypes): [{string.Join(',', archetypes)}]\n\n" +
                        $"Select ONE character who is the murderer (they NEED be guilty directly or indirectly)\n" +
                        $"Select ONE character who is a witness (they saw the murder or something suspicious)\n" +
                        $"Select ONE character who is a victim.\n" +
                        $"Include who's the murderer and who's the victim in the story." +
                        $"The story must be a single paragraph of literary narration, not a list or report.\n" +
                        $"A list of the **{gameConfig.Npcs.Count + 1}** characters with this information for each:\n" +
                            $"name (you choose)\n" +
                            $"archetype (one of the four used)\n" +
                            $"age (an integer)\n" +
                            $"description (about who they are, their personality, what they saw at the moment of the murder, what they are doing during the story, how they feel, who they like or dislike, and one habit or routine they have)\n" +
                            $"murderer (boolean)\n" +
                            $"dead (boolean)\n" + 
                            $"Remember to NOT write a comma after description as it is an end of the child JSON object. Remember to output ONLY VALID JSON with structure given above. Remember to not write comma after last objects in list and in object!" +
                            $"Use this EXACT format for your output:\n\n" +
                            $"{{\n" +
                                $"\"story\": \"Your short story goes here.\",\n" +
                                $"\"characters\": [\n" +
                                    $"{{\n\"name\": \"Full name\",\n" +
                                    $"\"archetype\": <(1-{archetypes.Count}) index from personality list matching the character>,\n" +
                                    $"\"age\": 52,\n" +
                                    $"\"description\": \"You are a delusional man. You are 52 years old. [Add more details here.]\",\n" +
                                    $"\"murderer\": false\n" +
                                    $"\"dead\": false\n" + 
                                    $"}}," +
                                $"\n...\n]" +
                            $"\n}}\n";
        
        List<Message> messages = new List<Message>();
        messages.Add(new Message { role =  "user", content = prompt});

        while (true)
        {
            bool callbackCalled = false;
            string resp = null;
        
            Debug.Log("Generating history...");
            
            LlmManager.Instance.Chat(gameConfig.NarratorModelId.ToString(), messages, result =>
            {
                callbackCalled = true;
                resp = result.response;
            }, (error) =>
            {
                Debug.LogError($"GameManager: GenerateHistory error: {error}");
                callbackCalled = true;
            }, 0.95f, 0.5f);
        
            // Wait for the callback to be called
            while (!callbackCalled)
                yield return null;

            if (resp == null)
                continue;

            if (!resp.Contains('{') || !resp.Contains('}'))
            {
                Debug.LogError($"GameManager: GenerateHistory error: missing JSON brackets\n{resp}");
                continue;
            } 
                
            string strippedResp = resp.Substring(resp.IndexOf('{'));
            strippedResp = strippedResp.Substring(0, strippedResp.LastIndexOf('}') + 1);

            try
            {
                generatedHistory = JsonUtility.FromJson<GeneratedHistoryDTO>(strippedResp);
                if (string.IsNullOrWhiteSpace(generatedHistory.story))
                {
                    Debug.LogError($"GameManager: GenerateHistory error: generated history is empty\nFull response:{resp}\n\nStripped response:{strippedResp}");
                    continue;
                }

                if (generatedHistory.characters.Count(x => x.murderer) != 1)
                {
                    Debug.LogError($"GameManager: GenerateHistory error: generated history has {generatedHistory.characters.Count(x => x.murderer)} murderers!\nFull response:{resp}\n\nStripped response:{strippedResp}");
                    continue;
                }
                
                if (generatedHistory.characters.Count(x => x.dead) != 1)
                {
                    Debug.LogError($"GameManager: GenerateHistory error: generated history has {generatedHistory.characters.Count(x => x.murderer)} victims!\nFull response:{resp}\n\nStripped response:{strippedResp}");
                    continue;
                }

                if (generatedHistory.characters.Any(x => x.archetype - 1 < 0 || x.archetype > archetypes.Count))
                {
                    Debug.LogError($"GameManager: GenerateHistory error: generated character has invalid archetype index!\nFull response:{resp}\n\nStripped response:{strippedResp}");
                    continue;     
                }

                if (generatedHistory.characters.Count != gameConfig.Npcs.Count + 1)
                {
                    Debug.LogError($"GameManager: GenerateHistory error: history character count does not match gameConfig.Npcs.Count + 1!\nFull response:{resp}\n\nStripped response:{strippedResp}");
                    continue;     
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManager: Error parsing generated history output: {e.Message}:\nFull response:{resp}\n\nStripped response:{strippedResp}");
                continue;
            }
        
            Debug.Log($"GameManager: Generate history complete:\n{generatedHistory}");
            break;
        }
    }

    /// <summary>
    /// Coroutine that generates blocks from Story using Narrator LLM Model,
    /// If the generation fails, it retries automatically.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator ConvertHistoryToBlocks()
    {
        string prompt = $"You will be given a short story.\n" +
                        $"Extract SIX most important factual events.\n" +
                        $"Then, think of TWO false sentences that do not occur in the story, but are believable enough to fool someone. " +
                        $"You will NOT include names in the output." + 
                        $"Output these sentences into a JSON Object structure:\n\n" +
                        $"\"key_events\" — an array of exactly six short English sentences, each stating a concrete, factual event that actually appears in the story.\n" +
                        $"\"false_events\" — an array of exactly two short English sentences that are believable but clearly did not happen in the story.\n" +
                        $"Your response must be ONLY this EXACT CORRECT JSON object:\n" +
                            $"{{\n\"key_events\": [\n" +
                                $"\"Event 1 here.\",\n" +
                                $"\"Event 2 here.\",\n" +
                                $"\"Event 3 here.\",\n" +
                                $"\"Event 4 here.\",\n" +
                                $"\"Event 5 here.\",\n" +
                                $"\"Event 6 here.\"\n" +
                            $"],\n" +
                            $"\"false_events\": [\n" +
                                $"\"False event 1 here.\",\n" +
                                $"\"False event 2 here.\"\n" +
                            $"]\n}}";
        List<Message> messages = new List<Message>();
        messages.Add(new Message { role =  "system", content = prompt});
        messages.Add(new Message { role =    "user", content = generatedHistory.story});
        
        while (true)
        {
            bool callbackCalled = false;
            string resp = null;
            
            Debug.Log("Converting history to blocks...");
        
            LlmManager.Instance.Chat(gameConfig.NarratorModelId.ToString(), messages, result =>
            {
                callbackCalled = true;
                resp = result.response;
            }, (error) =>
            {
                Debug.LogError($"GameManager: ConvertHistoryToBlocks error: {error}");
                callbackCalled = true;
            }, 0.95f, 0.5f);
        
            // Wait for the callback to be called
            while (!callbackCalled)
                yield return null;

            if (resp == null)
                continue;

            if (!resp.Contains('{') || !resp.Contains('}'))
            {
                Debug.LogError($"GameManager: ConvertHistoryToBlocks error: missing JSON brackets\n{resp}");
                continue;
            } 
            
            string strippedResp = resp.Substring(resp.IndexOf('{'));
            strippedResp = strippedResp.Substring(0, strippedResp.LastIndexOf('}') + 1);
        
            try
            {
                storyBlocks = JsonUtility.FromJson<ConvertHistoryToBlocksDTO>(strippedResp);
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManager: ConvertHistoryToBlocks error: {e.Message}:\nFull response:{resp}\n\nStripped response:{strippedResp}");
                continue;
            }
        
            Debug.Log($"GameManager: ConvertHistoryToBlocks complete:\n{storyBlocks}");
        
            HistoryGenerated = true;
            break;
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12)){
            string folderPath = "Assets/Screenshots/"; 
    
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);
            
            var screenshotName =
                                    "Screenshot_" +
                                    System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") +
                                    ".png";
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName));
            Debug.Log(folderPath + screenshotName);
        }
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            GitmanikConsole.singleton.ToggleConsole();
        }
    }

    private void ApplySettings()
    {
        // VIDEO
        int graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        QualitySettings.SetQualityLevel(graphicsQuality);

        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;

        // AUDIO
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        AudioListener.volume = masterVolume;

        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = musicVolume;
        }

    }

    [ConsoleCommand("npcs", "List all npcs")]
    public static bool ListAllNpcs()
    {
        string x = "NPCs:\n";
        foreach (var npc in GameManager.Instance.npcs)
            x += $"<b>{npc.EntityID} {npc.NpcName}</b> ({npc.transform.position})\n<b>System:</b> ({npc.GetCurrentDecision().DebugInfo()})\n<b>Hunger:</b> {npc.Hunger}\n<b>Thirst:</b> {npc.Thirst}\n<b>Obtained Memories:</b>\n{string.Join("\n", npc.ObtainedMemories)}\n<b>System Prompt:</b>\n{npc.SystemPrompt.Replace('.', '\n')}\n";
        Debug.Log(x);
        return true;
    }

    [ConsoleCommand("tp", "Teleport to NPC")]
    public static bool TeleportToNPC(string par1)
    {
        if (!int.TryParse(par1, out var id))
            return false;
        
        var npc = Instance.npcs.Find(x => x.EntityID == id);
        if (npc == null)
            return false;
        
        PlayerController.Instance.transform.position = npc.transform.position;
        return true;
    }

    [ConsoleCommand("int", "Interact with NPC")]
    public static bool InteractWithNPC(string par1)
    {
        if (!int.TryParse(par1, out var id))
            return false;
        
        var npc = Instance.npcs.Find(x => x.EntityID == id);
        if (npc == null)
            return false;
        
        PlayerController.Instance.StartInteraction(npc);
        return true;
    }

    [ConsoleCommand("story", "Dumps GeneratedHistoryDTO as JSON")]
    public static bool DumpStory()
    {
        Debug.Log(JsonUtility.ToJson(Instance.generatedHistory));
        return true;
    }
    [ConsoleCommand("blocks", "Dumps ConvertHistoryToBlocksDTO as JSON")]
    public static bool DumpBlocks()
    {
        Debug.Log(JsonUtility.ToJson(Instance.storyBlocks));
        return true;
    }
}
