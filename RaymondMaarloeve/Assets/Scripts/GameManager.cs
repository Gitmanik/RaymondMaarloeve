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
    
    public GeneratedHistoryDTO generatedHistory;
    public ConvertHistoryToBlocksDTO storyBlocks;

    public NPC murdererNPC;

    public GameConfig gameConfig { get; private set; }

    [SerializeField] private GameObject uiGameObject;

    [SerializeField] private AudioSource musicAudioSource;


    IEnumerator Start()
    {
        Debug.Log("GameManager: Start initialization");
        Instance = this;

        gameConfig = GameConfig.LoadGameConfig(Path.Combine(Application.dataPath, "game_config.json"));
        Debug.Log("GameManager: Config loaded");

        Screen.SetResolution(gameConfig.GameWindowWidth, gameConfig.GameWindowHeight, gameConfig.FullScreen);
        Application.targetFrameRate = 60;

        ApplySettings();

        LlmManager.Instance.Setup(gameConfig.LlmServerApi);
        Debug.Log("GameManager: LLM Setup started");

        // Wait for LLM connection and model loading
        yield return StartCoroutine(WaitForLlmConnection());

        yield return StartCoroutine(GenerateHistory());

        yield return StartCoroutine(ConvertHistoryToBlocks());
        MiniGameManager.Instance.Setup(storyBlocks.key_events, storyBlocks.false_events);

        MapGenerator.Instance.GenerateMap();
        
        List<GameObject> npcPrefabsList = npcPrefabs.ToList();

        if (LlmServerReady && MapGenerator.Instance.IsMapGenerated)
        {
            uiGameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(LlmServerReady ? "Game Manager: Map not generated yet." : "LLM Server not ready.");
        }

            
        Debug.Log("Game Manager: " + gameConfig.Npcs.Count + " NPCs to spawn");

        var localGeneratedHistory = generatedHistory;
        
        List<string> archetypes = gameConfig.Models.FindAll(x => x.Id != gameConfig.NarratorModelId).
            ConvertAll(x => x.Name.Substring(0,x.Name.IndexOf('.')).Replace("_", " "));
        
        foreach (var npcConfig in gameConfig.Npcs)
        {
            Vector3 npcPosition = new Vector3(
                MapGenerator.Instance.transform.position.x - MapGenerator.Instance.mapWidth / 2 + Random.Range(0, MapGenerator.Instance.mapWidth),
                0,
                MapGenerator.Instance.transform.position.z - MapGenerator.Instance.mapLength / 2 + Random.Range(0, MapGenerator.Instance.mapLength)
            );

            string npcModelPath = gameConfig.Models.FirstOrDefault(m => m.Id == npcConfig.ModelId)?.Path;
            int npcVariant = Random.Range(0, npcPrefabsList.Count);
            
            GameObject newNpc = Instantiate(npcPrefabsList[npcVariant], npcPosition, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(newNpc, SceneManager.GetSceneByName("Game"));
            npcPrefabsList.RemoveAt(npcVariant);
            var npcComponent = newNpc.GetComponent<NPC>();

            var characterDTO =
                localGeneratedHistory.characters.Find(x => $"{archetypes[x.archetype - 1].Replace(" ", "_")}.gguf" == gameConfig.Models.Find(y => y.Id == npcConfig.ModelId).Name);
            localGeneratedHistory.characters.Remove(characterDTO);
            
            IDecisionSystem system;
            if (string.IsNullOrEmpty(npcModelPath))
            {
                Debug.LogError($"Model path not found for NPC with ID {npcConfig.ModelId}");
                system = new NullDecisionSystem();
            }
            else
            {
                system = new LlmDecisionMaker();
            }
            npcComponent.Setup(system, npcConfig.ModelId.ToString(), characterDTO.name, characterDTO.description);
            
            HashSet<BuildingData.BuildingType> allowedTypes = new HashSet<BuildingData.BuildingType>()
            {
                BuildingData.BuildingType.House,
                BuildingData.BuildingType.Tavern,
                BuildingData.BuildingType.Blacksmith,
                BuildingData.BuildingType.Church
            };
            npcComponent.HisBuilding = MapGenerator.Instance.GetBuilding(allowedTypes);
            var buildingData = npcComponent.HisBuilding.GetComponent<BuildingData>();
            buildingData.HisNPC = npcComponent;

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
        List<string> archetypes = gameConfig.Models.FindAll(x => x.Id != gameConfig.NarratorModelId).
            ConvertAll(x => x.Name.Substring(0,x.Name.IndexOf('.')).Replace("_", " "));

        string prompt = $"You are a creative writer. " +
                        $"Write ONLY a VALID JSON object with body specified below:\n\n" +
                        $"A short dark story (maximum 200 words) set in a medieval village. " +
                        $"The story must include a mysterious death or murder, but the victim must be a {gameConfig.Npcs.Count + 1} character not listed in the main {gameConfig.Npcs.Count}. " +
                        $"The {gameConfig.Npcs.Count} characters in the JSON section must all be alive and active during the story." +
                        $"Use a dark tone with rich sensory details (e.g., rain, silence, fear, time of day).\n" +
                        $"Use only the following five locations:\nThe church\nThe well\n" +
                        $"The house of each character in the story (Do not use any other places.)\n" +
                        $"There must be exactly {gameConfig.Npcs.Count} game characters, each with one of these personality types (archetypes):\n" +
                        $"[{string.Join(',', archetypes)}]\n\n" +
                        $"Randomly select:\n" +
                        $"One character who is the murderer (they can be guilty directly or indirectly)\n" +
                        $"One character who is a witness (they saw the murder or something suspicious)\n" +
                        $"Do not say directly who the murderer or witness is, but include subtle clues through behavior, dialogue, or physical evidence.\n" +
                        $"The story must be a single paragraph of literary narration, not a list or report.\n" +
                        $"Do not invent other characters. " +
                        $"Describe a mysterious situation with tension and uncertainty.\n\n" +
                        $"A list of the **{gameConfig.Npcs.Count}** characters with this information for each:\n" +
                            $"name (you choose)\n" +
                            $"archetype (one of the four used)\n" +
                            $"age (an integer)\n" +
                            $"description (about who they are, their personality, what they are doing during the story, how they feel, who they like or dislike, and one habit or routine they have)\n" +
                            $"murderer (boolean)\n" +
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
                                    $"}}," +
                                $"\n...\n]" +
                            $"\n}}\n";
        
        List<Message> messages = new List<Message>();
        messages.Add(new Message { role =  "user", content = prompt});

        while (true)
        {
            bool callbackCalled = false;
            string resp = null;
        
            LlmManager.Instance.Chat(gameConfig.NarratorModelId.ToString(), messages, result =>
            {
                callbackCalled = true;
                resp = result.response;
            }, (error) =>
            {
                Debug.LogError($"GameManager: GenerateHistory error: {error}");
                callbackCalled = true;
            });
        
            // Wait for the callback to be called
            while (!callbackCalled)
                yield return null;

            if (resp == null)
                continue;
            
            string strippedResp = resp.Substring(resp.IndexOf('{'));
            strippedResp = strippedResp.Substring(0, strippedResp.LastIndexOf('}') + 1);

            try
            {
                generatedHistory = JsonUtility.FromJson<GeneratedHistoryDTO>(strippedResp);
                if (string.IsNullOrWhiteSpace(generatedHistory.story))
                {
                    Debug.LogError($"GameManager: GenerateHistory error: generated history is empty");
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
        string prompt = $"You will read a short story and extract the six most important factual events. " +
                        $"Then, generate two false sentences that do not occur in the story. " +
                        $"Return your output strictly as a JSON object with two keys:\n\n" +
                        $"\"key_events\" — an array of exactly six short English sentences, each stating a concrete, factual event that actually appears in the story.\n" +
                        $"\"false_events\" — an array of exactly two short English sentences that are believable but clearly did not happen in the story.\n" +
                        $"Do not include any notes, explanation, comments, or prose. Use only concise, fact-based language.\n\n" +
                        $"Here is the story:\n" +
                        $"\"{generatedHistory.story}\"\n\n" +
                        $"Your response must be ONLY this EXACT CORRENT JSON object:\n" +
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
        messages.Add(new Message { role =  "user", content = prompt});
        
        while (true)
        {
            bool callbackCalled = false;
            string resp = null;
        
            LlmManager.Instance.Chat(gameConfig.NarratorModelId.ToString(), messages, result =>
            {
                callbackCalled = true;
                resp = result.response;
            }, (error) =>
            {
                Debug.LogError($"GameManager: ConvertHistoryToBlocks error: {error}");
                callbackCalled = true;
            });
        
            // Wait for the callback to be called
            while (!callbackCalled)
                yield return null;

            if (resp == null)
                continue;

            string strippedResp = resp.Substring(resp.IndexOf('{'));
            strippedResp = strippedResp.Substring(0, strippedResp.LastIndexOf('}') + 1);
        
            try
            {
                storyBlocks = JsonUtility.FromJson<ConvertHistoryToBlocksDTO>(strippedResp);
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManager: Error parsing generated history blocks output: {e.Message}:\nFull response:{resp}\n\nStripped response:{strippedResp}");
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
            x += $"{npc.EntityID} Pos: {npc.transform.position} , Name: ({npc.NpcName}) System: ({npc.GetDecisionSystem()}: {npc.GetCurrentDecision().PrettyName}), Hunger: {npc.Hunger}, Thirst: {npc.Thirst}\nObtained Memories:{string.Join("\n", npc.ObtainedMemories)}\nSystem Prompt: {npc.SystemPrompt}\n";
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
}
