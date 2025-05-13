//MapGenerator.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
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

    public List<BuildingSetup> buildings = new();
    public List<GameObject> spawnedBuildings = new();

    private List<Tile> buildingTiles;

    private float[,,] baseAlphaMap;
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapLength;

    void Awake()
    {
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        PathGenerator.ClearMap(terrain);
        // zapamiętanie oryginalnych alphamap
        baseAlphaMap = terrain.terrainData.GetAlphamaps(0, 0,
            terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

        // rozmiar terenu
        Vector3 newSize = new Vector3(mapWidthInTiles * tileSize,
            terrain.terrainData.size.y, mapLengthInTiles * tileSize);
        terrain.terrainData.size = newSize;

        // wyśrodkowanie terenu względem obiektu
        Vector3 centerOffset = new Vector3(mapWidthInTiles * tileSize / 2f,
            0, mapLengthInTiles * tileSize / 2f);
        terrain.transform.position = transform.position - centerOffset;
    }

    public void GenerateMap()
    {
        Debug.Log("Generating map");
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;
        buildingTiles = new List<Tile>();

        // 1) Inicjalizacja kafelków
        for (int x = 0; x < mapWidthInTiles; x++)
            for (int z = 0; z < mapLengthInTiles; z++)
                tiles[x, z] = new Tile();

        // 2) Ustawienie pozycji, sąsiadów i ewentualny spawn budynków
        for (int x = 0; x < mapWidthInTiles; x++)
            for (int z = 0; z < mapLengthInTiles; z++)
            {
                var tile = tiles[x, z];
                tile.GridPosition = new Vector2Int(x, z);
                tile.TileCenter = new Vector2(
                    transform.position.x - mapWidth / 2 + x * tileSize + tileSize / 2f,
                    transform.position.z - mapLength / 2 + z * tileSize + tileSize / 2f);

                tile.PosXWallCenter = new Vector2(
                    transform.position.x - mapWidth / 2 + x * tileSize + tileSize,
                    transform.position.z - mapLength / 2 + z * tileSize + tileSize / 2f);

                var neighbours = new List<Tile>();
                if (x > 0) neighbours.Add(tiles[x - 1, z]);
                if (z > 0) neighbours.Add(tiles[x, z - 1]);
                if (x < mapWidthInTiles - 1) neighbours.Add(tiles[x + 1, z]);
                if (z < mapLengthInTiles - 1) neighbours.Add(tiles[x, z + 1]);
                tile.Neighbors = neighbours.ToArray();

                // spawn budynku z prawdopodobieństwem buildingsDensity
                if (Random.value <= buildingsDensity)
                {
                    var prefab = PickBuilding();
                    if (prefab != null)
                    {
                        Vector3 pos = new Vector3(
                            transform.position.x - mapWidth / 2 + x * tileSize + tileSize / 2f,
                            0,
                            transform.position.z - mapLength / 2 + z * tileSize + tileSize / 2f);

                        var go = Instantiate(prefab, pos, Quaternion.identity, terrain.transform);
                        spawnedBuildings.Add(go);
                        tile.IsBuilding = true;
                        tile.Building = go;
                        var buildingData = go.GetComponent<BuildingData>();

                        if (buildingData != null)
                        {
                            buildingData.HisTile = tile;
                            if (buildingData.HisTile == tile)
                            {
                                Debug.LogWarning($"✅ Tile poprawnie przypisany do budynku: {tile}");
                            }
                            else
                            {
                                Debug.LogWarning("⚠️ Tile NIE został poprawnie przypisany!");
                            }

                        }

                        buildingTiles.Add(tile);

                    }
                }
            }

        // 3) Wyznaczanie i rysowanie ścieżek w osobnym managerze
        PathGenerator.GeneratePaths(tiles, buildingTiles, terrain);

        // 4) Budowa NavMesh dla całej mapy
        surface.BuildNavMesh();
    }

    private GameObject PickBuilding()
    {
        var available = buildings.FindAll(b => b.currentCount < b.maxCount);
        if (available.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var b in available) totalWeight += b.weight;

        float rnd = Random.value * totalWeight;
        float cum = 0f;
        foreach (var b in available)
        {
            cum += b.weight;
            if (rnd <= cum)
            {
                b.currentCount++;
                return b.prefab;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        // przy zniszczeniu przywracamy oryginalne alphamapy
        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    }
}

// Klasa Tile i Building pozostają bez zmian
public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 PosXWallCenter;
    public Tile[] Neighbors = new Tile[0];
    public GameObject TileObject;
    public GameObject Building;
    public bool IsBuilding = false;
    public bool IsPath = false;
}

[Serializable]
public class BuildingSetup
{
    public GameObject prefab;
    public GameObject OwnerNPC = null;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    public int occurenceRadius = 5;

    [HideInInspector] public int currentCount = 0;
}
