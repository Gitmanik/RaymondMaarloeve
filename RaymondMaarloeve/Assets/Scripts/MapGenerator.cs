using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Generates the map with separated responsibilities for buildings, walls, and decorations.
/// Handles terrain resizing, tile initialization, structure placement, and NavMesh generation.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    /// <summary> Singleton instance of the map generator. </summary>
    public static MapGenerator Instance { get; private set; }
    public bool IsMapGenerated { get; private set; } = false;

    [Header("Base Map Configuration")]
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

    private Tile[,] tiles;
    private List<Tile> allTiles = new();
    private GameObject wallsRoot;

    [HideInInspector] public List<Tile> buildingsMainTile;
    [HideInInspector] public List<GameObject> spawnedBuildings;
    [HideInInspector] public int mapWidth, mapLength;

    /// <summary>
    /// Initializes terrain and tile grid during object creation.
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
    /// Entry point for generating the entire map.
    /// </summary>
    public void GenerateMap()
    {
        Debug.Log("MapGenerator: Starting map generation");
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;
        Debug.Log($"MapGenerator: Map size: {mapWidth}x{mapLength}");

        buildingTiles = new List<Tile>();

        InitializeTiles();

        var wallSpawner = new WallSpawner(walls, terrain, tileSize, mapWidthInTiles, mapLengthInTiles);
        wallsRoot = wallSpawner.SpawnWalls(tiles);

        var buildingSpawner = new BuildingSpawner(terrain, tileSize, mapWidthInTiles, mapLengthInTiles, WallsMargin);
        spawnedBuildings = buildingSpawner.SpawnBuildings(tiles, allTiles, buildings);

        PathGenerator.GeneratePaths(tiles, buildingsMainTile, terrain);

        var decorationSpawner = new DecorationSpawner(terrain, tileSize);
        decorationSpawner.SpawnDecorations(tiles, allTiles, decorations);

        foreach (var tile in buildingsMainTile)
        {
            Debug.LogWarning($"{tile.Building.name} building tile at {tile.GridPosition}");
        }

        if (markTiles)
            MarkTiles();

        surface.BuildNavMesh();
    }

    /// <summary>
    /// Initializes tile data including center positions and neighbors.
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
    /// Returns a building GameObject that matches one of the allowed types and is not yet assigned to an NPC.
    /// </summary>
    public GameObject GetBuilding(HashSet<BuildingData.BuildingType> allowedTypes)
    {
        var options = spawnedBuildings.FindAll(go => {
            var bd = go.GetComponent<BuildingData>();
            return bd != null && allowedTypes.Contains(bd.HisType) && bd.HisNPC == null;
        });
        return options.Count > 0 ? options[Random.Range(0, options.Count)] : null;
    }

    /// <summary>
    /// Paints borders of non-building tiles with a debug texture layer.
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
                    for (int t = 0; t < thickness; t++)
                    {
                        SetAlphaSafe(alphas, x, startZ + t, 2, layers);
                        SetAlphaSafe(alphas, x, endZ - t, 2, layers);
                    }

                for (int z = startZ; z <= endZ; z++)
                    for (int t = 0; t < thickness; t++)
                    {
                        SetAlphaSafe(alphas, startX + t, z, 2, layers);
                        SetAlphaSafe(alphas, endX - t, z, 2, layers);
                    }
            }
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    /// <summary>
    /// Safely paints a specific texture layer on the alpha map.
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
    /// Logs all tile states to the console for debugging purposes.
    /// </summary>
    [ContextMenu("Log tile states to console")]
    public void DebugTilesInConsole()
    {
        for (int z = mapLengthInTiles - 1; z >= 0; z--)
        {
            string row = "";
            for (int x = 0; x < mapWidthInTiles; x++)
            {
                row += tiles[x, z].IsBuilding ? "[X]" : "[ ]";
            }
            Debug.Log($"Row Z={z}: {row}");
        }
    }
}

/// <summary>
/// Represents a single grid unit (tile) on the generated map.
/// Stores position, state flags, and neighbor references.
/// </summary>
public class Tile
{
    /// <summary> Grid coordinates (X,Z) of the tile. </summary>
    public Vector2Int GridPosition;

    /// <summary> Center position of the tile in world coordinates (X,Z). </summary>
    public Vector2 TileCenter;

    /// <summary> Center position of the tile's front side in world coordinates (used for wall placement). </summary>
    public Vector2 FrontWallCenter;

    /// <summary> Neighboring tiles (up to 4 directions). </summary>
    public Tile[] Neighbors = new Tile[0];

    /// <summary> The GameObject (building) occupying this tile, if any. </summary>
    public GameObject Building;

    /// <summary> Indicates if this tile is the central tile for a building. </summary>
    public bool IsBuilding = false;

    /// <summary> Indicates if this tile is part of any building's footprint. </summary>
    public bool IsPartOfBuilding = false;

    /// <summary> Indicates if this tile is part of a path. </summary>
    public bool IsPath = false;
}

/// <summary>
/// Serializable structure representing a building prefab and its generation parameters.
/// Used by MapGenerator during procedural placement.
/// </summary>
[Serializable]
public class BuildingSetup
{
    /// <summary> The prefab GameObject to instantiate as a building. </summary>
    public GameObject prefab;

    /// <summary> Probability weight for this building to be selected during placement (0–1). </summary>
    [Range(0f, 1f)]
    public float weight = 0.1f;

    /// <summary> Maximum number of times this building can appear on the map. </summary>
    public int maxCount = 3;

    /// <summary> Tracks how many times this building has already been placed. </summary>
    [HideInInspector]
    public int currentCount = 0;
}

/// <summary>
/// Serializable wrapper for a wall prefab. Used by WallSpawner to build perimeter walls.
/// </summary>
[Serializable]
public class WallsSetup
{
    /// <summary> The prefab GameObject to instantiate as a wall segment. </summary>
    public GameObject prefab;
}
