using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; private set; }

    public int width = 100, height = 100;
    public int buildingArea = 10;
    public GameObject[] Buildings;
    public Terrain terrain;

    public List<GameObject> spawnedBuildings = new List<GameObject>();

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

        for (int x = 0; x < terrainData.size.x; x += buildingArea)
        {
            for (int z = 0; z < terrainData.size.z; z += buildingArea)
            {
                Vector3 position = new Vector3(
                    transform.position.x - width / 2 + x + buildingArea / 2,
                    0,
                    transform.position.z * 2 - height / 2 + z + buildingArea / 2
                );

                int randomIndex = Random.Range(0, 100);
                GameObject result = randomIndex switch
                {
                    int n when (n >= 0 && n <= 4) => Instantiate(Buildings[0], position, Quaternion.identity, terrain.transform),
                    int n when (n >= 5 && n <= 9) => Instantiate(Buildings[1], position, Quaternion.identity, terrain.transform),
                    int n when (n >= 10 && n <= 14) => Instantiate(Buildings[2], position, Quaternion.identity, terrain.transform),
                    int n when (n >= 15 && n <= 19) => Instantiate(Buildings[3], position, Quaternion.identity, terrain.transform),
                    _ => null
                };

                if (result != null)
                {
                    if (!result.TryGetComponent<NavMeshObstacle>(out var obstacle))
                    {
                        obstacle = result.AddComponent<NavMeshObstacle>();
                    }
                    obstacle.carving = true;

                    spawnedBuildings.Add(result);
                }
            }
        }

        //DrawPathsBetweenBuildings();
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

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

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
                    PaintCircleOnAlphaMap(alphaMap, (int)pos.x, (int)pos.z, pathWidth, 1, numTextures);
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
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
}
