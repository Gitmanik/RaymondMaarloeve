using System;
using System.Collections.Generic;
using System.Linq;
using Gitmanik.Console;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int npcCount = 6;
    public GameObject[] npcPrefabs;
    
    private int entityIDCounter = 0;
    public int GetEntityID() => ++entityIDCounter;
    
    public List<NPC> npcs = new List<NPC>();
    
    void Start()
    {
        
        Debug.Log("Game Manager starting");
        Application.targetFrameRate = 60;
        Instance = this;
        
        // TODO: Prawdopodobnie tutaj konfiguracja połączenia z serwerem LLM
        // Będzie wymagany centralny system tych systemów, żeby korzystały z jednego połączenia sieciowego?
        
        TerrainGenerator.Instance.GenerateMap();

        List<GameObject> npcPrefabsList = npcPrefabs.ToList();
        
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 npcPosition = new Vector3(TerrainGenerator.Instance.transform.position.x - TerrainGenerator.Instance.width/2 + Random.Range(0, TerrainGenerator.Instance.width), 0, TerrainGenerator.Instance.transform.position.z - TerrainGenerator.Instance.height/2 + Random.Range(0, TerrainGenerator.Instance.height));
            
            int npcVariant = Random.Range(0, npcPrefabsList.Count);
            GameObject newNpc = Instantiate(npcPrefabsList[npcVariant], npcPosition, Quaternion.identity);
            npcPrefabsList.RemoveAt(npcVariant);
            
            // TODO: Prawdopodobnie tutaj konfiguracja pamięci NPC
            // TODO: Przekazać tutaj obiekt IDecisionSystem połączony z LLM z fabryki
            var npcComponent = newNpc.GetComponent<NPC>(); 
            npcComponent.Setup(new RandomDecisionMaker());
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
            x += $"{npc.EntityID} Pos: {npc.transform.position} , Name: ({npc.npcName}) System: ({npc.GetDecisionSystem()}, {npc.GetCurrentDecision()})\n";
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
