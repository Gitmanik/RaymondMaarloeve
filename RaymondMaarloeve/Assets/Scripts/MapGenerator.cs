//MapGenerator.cs
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
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapLength;

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
                tiles[x, z].TileCenter = new Vector2(transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize / 2f,
                                                     transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f);
                tiles[x, z].PosXWallCenter = new Vector2(transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize,
                                                     transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f);
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
            if(tile.IsBuilding)
            {
                PaintPath(tile.TileCenter, tile.PosXWallCenter, 0.7f, 1);
            }

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
    private void OnDestroy()
    {
        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    }

    //private void CleanMap()
    //{
    //    terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    //}
    public void PaintPath(Vector2 start, Vector2 end, float radius, int textureLayerIndex)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int alphaWidth = data.alphamapWidth;
        int alphaHeight = data.alphamapHeight;
        int numLayers = data.alphamapLayers;

        float[,,] alphas = data.GetAlphamaps(0, 0, alphaWidth, alphaHeight);

        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (radius * 0.5f)); // krok co pó³ promienia
        float mapRadius = (radius / data.size.x) * alphaWidth;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            //WARNING coords are switched, in other way paths are painted in mirror Tile. IDK where is bug
            int mapZ = (int)(((point.x - terrainPos.x) / data.size.x) * alphaWidth);
            int mapX = (int)(((point.y - terrainPos.z) / data.size.z) * alphaHeight);

            PaintCircle(alphas, mapX, mapZ, mapRadius, textureLayerIndex, numLayers);
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    private void PaintCircle(float[,,] alphas, int centerX, int centerZ, float radius, int texIndex, int numTextures)
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


    private void OnDrawGizmos()
    {
        if (tiles == null) return;

        // Kolor i rozmiar kó³ek dla œrodków kafelków
        Gizmos.color = Color.green;
        float sphereOffset = 0.1f;
        float sphereRadius = 0.2f;

        for (int x = 0; x < mapWidthInTiles; x++)
        {
            for (int z = 0; z < mapLengthInTiles; z++)
            {
                // jeœli nie masz Vector3 TileCenter, to zast¹p TileCenter.X i .Y odpowiednio
                Vector3 tc = new Vector3(
                    tiles[x, z].TileCenter.x,
                    terrain.transform.position.y,
                    tiles[x, z].TileCenter.y
                );
                Gizmos.DrawSphere(tc + Vector3.up * sphereOffset, sphereRadius);

                if (tiles[x, z].IsBuilding && tiles[x, z].Building != null)
                {
                    // rysujemy druciany szeœcian wokó³ budynku
                    Gizmos.color = Color.red;
                    Vector3 bp = tiles[x, z].Building.transform.position;
                    Gizmos.DrawWireCube(bp + Vector3.up * sphereOffset, Vector3.one * 0.5f);

                    // przywracamy kolor dla kulki
                    Gizmos.color = Color.green;
                }
            }
        }
    }


}
public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 PosXWallCenter;
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

