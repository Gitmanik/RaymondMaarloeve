using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for generating the game map, including terrain layout, building placement, wall construction, decoration, and clues.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the MapGenerator.
    /// </summary>
    public static MapGenerator Instance { get; private set; }

    /// <summary>
    /// Indicates whether the map has already been generated.
    /// </summary>
    public bool IsMapGenerated { get; private set; } = false;

    [Header("Map configuration")]
    public NavMeshSurface surface;
    public Terrain terrain;
    public int tileSize = 10;
    public int mapWidthInTiles = 10, mapLengthInTiles = 10;
    [Range(0f, 1f)] public float buildingsDensity = 0.2f;
    public bool markTiles = false;
    public int WallsMargin = 10;

    [Header("Prefabs")]
    public List<BuildingSetup> buildings = new();
    public List<WallsSetup> walls = new();
    public List<BuildingSetup> decorations = new();
    public List<ClueSetup> clues = new();

    private Tile[,] tiles;
    private List<Tile> allTiles = new();
    private GameObject wallsRoot;

    [HideInInspector] public List<Tile> buildingsMainTile;
    [HideInInspector] public List<GameObject> spawnedBuildings;
    [HideInInspector] public int mapWidth, mapLength;

    /// <summary>
    /// Initializes the tile array and terrain properties.
    /// </summary>
    void Awake()
    {
        Debug.Log("MapGenerator: Awake started");
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];
        Debug.Log($"MapGenerator: Initialized tiles array [{mapWidthInTiles}x{mapLengthInTiles}]");

        PathGenerator.ClearMap(terrain);

        Vector3 newSize = new Vector3(mapWidthInTiles * tileSize, terrain.terrainData.size.y, mapLengthInTiles * tileSize);
        terrain.terrainData.size = newSize;

        Vector3 centerOffset = new Vector3(mapWidthInTiles * tileSize / 2f, 0, mapLengthInTiles * tileSize / 2f);
        terrain.transform.position = transform.position - centerOffset;
    }

    /// <summary>
    /// Generates the full map including walls, buildings, decorations, and paths.
    /// </summary>
    public void GenerateMap()
    {
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;
        buildingsMainTile = new();
        spawnedBuildings = new();

        InitializeTiles();

        var wallSpawner = new WallSpawner(walls, terrain, tileSize, mapWidthInTiles, mapLengthInTiles);
        wallsRoot = wallSpawner.SpawnWalls(tiles);

        var buildingSpawner = new BuildingSpawner(terrain, tileSize, mapWidthInTiles, mapLengthInTiles, WallsMargin);
        spawnedBuildings = buildingSpawner.SpawnBuildings(tiles, allTiles, buildings);

        PathGenerator.GeneratePaths(tiles, buildingsMainTile, terrain);

        var decorationSpawner = new DecorationSpawner(terrain, tileSize);
        decorationSpawner.SpawnDecorations(tiles, allTiles, decorations);

        if (markTiles)
            MarkTiles();

        surface.BuildNavMesh();
        IsMapGenerated = true;
        Debug.Log("MapGenerator: Map generated, IsMapGenerated = TRUE");
    }

    /// <summary>
    /// Spawns clues randomly on the generated map.
    /// </summary>
    public void GenerateClue()
    {
        var clueSpawner = new ClueSpawner(terrain, tileSize);
        clueSpawner.SpawnClues(tiles, allTiles, clues);

        surface.BuildNavMesh();
        Debug.Log("MapGenerator: Clue spawned");
    }

    /// <summary>
    /// Initializes the tile grid and their neighbors.
    /// </summary>
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

    /// <summary>
    /// Returns a suitable building for an NPC from allowed types.
    /// </summary>
    /// <param name="allowedTypes">Set of allowed building types.</param>
    /// <returns>Available building GameObject or null.</returns>
    public GameObject GetBuilding(HashSet<BuildingData.BuildingType> allowedTypes)
    {
        var options = spawnedBuildings.FindAll(go => {
            var bd = go.GetComponent<BuildingData>();
            return bd != null && allowedTypes.Contains(bd.HisType) && bd.HisNPC.Count <= 2;
        });
        return options.Count > 0 ? options[Random.Range(0, options.Count)] : null;
    }

    /// <summary>
    /// Marks unused tiles visually on the terrain using a specific texture layer.
    /// </summary>
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

    /// <summary>
    /// Sets the alpha texture layer value safely for a given tile coordinate.
    /// </summary>
    private void SetAlphaSafe(float[,,] alphas, int x, int z, int layer, int layersCount)
    {
        if (z < 0 || z >= alphas.GetLength(0) || x < 0 || x >= alphas.GetLength(1))
            return;

        for (int i = 0; i < layersCount; i++)
            alphas[z, x, i] = 0f;

        alphas[z, x, layer] = 1f;
    }

    /// <summary>
    /// Prints tile layout information to the Unity console for debugging purposes.
    /// </summary>
    [ContextMenu("Debug tiles in console")]
    public void DebugTilesInConsole()
    {
        for (int z = mapLengthInTiles - 1; z >= 0; z--)
        {
            string row = "";
            for (int x = 0; x < mapWidthInTiles; x++)
            {
                row += tiles[x, z].IsBuilding ? "[X]" : "[ ]";
            }
            Debug.Log($"MapGenerator: Row Z={z}: {row}");
        }
    }
}

/// <summary>
/// Represents a single map tile with spatial and classification metadata.
/// </summary>
public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 FrontWallCenter;
    public Tile[] Neighbors = new Tile[0];
    public GameObject Prefab;
    public bool IsBuilding = false;
    public bool IsPath = false;
    public bool IsPartOfBuilding = false;
    public bool IsClue = false;
    public bool IsDecoration = false;
}

/// <summary>
/// Configuration for building prefabs with weighting and limits.
/// </summary>
[Serializable]
public class BuildingSetup
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    [HideInInspector] public int currentCount = 0;
}

/// <summary>
/// Configuration for wall prefabs.
/// </summary>
[Serializable]
public class WallsSetup
{
    public GameObject prefab;
}

/// <summary>
/// Configuration for clue prefabs.
/// </summary>
[Serializable]
public class ClueSetup
{
    public GameObject prefab;
}
