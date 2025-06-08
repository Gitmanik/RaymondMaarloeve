using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gitmanik.Console;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Core manager controlling game setup, NPC initialization, and LLM connection.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Main Player")]
    public GameObject player;

    [Header("NPCs")]
    public int npcCount = 6;
    public GameObject[] npcPrefabs;
    public List<NPC> npcs = new List<NPC>();

    [Header("LLM Setup")]
    [HideInInspector] public bool LlmServerReady = false;

    [Header("Game UI")]
    [SerializeField] private GameObject uiGameObject;

    public GameConfig gameConfig { get; private set; }

    private int entityIDCounter = 0;
    public int GetEntityID() => ++entityIDCounter;

    private void Awake()
    {
        uiGameObject.SetActive(true);
    }

    private void Start()
    {
        Debug.Log("📦 Game Manager starting");
        Instance = this;

        // Load config
        gameConfig = GameConfig.LoadGameConfig(Path.Combine(Application.dataPath, "game_config.json"));
        Screen.SetResolution(gameConfig.GameWindowWidth, gameConfig.GameWindowHeight, gameConfig.FullScreen);
        Application.targetFrameRate = 60;

        // Connect to LLM Server
        LlmManager.Instance.Setup(gameConfig.LlmServerApi);
        LlmManager.Instance.Connect(success =>
        {
            if (success)
            {
                Debug.Log("✅ Connected to LLM Server");
                int modelsToLoad = gameConfig.Models.Count;

                foreach (var model in gameConfig.Models)
                {
                    LlmManager.Instance.LoadModel(model.Id.ToString(), model.Path, dto =>
                    {
                        modelsToLoad--;
                        if (modelsToLoad == 0)
                            LlmServerReady = true;
                        LlmManager.Instance.GenericComplete(dto);
                    }, Debug.LogError);
                }
            }
            else
            {
                Debug.LogError("❌ Failed to connect to LLM Server");
            }
        });

        MapGenerator.Instance.GenerateMap();
        SpawnNPCs();
        PlacePlayerAtEntrance();
    }

    /// <summary>
    /// Spawns and sets up all NPCs from the game config.
    /// </summary>
    private void SpawnNPCs()
    {
        var npcPrefabsList = npcPrefabs.ToList();

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

            string systemPrompt =
                "Your name is Wilfred von Rabenstein. You are a fallen knight, a drunkard, and a man whose name was once spoken with reverence, now drowned in ale and regret. [...]";

            if (string.IsNullOrEmpty(npcModelPath))
            {
                Debug.LogError($"🚫 Model path not found for NPC with ID {npcConfig.ModelId}");
                npcComponent.Setup(new RandomDecisionMaker(), null, $"Npc-{npcConfig.Id}", systemPrompt);
            }
            else
            {
                Debug.Log($"📦 NPC {npcConfig.Id} Model Path: {npcModelPath}");
                npcComponent.Setup(new LlmDecisionMaker(), npcConfig.ModelId.ToString(), $"Npc-{npcConfig.Id}", systemPrompt);
            }

            // Assign building to NPC
            HashSet<BuildingData.BuildingType> allowedTypes = new()
            {
                BuildingData.BuildingType.House,
                BuildingData.BuildingType.Tavern,
                BuildingData.BuildingType.Blacksmith,
                BuildingData.BuildingType.Church
            };

            npcComponent.HisBuilding = MapGenerator.Instance.GetBuilding(allowedTypes);
            npcComponent.HisBuilding.GetComponent<BuildingData>().HisNPC = npcComponent;

            npcs.Add(npcComponent);
        }
    }

    /// <summary>
    /// Places the player object at the spawn point (Gate > PlayerSpawner).
    /// </summary>
    private void PlacePlayerAtEntrance()
    {
        GameObject wallsRoot = GameObject.Find("WallsRoot");
        if (wallsRoot == null)
        {
            Debug.LogError("❌ WallsRoot not found!");
            return;
        }

        Transform gate = null;
        foreach (Transform child in wallsRoot.transform)
        {
            if (child.name == "GATE(Clone)")
            {
                gate = child;
                break;
            }
        }

        if (gate == null)
        {
            Debug.LogError("❌ Gate (GATE(Clone)) not found!");
            return;
        }

        Transform entrance = gate.Find("PlayerSpawner");
        if (entrance == null)
        {
            Debug.LogError("❌ PlayerSpawner not found!");
            return;
        }

        player.transform.position = entrance.position;
        player.transform.rotation = entrance.rotation;

        Debug.Log("✅ Player placed at Entrance.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string folderPath = "Assets/Screenshots/";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string screenshotName = "Screenshot_" + System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
            ScreenCapture.CaptureScreenshot(Path.Combine(folderPath, screenshotName));
            Debug.Log($"📸 Screenshot saved to: {folderPath + screenshotName}");
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            GitmanikConsole.singleton.ToggleConsole();
        }
    }

    #region Developer Console Commands

    [ConsoleCommand("npcs", "List all npcs")]
    public static bool ListAllNpcs()
    {
        string output = "NPCs:\n";
        foreach (var npc in Instance.npcs)
        {
            output += $"🧍 ID: {npc.EntityID}, Pos: {npc.transform.position}, Name: {npc.NpcName}, " +
                      $"System: {npc.GetDecisionSystem()}, Decision: {npc.GetCurrentDecision()}, " +
                      $"Hunger: {npc.Hunger}, Thirst: {npc.Thirst}\n";
        }
        Debug.Log(output);
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

    #endregion
}
