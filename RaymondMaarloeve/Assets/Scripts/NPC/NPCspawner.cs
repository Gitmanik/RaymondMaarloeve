using System.Collections.Generic;
using UnityEngine;

public class NPCspawner : MonoBehaviour
{
    public static List<NPC> SpawnNPCs(List<GameObject> npcPrefabs, int npcCount)
    {
        // --- Przygotowanie listy budynków prywatnych ---
        // --- przygotowanie listy GameObjectów budynków ---
        var allowedTypes = new HashSet<BuildingData.BuildingType>
        {
            BuildingData.BuildingType.House,
            BuildingData.BuildingType.Tavern,
            BuildingData.BuildingType.Blacksmith,
            BuildingData.BuildingType.Church
        };

        List<GameObject> buildings = new List<GameObject>();

        foreach (GameObject go in MapGenerator.Instance.spawnedBuildings)
        {
            var buildingdata = go.GetComponent<BuildingData>();
            if (buildingdata != null && allowedTypes.Contains(buildingdata.HisType))
            {
                buildings.Add(go); // <- dodajesz ca³y GameObject
            }
        }

        // --- Tasowanie budynków ---
        for (int i = 0; i < buildings.Count; i++)
        {
            int j = Random.Range(i, buildings.Count);
            var tmp = buildings[i];
            buildings[i] = buildings[j];
            buildings[j] = tmp;
        }

        // --- Tasowanie prefabów NPC ---
        var prefabs = new List<GameObject>(npcPrefabs);
        for (int i = 0; i < prefabs.Count; i++)
        {
            int j = Random.Range(i, prefabs.Count);
            var tmp = prefabs[i];
            prefabs[i] = prefabs[j];
            prefabs[j] = tmp;
        }

        // --- Lista wynikowa NPC ---
        var result = new List<NPC>(npcCount);
        //private GameObject
        // --- Spawnowanie NPC ---
        for (int i = 0; i < npcCount; i++)
        {
            // Wybór budynku (jeœli jest)
            GameObject chosenBuilding = null;
            if (i < buildings.Count)
                chosenBuilding = buildings[i];

            // Wybór prefabrykatów NPC (modulo liczba prefabów)
            int prefabIdx = i % prefabs.Count;
            var go = Instantiate(prefabs[prefabIdx]);
            var chosenBuildingData = chosenBuilding.GetComponent<BuildingData>();
            // Jeœli budynek zosta³ przydzielony, spawn przy budynku
            if (chosenBuilding != null)
            {
                var coords = chosenBuildingData.HisTile.FrontWallCenter;
                go.transform.position = new Vector3(coords.x, 0, coords.y);
                chosenBuildingData.HisNPC = go.GetComponent<NPC>();
            }
            else
            {
                // Jeœli nie ma budynku, spawn losowy + log b³êdu
                Debug.LogError("NPC doesn't have his building!");

                float x = go.transform.position.x - MapGenerator.Instance.mapWidth / 2f + Random.Range(0f, MapGenerator.Instance.mapWidth);
                float z = go.transform.position.z - MapGenerator.Instance.mapLength / 2f + Random.Range(0f, MapGenerator.Instance.mapLength);
                go.transform.position = new Vector3(x, 0, z);
            }

            // Konfiguracja NPC
            var npc = go.GetComponent<NPC>();
            npc.Setup(new RandomDecisionMaker());
            npc.HisBuilding = chosenBuilding;

            result.Add(npc);
        }

        return result;
    }

}
