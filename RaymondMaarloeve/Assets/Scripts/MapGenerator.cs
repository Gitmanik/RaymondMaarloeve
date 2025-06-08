// Zawiera: generowanie mapy z podziałem na osobne klasy do murów, budynków i dekoracji

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    [Header("Podstawowe dane mapy")]
    public NavMeshSurface surface;
    public Terrain terrain;
    public int tileSize = 10;
    public int mapWidthInTiles = 10, mapLengthInTiles = 10;
    [Range(0f, 1f)] public float buildingsDensity = 0.2f;
    public bool markTiles = false;
    public int WallsMargin = 10;

    [Header("Prefabrykaty")]
    public List<BuildingSetup> buildings = new();
    public List<WallsSetup> walls = new();
    public List<BuildingSetup> decorations = new();

    private Tile[,] tiles;
    private List<Tile> allTiles = new();
    private GameObject wallsRoot;

    [HideInInspector] public List<Tile> buildingsMainTile;
    [HideInInspector] public List<GameObject> spawnedBuildings;
    [HideInInspector] public int mapWidth, mapLength;

    void Awake()
    {
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        PathGenerator.ClearMap(terrain);

        Vector3 newSize = new Vector3(mapWidthInTiles * tileSize, terrain.terrainData.size.y, mapLengthInTiles * tileSize);
        terrain.terrainData.size = newSize;

        Vector3 centerOffset = new Vector3(mapWidthInTiles * tileSize / 2f, 0, mapLengthInTiles * tileSize / 2f);
        terrain.transform.position = transform.position - centerOffset;
    }

    public void GenerateMap()
    {
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;
        buildingsMainTile = new();
        spawnedBuildings = new();

        InitializeTiles();

        var wallHandler = new WallSpawner(walls, terrain, tileSize, mapWidthInTiles, mapLengthInTiles);
        wallsRoot = wallHandler.SpawnWalls(tiles);
        //MarkWallTiles();

        var buildingSpawner = new BuildingSpawner(terrain, tileSize, mapWidthInTiles, mapLengthInTiles, WallsMargin);
        spawnedBuildings = buildingSpawner.SpawnBuildings(tiles, allTiles, buildings);

        var decorationSpawner = new DecorationSpawner(terrain, tileSize);
        decorationSpawner.SpawnDecorations(tiles, allTiles, decorations);
        foreach (var tile in buildingsMainTile)
        {
            Debug.LogWarning($"{tile.Building.name} building tile at {tile.GridPosition}");
        }
        PathGenerator.GeneratePaths(tiles, buildingsMainTile, terrain);

        if (markTiles)
            MarkTiles();

        surface.BuildNavMesh();
    }

    void InitializeTiles()
    {
        allTiles.Clear();
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        for (int x = 0; x < mapWidthInTiles; x++)
        {
            for (int z = 0; z < mapLengthInTiles; z++)
            {
                var tile = new Tile();
                tile.GridPosition = new Vector2Int(x, z);
                tile.TileCenter = new Vector2(
                    transform.position.x - mapWidth / 2 + x * tileSize + tileSize / 2f,
                    transform.position.z - mapLength / 2 + z * tileSize + tileSize / 2f);

                tile.FrontWallCenter = new Vector2(
                    transform.position.x - mapWidth / 2 + x * tileSize + tileSize,
                    transform.position.z - mapLength / 2 + z * tileSize + tileSize / 2f);

                tiles[x, z] = tile;
                allTiles.Add(tile);
            }
        }

        for (int x = 0; x < mapWidthInTiles; x++)
        {
            for (int z = 0; z < mapLengthInTiles; z++)
            {
                var tile = tiles[x, z];
                var neighbours = new List<Tile>();
                if (x > 0) neighbours.Add(tiles[x - 1, z]);
                if (z > 0) neighbours.Add(tiles[x, z - 1]);
                if (x < mapWidthInTiles - 1) neighbours.Add(tiles[x + 1, z]);
                if (z < mapLengthInTiles - 1) neighbours.Add(tiles[x, z + 1]);
                tile.Neighbors = neighbours.ToArray();
            }
        }
    }

    //Funkcja wyboru budynku dla NPC
    public GameObject GetBuilding(HashSet<BuildingData.BuildingType> allowedTypes)
    {
        var options = spawnedBuildings.FindAll(go => {
            var bd = go.GetComponent<BuildingData>();
            return bd != null && allowedTypes.Contains(bd.HisType) && bd.HisNPC == null;
        });
        return options.Count > 0 ? options[Random.Range(0, options.Count)] : null;
    }

    private void MarkTiles()
    {
        var data = terrain.terrainData;
        var tPos = terrain.transform.position;

        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;
        var alphas = data.GetAlphamaps(0, 0, w, h);

        float terrainWidth = data.size.x;
        float terrainHeight = data.size.z;

        int thickness = 1;
        int tilePixelW = Mathf.RoundToInt((tileSize / terrainWidth) * w);
        int tilePixelH = Mathf.RoundToInt((tileSize / terrainHeight) * h);

        foreach (var tile in tiles)
        {
            if (tile != null && !tile.IsBuilding && !tile.IsPartOfBuilding)
            {
                Vector2 center = tile.TileCenter;

                int mapX = Mathf.RoundToInt(((center.x - tPos.x) / terrainWidth) * w);
                int mapZ = Mathf.RoundToInt(((center.y - tPos.z) / terrainHeight) * h);

                int startX = mapX - tilePixelW / 2;
                int endX = startX + tilePixelW - 1;

                int startZ = mapZ - tilePixelH / 2;
                int endZ = startZ + tilePixelH - 1;

                for (int x = startX; x <= endX; x++)
                {
                    for (int t = 0; t < thickness; t++)
                    {
                        SetAlphaSafe(alphas, x, startZ + t, 2, layers);
                        SetAlphaSafe(alphas, x, endZ - t, 2, layers);
                    }
                }

                for (int z = startZ; z <= endZ; z++)
                {
                    for (int t = 0; t < thickness; t++)
                    {
                        SetAlphaSafe(alphas, startX + t, z, 2, layers);
                        SetAlphaSafe(alphas, endX - t, z, 2, layers);
                    }
                }
            }
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    private void SetAlphaSafe(float[,,] alphas, int x, int z, int layer, int layersCount)
    {
        if (z < 0 || z >= alphas.GetLength(0) || x < 0 || x >= alphas.GetLength(1))
            return;

        for (int i = 0; i < layersCount; i++)
            alphas[z, x, i] = 0f;

        alphas[z, x, layer] = 1f;
    }

    [ContextMenu("Debuguj tile'e w konsoli")]
    public void DebugTilesInConsole()
    {
        for (int z = mapLengthInTiles - 1; z >= 0; z--)
        {
            string row = "";
            for (int x = 0; x < mapWidthInTiles; x++)
            {
                row += tiles[x, z].IsBuilding ? "[X]" : "[ ]";
            }
            Debug.Log($"Rząd Z={z}: {row}");
        }
    }
}

public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 FrontWallCenter;
    public Tile[] Neighbors = new Tile[0];
    public GameObject Building;
    public bool IsBuilding = false;
    public bool IsPath = false;
    public bool IsPartOfBuilding = false;
}

[Serializable]
public class BuildingSetup
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    [HideInInspector] public int currentCount = 0;
}

[Serializable]
public class WallsSetup
{
    public GameObject prefab;
}
