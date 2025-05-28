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
    public bool markTiles = false; 

    Tile[,] tiles;

    public List<BuildingSetup> buildings = new();
    public List<GameObject> spawnedBuildings = new();
    public List<WallsSetup> walls = new();
    private int WallsMargin = 10;
    private GameObject wallsRoot;

    private List<Tile> buildingTiles;

    private float[,,] baseAlphaMap;
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapLength;

    
    void Awake()
    {
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        PathGenerator.ClearMap(terrain);

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

                tile.FrontWallCenter = new Vector2(
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

                        }

                        buildingTiles.Add(tile);

                    }
                }
            }

        // 3) Wyznaczanie i rysowanie ścieżek w osobnym managerze
        PathGenerator.GeneratePaths(tiles, buildingTiles, terrain);
        if (markTiles)
        {
            MarkTiles();
        }

        Debug.Log($"Zaznaczono tile zajęte przez mury (AssignOccupiedTiles) – {count} obiektów.");
    }



    void SpawnBuildings()
    {
        var shuffled = new List<Tile>(allTiles);
        Shuffle(shuffled);

        int buildingsPlaced = 0;
        int minimumBuildings = 6;

        foreach (var tile in shuffled)
        {
            float currentDensity = buildingsPlaced < minimumBuildings ? 1f : buildingsDensity;

            if (Random.value > currentDensity)
                continue;

            Vector2Int gridPos = tile.GridPosition;
            
            if (gridPos.x < WallsMargin || gridPos.y < WallsMargin || gridPos.x >= mapWidthInTiles - WallsMargin || gridPos.y >= mapLengthInTiles - WallsMargin)
                continue;

            if (tile.IsBuilding || tile.IsPartOfBuilding)
                continue;

            var prefab = PickBuilding();
            if (prefab == null)
                continue;

            // Losowa rotacja
            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

            Vector3 pos3 = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos3.y = terrain.SampleHeight(pos3) + terrain.transform.position.y;

            // Instancjonowanie z rotacją
            var go = Instantiate(prefab, pos3, rotation, terrain.transform);
            var buildingData = go.GetComponent<BuildingData>();
            if (buildingData == null)
            {
                Destroy(go);
                continue;
            }

            bool success = buildingData.AssignOccupiedTiles(tileSize, tiles);
            if (!success)
            {
                Destroy(go);
                continue;
            }

            spawnedBuildings.Add(go);
            tile.IsBuilding = true;
            tile.Building = go;
            buildingsMainTile.Add(tile);

            buildingsPlaced++;
        }

        Debug.Log($"Zbudowano {buildingsPlaced} budynków.");
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

    public GameObject GetBuilding(HashSet<BuildingData.BuildingType> allowedTypes)
    {
        List<GameObject> buildings = new List<GameObject>();

        foreach (GameObject go in spawnedBuildings)
        {
            var buildingdata = go.GetComponent<BuildingData>();
            if (buildingdata != null && allowedTypes.Contains(buildingdata.HisType) && buildingdata.HisNPC == null)
            {
                buildings.Add(go);
            }
        }
        GameObject chosenBuilding = null;
        int j = Random.Range(0, buildings.Count);
        chosenBuilding = buildings[j];


        return chosenBuilding;
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

        int thickness = 1; // obwódka 2px

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
                        SetAlphaSafe(alphas, x, startZ + t, 2, layers); // góra
                        SetAlphaSafe(alphas, x, endZ - t, 2, layers);   // dół
                    }
                }

                for (int z = startZ; z <= endZ; z++)
                {
                    for (int t = 0; t < thickness; t++)
                    {
                        SetAlphaSafe(alphas, startX + t, z, 2, layers); // lewa
                        SetAlphaSafe(alphas, endX - t, z, 2, layers);   // prawa
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
        for (int z = mapLengthInTiles - 1; z >= 0; z--) // od góry, żeby było jak mapa
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

// Klasa Tile i Building pozostają bez zmian
public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 FrontWallCenter;
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
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    public int occurenceRadius = 5;

    [HideInInspector] public int currentCount = 0;
}
[Serializable]
public class WallsSetup
{
    public GameObject prefab;

}
