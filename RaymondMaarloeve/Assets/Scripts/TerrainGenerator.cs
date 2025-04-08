using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class BuildingConfig
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 10;

    [HideInInspector] public int currentCount = 0;
}

public enum TileType
{
    Empty,
    Building,
    Road
}

public class Tile
{
    public Vector2Int gridPos;
    public Vector3 centerWorld;
    public Vector3 rightEdgeWorld;

    public GameObject building;
    public Tile north, south, east, west;

    public TileType Type { get; private set; } = TileType.Empty;
    public bool IsEmpty => Type == TileType.Empty;

    public Tile(Vector2Int gridPos, Vector3 terrainOrigin, int tileSize)
    {
        this.gridPos = gridPos;

        centerWorld = new Vector3(
            terrainOrigin.x + gridPos.x + tileSize / 2f,
            0,
            terrainOrigin.z + gridPos.y + tileSize / 2f
        );

        rightEdgeWorld = centerWorld + new Vector3(tileSize / 2f, 0, 0);
    }

    public void SetBuilding(GameObject building)
    {
        this.building = building;
        Type = TileType.Building;
        Debug.Log($"Tile {gridPos} -> budynek przypisany");

    }

    public void SetAsRoad()
    {
        if (Type == TileType.Empty)
            Type = TileType.Road;
    }

