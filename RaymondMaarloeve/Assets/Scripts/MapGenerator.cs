using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    public NavMeshSurface surface;
    public Terrain terrain;
    public int tileSize = 10;
    public int mapWidthInTiles = 10, mapLengthInTiles = 10;
    [Range(0f, 1f)]
    public float buildingsDensity = 0.2f;

    Tile[,] tiles;

    public List<Building> buildings = new();
    public List<GameObject> spawnedBuildings = new();


    private float[,,] baseAlphaMap;
    //private float[,,] currentAlphaMap;
    public int mapWidth;
    public int mapLength;

    void Awake()
    {
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        baseAlphaMap = terrain.terrainData.GetAlphamaps(0, 0,
        terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        Vector3 newSize = new Vector3(mapWidthInTiles * tileSize, terrain.terrainData.size.y, mapLengthInTiles * tileSize);
        terrain.terrainData.size = newSize;

        Vector3 centerOffset = new Vector3(mapWidthInTiles * tileSize / 2f, 0, mapLengthInTiles * tileSize / 2f);
        terrain.transform.position = transform.position - centerOffset;

    }

    public void GenerateMap()
    {
        Debug.Log("Generating map");
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;

        for (int x = 0; x < mapWidthInTiles; x++)
        {
            for (int z = 0; z < mapLengthInTiles; z++)
            {
                tiles[x, z] = new Tile();
                tiles[x, z].GridPosition = new Vector2Int(x * tileSize, z * tileSize);
                tiles[x, z].IsBuilding = false;

                if (Random.value > buildingsDensity) continue; //spawn building decision

                Vector3 buildingPosition = new Vector3(
                    transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize / 2f,
                    0,
                    transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f
                );

                GameObject buildingPrefab = PickBuilding();
                if (buildingPrefab == null) continue; //if all bulidings spawned

                GameObject bd = Instantiate(buildingPrefab, buildingPosition, Quaternion.identity, terrain.transform);
                spawnedBuildings.Add(bd);
                tiles[x, z].IsBuilding = true;
                tiles[x, z].Building = bd;


            }
        }
        foreach (var tile in tiles)
        {
            

        }



        surface.BuildNavMesh();
    }
    private GameObject PickBuilding()
    {
        List<Building> available = buildings.FindAll(b => b.currentCount < b.maxCount);
        if (available.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var b in available) totalWeight += b.weight;

        float rnd = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var b in available)
        {
            cumulative += b.weight;
            if (rnd <= cumulative)
            {
                b.currentCount++;
                return b.prefab;
            }
        }

        return null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    }

    //private void CleanMap()
    //{
    //    terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    //}
}
public class Tile
{
    public Vector2Int GridPosition;
    public GameObject TileObject;
    public GameObject Building;
    public bool IsBuilding = false;
}

[Serializable]
public class Building
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    public int occurenceRadius = 5;
    [HideInInspector] public int currentCount = 0;
    //[HideInInspector] public Vector3 builidingCoordinates = Vector3.zero;
}

