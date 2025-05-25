using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gitmanik.Console;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int npcCount = 6;
    public GameObject[] npcPrefabs;
    
    private int entityIDCounter = 0;
    public int GetEntityID() => ++entityIDCounter;
    
    public List<NPC> npcs = new List<NPC>();
    [HideInInspector] public bool LlmServerReady = false;
    
    public GameConfig gameConfig { get; private set; }

    [SerializeField] private GameObject uiGameObject;
    
    void Awake()
    {
        uiGameObject.SetActive(true);
    }
    
    void Start()
    {
        Debug.Log("Game Manager starting");
        Instance = this;

        
        gameConfig = GameConfig.LoadGameConfig(Path.Combine(Application.dataPath, "game_config.json"));
        
        Screen.SetResolution(gameConfig.GameWindowWidth, gameConfig.GameWindowHeight, gameConfig.FullScreen);
        Application.targetFrameRate = 60;
        
        LlmManager.Instance.Setup(gameConfig.LlmServerApi);
        LlmManager.Instance.Connect(x =>
        {
            if (x)
            {
                Debug.Log("Connected to LLM Server");
                int modelsToLoad = gameConfig.Models.Count;
                
                foreach (var model in gameConfig.Models)
                    LlmManager.Instance.LoadModel(model.Id.ToString(), model.Path, (dto) =>
                    {
                        modelsToLoad--;
                        if (modelsToLoad == 0)
                            LlmServerReady = true;
                        LlmManager.Instance.GenericComplete(dto);
                    }, Debug.LogError);
            }
            else
            {
                Debug.LogError("Failed to connect to LLM Server");
            }
        });
        
        MapGenerator.Instance.GenerateMap();
        
        List<GameObject> npcPrefabsList = npcPrefabs.ToList();
        
        

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
            npcPrefabsList.RemoveAt(npcVariant);
            var npcComponent = newNpc.GetComponent<NPC>(); 

            var tmpSystemPrompt =
                "Your name is Wilfred von Rabenstein. You are a fallen knight, a drunkard, and a man whose name was once spoken with reverence, now drowned in ale and regret. You are 42 years old. You are undesirable in most places, yet your blade still holds value for those desperate enough to hire a ruined man. It is past midnight. You are slumped against the wall of a rundown tavern, the rain mixing with the stale stench of cheap wine on your cloak. You know the filth of the city—the beggars, the whores, the men who whisper in shadows. You drink every night until the world blurs, until the past feels like a dream. You speak with the slurred grace of a man who once addressed kings but now bargains for pennies.";
            
            if (string.IsNullOrEmpty(npcModelPath)) {
                Debug.LogError($"Model path not found for NPC with ID {npcConfig.ModelId}");
                npcComponent.Setup(new RandomDecisionMaker(), null, $"Npc-{npcConfig.Id}", tmpSystemPrompt
                    );
            }
            else {
                Debug.Log($"NPC {npcConfig.Id} Model Path: {npcModelPath}");
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
            x += $"{npc.EntityID} Pos: {npc.transform.position} , Name: ({npc.NpcName}) System: ({npc.GetDecisionSystem()}, {npc.GetCurrentDecision()}), Hunger: {npc.Hunger}, Thirst: {npc.Thirst}\n";
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
