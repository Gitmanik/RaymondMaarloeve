using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int npcCount = 6;
    public GameObject[] npcPrefabs;
    
    void Start()
    {
        Debug.Log("Game Manager starting");
        Application.targetFrameRate = 60;
        
        TerrainGenerator.Instance.GenerateMap();
        
        List<GameObject> npcs = npcPrefabs.ToList();
        
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 npcPosition = new Vector3(TerrainGenerator.Instance.transform.position.x - TerrainGenerator.Instance.width/2 + Random.Range(0, TerrainGenerator.Instance.width), 0, TerrainGenerator.Instance.transform.position.z - TerrainGenerator.Instance.height/2 + Random.Range(0, TerrainGenerator.Instance.height));
            
            int npcVariant = Random.Range(0, npcs.Count);
            GameObject newNpc = Instantiate(npcs[npcVariant], npcPosition, Quaternion.identity);
            npcs.RemoveAt(npcVariant);
            newNpc.GetComponent<NPC>().Setup();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
