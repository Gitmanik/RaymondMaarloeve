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

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; private set; }

    public NavMeshSurface surface;
    public Terrain terrain;

    public int width = 100, height = 100;
    public int buildingArea = 10;

    [Range(0f, 1f)]
    public float density = 0.2f;

    public List<BuildingConfig> buildingConfigs = new List<BuildingConfig>();
    public List<GameObject> spawnedBuildings = new List<GameObject>();

    private float[,,] baseAlphaMap; // czysta trawa
    private float[,,] currentAlphaMap;

    void Awake()
    {
        Instance = this;
    }

    public void GenerateMap()
    {
        Debug.Log("Generating map");

        width = (int)Math.Floor(terrain.terrainData.size.x);
        height = (int)Math.Floor(terrain.terrainData.size.z);

        TerrainData terrainData = terrain.terrainData;

        spawnedBuildings.Clear();

        foreach (var config in buildingConfigs)
            config.currentCount = 0;

        // wygeneruj i przetasuj listę możliwych pozycji siatki
        List<Vector2Int> gridPositions = new List<Vector2Int>();
        for (int x = 0; x < width; x += buildingArea)
        {
            for (int z = 0; z < height; z += buildingArea)
            {
                gridPositions.Add(new Vector2Int(x, z));
            }
        }

        Shuffle(gridPositions);

        foreach (var gridPos in gridPositions)
        {
            if (Random.value > density)
                continue;

            float offsetX = Random.Range(1f, buildingArea - 1f);
            float offsetZ = Random.Range(1f, buildingArea - 1f);

            Vector3 position = new Vector3(
                transform.position.x - width / 2 + gridPos.x + offsetX,
                0,
                transform.position.z - height / 2 + gridPos.y + offsetZ
            );

            GameObject prefab = PickBuilding();
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, position, Quaternion.identity, terrain.transform);
            spawnedBuildings.Add(go);
        }

        baseAlphaMap = CreateCleanAlphaMap();
        terrainData.SetAlphamaps(0, 0, baseAlphaMap);

        DrawPathsBetweenBuildings();
        surface.BuildNavMesh();
    }

    private GameObject PickBuilding()
    {
        List<BuildingConfig> available = buildingConfigs.FindAll(b => b.currentCount < b.maxCount);
        if (available.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var b in available)
            totalWeight += b.weight;

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

    private float[,,] CreateCleanAlphaMap()
    {
        TerrainData data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        float[,,] alphaMap = new float[w, h, layers];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                alphaMap[x, y, 0] = 1f; // trawa = layer 0

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
        Debug.Log("Przywrócono teren do bazowej wersji (sama trawa).");
    }

    public void ClearBuildings()
    {
        foreach (var go in spawnedBuildings)
            DestroyImmediate(go); // użyj Destroy() jeśli tylko w PlayMode
        spawnedBuildings.Clear();
    }

    void DrawPathsBetweenBuildings()
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

            if (to != null)
            {
                Vector3 worldStart = from.transform.position;
                Vector3 worldEnd = to.transform.position;

                Vector3 alphaStart = WorldToAlphaMapCoord(worldStart, terrainData);
                Vector3 alphaEnd = WorldToAlphaMapCoord(worldEnd, terrainData);

                int steps = 100;
                float pathWidth = 3f;

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    Vector3 pos = Vector3.Lerp(alphaStart, alphaEnd, t);
                    PaintCircleOnAlphaMap(currentAlphaMap, (int)pos.x, (int)pos.z, pathWidth, 1, numTextures);
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, currentAlphaMap);
    }

    Vector3 WorldToAlphaMapCoord(Vector3 worldPos, TerrainData terrainData)
    {
        Vector3 terrainPos = worldPos - terrain.transform.position;
        float x = (terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth;
        float z = (terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight;
        return new Vector3(x, 0, z);
    }

    GameObject GetClosestBuilding(GameObject from)
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

    void PaintCircleOnAlphaMap(float[,,] alphas, int centerX, int centerZ, float radius, int texIndex, int numTextures)
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

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
