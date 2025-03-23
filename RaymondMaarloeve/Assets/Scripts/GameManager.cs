using System;
using System.Collections.Generic;
using System.Linq;
using Gitmanik.Console;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int npcCount = 6;
    public GameObject[] npcPrefabs;
    
    void Start()
    {
        Debug.Log("Game Manager starting");
        Application.targetFrameRate = 60;
        Instance = this;
        
        // TODO: Prawdopodobnie tutaj konfiguracja połączenia z serwerem LLM
        // Będzie wymagany centralny system tych systemów, żeby korzystały z jednego połączenia sieciowego?
        
        TerrainGenerator.Instance.GenerateMap();
        
        List<GameObject> npcs = npcPrefabs.ToList();
        
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 npcPosition = new Vector3(TerrainGenerator.Instance.transform.position.x - TerrainGenerator.Instance.width/2 + Random.Range(0, TerrainGenerator.Instance.width), 0, TerrainGenerator.Instance.transform.position.z - TerrainGenerator.Instance.height/2 + Random.Range(0, TerrainGenerator.Instance.height));
            
            int npcVariant = Random.Range(0, npcs.Count);
            GameObject newNpc = Instantiate(npcs[npcVariant], npcPosition, Quaternion.identity);
            npcs.RemoveAt(npcVariant);
            
            // TODO: Prawdopodobnie tutaj konfiguracja pamięci NPC
            // TODO: Przekazać tutaj obiekt IDecisionSystem połączony z LLM z fabryki
            newNpc.GetComponent<NPC>().Setup(new RandomDecisionMaker());
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
}
