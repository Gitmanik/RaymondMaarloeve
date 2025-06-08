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

    public NPC murdererNPC;

    public GameConfig gameConfig { get; private set; }

    [SerializeField] private GameObject uiGameObject;
    

    
    IEnumerator Start()
    {
        Debug.Log("GameManager: Start initialization");
        Instance = this;

        gameConfig = GameConfig.LoadGameConfig(Path.Combine(Application.dataPath, "game_config.json"));
        Debug.Log("GameManager: Config loaded");

        Screen.SetResolution(gameConfig.GameWindowWidth, gameConfig.GameWindowHeight, gameConfig.FullScreen);
        Application.targetFrameRate = 60;

        LlmManager.Instance.Setup(gameConfig.LlmServerApi);
        Debug.Log("GameManager: LLM Setup started");

        // Wait for LLM connection and model loading
        yield return StartCoroutine(WaitForLlmConnection());


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

            var tmpSystemPrompt =
                "Your name is Wilfred von Rabenstein. You are a fallen knight, a drunkard, and a man whose name was once spoken with reverence, now drowned in ale and regret. You are 42 years old. You are undesirable in most places, yet your blade still holds value for those desperate enough to hire a ruined man. It is past midnight. You are slumped against the wall of a rundown tavern, the rain mixing with the stale stench of cheap wine on your cloak. You know the filth of the city—the beggars, the whores, the men who whisper in shadows. You drink every night until the world blurs, until the past feels like a dream. You speak with the slurred grace of a man who once addressed kings but now bargains for pennies.";

            if (string.IsNullOrEmpty(npcModelPath))
            {
                Debug.LogError($"Model path not found for NPC with ID {npcConfig.ModelId}");
                npcComponent.Setup(new NullDecisionSystem(), null, $"Npc-{npcConfig.Id}", tmpSystemPrompt
                );
            }
            else
            {
                Debug.Log($"Game Manager: NPC {npcConfig.Id} Model Path: {npcModelPath}");
                npcComponent.Setup(new LlmDecisionMaker(), npcConfig.ModelId.ToString(), $"Npc-{npcConfig.Id}", tmpSystemPrompt);
            }
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
        }
        
        murdererNPC = npcs[Random.Range(0, npcs.Count)];
    }

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

                int modelsToLoad = usedModels.Count;
                bool[] loaded = new bool[modelsToLoad];

                for (int i = 0; i < usedModels.Count; i++)
                {
                    Debug.Log($"GameManager: Loading model number {i+1} from path {usedModels[i].Path}");
                    int idx = i;
                    var model = usedModels[i];
                    LlmManager.Instance.LoadModel(model.Id.ToString(), model.Path, (dto) =>
                    {
                        loaded[idx] = true;
                        LlmManager.Instance.GenericComplete(dto);
                    }, Debug.LogError);
                }

                // Wait until all models are loaded
                while (loaded.Any(l => !l))
                    yield return null;

                LlmServerReady = true;
                Debug.Log("GameManager: All models loaded, LlmServerReady = TRUE");
                break;
            }
            else
            {
                Debug.LogError("GameManager: Failed to connect to LLM Server. Retrying in 5 seconds...");
                yield return new WaitForSeconds(5f);
            }
        }

        murdererNPC = npcs[Random.Range(0, npcs.Count)];

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