    public void SetNeighbors(Dictionary<Vector2Int, Tile> tiles)
    {
        tiles.TryGetValue(gridPos + Vector2Int.up, out north);
        tiles.TryGetValue(gridPos + Vector2Int.down, out south);
        tiles.TryGetValue(gridPos + Vector2Int.right, out east);
        tiles.TryGetValue(gridPos + Vector2Int.left, out west);
    }
}

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; private set; }

    public NavMeshSurface surface;
    public Terrain terrain;

    public int width = 100, height = 100;
    public int buildingArea = 10;

    [Range(0f, 1f)]
    public float density = 0.2f;

    public List<BuildingConfig> buildingConfigs = new();
    public List<GameObject> spawnedBuildings = new();

    private float[,,] baseAlphaMap;
    private float[,,] currentAlphaMap;

    private Dictionary<Vector2Int, Tile> _tiles;

    void Awake()
    {
        Instance = this;
    }

    public void GenerateMap()
    {
        Debug.Log("Generating map");

        width = (int)Math.Floor(terrain.terrainData.size.x);
        height = (int)Math.Floor(terrain.terrainData.size.z);

        spawnedBuildings.Clear();
        foreach (var config in buildingConfigs)
            config.currentCount = 0;

        // Inicjalizacja siatki Tile
        _tiles = new Dictionary<Vector2Int, Tile>();
        for (int x = 0; x < width; x += buildingArea)
            for (int z = 0; z < height; z += buildingArea)
                _tiles[new Vector2Int(x, z)] = new Tile(new Vector2Int(x, z), terrain.transform.position - new Vector3(width / 2f, 0, height / 2f), buildingArea);

        List<Vector2Int> gridPositions = new(_tiles.Keys);
        Shuffle(gridPositions);

        foreach (var gridPos in gridPositions)
        {
            if (Random.value > density) continue;

            float offsetX = Random.Range(1f, buildingArea - 1f);
            float offsetZ = Random.Range(1f, buildingArea - 1f);

            Vector3 position = new Vector3(
                transform.position.x - width / 2 + gridPos.x + buildingArea / 2f,
                0,
                transform.position.z - height / 2 + gridPos.y + buildingArea / 2f
            );


            GameObject prefab = PickBuilding();
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, position, Quaternion.identity, terrain.transform);
            spawnedBuildings.Add(go);

            Vector2Int tilePos = GetTileGridPos(position);
            if (_tiles.TryGetValue(tilePos, out var tile))
            {
                tile.SetBuilding(go);
            }
            else
            {
                Debug.LogWarning($"Brak tile dla pozycji budynku: {position} -> tilePos: {tilePos}");
            }

        }

        foreach (var tile in _tiles.Values)
            tile.SetNeighbors(_tiles);

        baseAlphaMap = CreateCleanAlphaMap();
        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);

        DrawPathsBetweenBuildings();
        DrawPathsFromTiles();
        surface.BuildNavMesh();
    }

    #region Budynki

    private GameObject PickBuilding()
    {
        List<BuildingConfig> available = buildingConfigs.FindAll(b => b.currentCount < b.maxCount);
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

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public void ClearBuildings()
    {
        foreach (var go in spawnedBuildings)
            DestroyImmediate(go);
        spawnedBuildings.Clear();
    }

    #endregion

    #region Ścieżki

    private void DrawPathsBetweenBuildings()
    {
        TerrainData terrainData = terrain.terrainData;

        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        int numTextures = terrainData.alphamapLayers;

        if (numTextures < 2)
        {
            Debug.LogWarning("Potrzebujesz co najmniej 2 tekstur w Terrain Layers: np. trawa i ścieżka.");
            return;
        }

        currentAlphaMap = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

        foreach (var from in spawnedBuildings)
        {
            GameObject to = GetClosestBuilding(from);
            if (to == null) continue;

            Vector3 start = WorldToAlphaMapCoord(from.transform.position, terrainData);
            Vector3 end = WorldToAlphaMapCoord(to.transform.position, terrainData);

            int steps = 100;
            float pathWidth = 3f;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 pos = Vector3.Lerp(start, end, t);
                PaintCircleOnAlphaMap(currentAlphaMap, (int)pos.x, (int)pos.z, pathWidth, 1, numTextures);
            }
        }

        terrainData.SetAlphamaps(0, 0, currentAlphaMap);
    }

    private void DrawPathsFromTiles()
    {
        TerrainData terrainData = terrain.terrainData;
        int numTextures = terrainData.alphamapLayers;

        if (numTextures < 2)
        {
            Debug.LogWarning("Potrzebujesz co najmniej 2 tekstur w Terrain Layers.");
            return;
        }

        foreach (var tile in _tiles.Values)
        {
            if (tile.IsEmpty) continue;

            Vector3 start = WorldToAlphaMapCoord(tile.centerWorld, terrainData);
            Vector3 end = WorldToAlphaMapCoord(tile.rightEdgeWorld, terrainData);

            int steps = 30;
            float pathWidth = 2f;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 pos = Vector3.Lerp(start, end, t);
                PaintCircleOnAlphaMap(currentAlphaMap, (int)pos.x, (int)pos.z, pathWidth, 1, numTextures);

                // Opcjonalne oznaczenie Tile jako droga
                Vector3 worldPos = AlphaMapToWorldCoord((int)pos.x, (int)pos.z, terrainData);
                Vector2Int tilePos = GetTileGridPos(worldPos);
                if (_tiles.TryGetValue(tilePos, out var targetTile))
                    targetTile.SetAsRoad();
            }
        }

        terrainData.SetAlphamaps(0, 0, currentAlphaMap);
    }

    private Vector3 WorldToAlphaMapCoord(Vector3 worldPos, TerrainData data)
    {
        Vector3 terrainPos = worldPos - terrain.transform.position;
        float x = (terrainPos.x / data.size.x) * data.alphamapWidth;
        float z = (terrainPos.z / data.size.z) * data.alphamapHeight;
        return new Vector3(x, 0, z);
    }

    private Vector3 AlphaMapToWorldCoord(int x, int z, TerrainData data)
    {
        float worldX = (x / (float)data.alphamapWidth) * data.size.x + terrain.transform.position.x;
        float worldZ = (z / (float)data.alphamapHeight) * data.size.z + terrain.transform.position.z;
        return new Vector3(worldX, 0, worldZ);
    }

    private Vector2Int GetTileGridPos(Vector3 worldPos)
    {
        float offsetX = worldPos.x - transform.position.x + width / 2f;
        float offsetZ = worldPos.z - transform.position.z + height / 2f;

        int gridX = Mathf.FloorToInt(offsetX / buildingArea) * buildingArea;
        int gridZ = Mathf.FloorToInt(offsetZ / buildingArea) * buildingArea;

        return new Vector2Int(gridX, gridZ);
    }

    private GameObject GetClosestBuilding(GameObject from)
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var other in spawnedBuildings)
        {
            if (other == from) continue;
            float dist = Vector3.Distance(from.transform.position, other.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = other;
            }
        }

        return closest;
    }

    private void PaintCircleOnAlphaMap(float[,,] alphas, int centerX, int centerZ, float radius, int texIndex, int numTextures)
    {
        int r = Mathf.CeilToInt(radius);
        for (int z = -r; z <= r; z++)
        {
            for (int x = -r; x <= r; x++)
            {
                int px = centerX + x;
                int pz = centerZ + z;
                if (px < 0 || pz < 0 || px >= alphas.GetLength(0) || pz >= alphas.GetLength(1)) continue;

                if (x * x + z * z <= radius * radius)
                {
                    for (int i = 0; i < numTextures; i++)
                        alphas[px, pz, i] = (i == texIndex) ? 1f : 0f;
                }
            }
        }
    }

    private float[,,] CreateCleanAlphaMap()
    {
        TerrainData data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        float[,,] alphaMap = new float[w, h, layers];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                alphaMap[x, y, 0] = 1f;

        return alphaMap;
    }

    public void ResetTerrainToBase()
    {
        if (baseAlphaMap == null)
        {
            Debug.LogWarning("baseAlphaMap jest null – nie utworzono go wcześniej.");
            return;
        }

        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
        Debug.Log("Teren przywrócony do trawy.");
    }

    #endregion
}
