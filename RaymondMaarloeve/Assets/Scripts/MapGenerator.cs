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

    private List<Tile> allTiles = new();


    Tile[,] tiles;

    public List<BuildingSetup> buildings = new();
    public List<GameObject> spawnedBuildings = new();
    public List<WallsSetup> walls = new();
    public int WallsMargin = 10;
    private GameObject wallsRoot;

    public List<BuildingSetup> decorations = new();


    public List<Tile> buildingsMainTile;

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
        buildingsMainTile = new List<Tile>();
        spawnedBuildings = new List<GameObject>();

        InitializeTiles();
        SpawnWalls();
        SpawnBuildings();
        SpawnDecorations();

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

        // przypisz sąsiadów
        for (int x = 0; x < mapWidthInTiles; x++)
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

    void SpawnWalls()
    {
        // Wyszukiwanie prefabów typu Wall i Tower z BuildingData
        GameObject wallPrefab = null;
        GameObject towerPrefab = null;
        GameObject gatePrefab = null;

        foreach (var wall in walls)
        {
            if (wall.prefab == null) continue;

            var data = wall.prefab.GetComponent<BuildingData>();
            if (data == null) continue;

            switch (data.HisType)
            {
                case BuildingData.BuildingType.Wall:
                    if (wallPrefab == null)
                        wallPrefab = wall.prefab;
                    break;
                case BuildingData.BuildingType.Tower:
                    if (towerPrefab == null)
                        towerPrefab = wall.prefab;
                    break;
                case BuildingData.BuildingType.Gate:
                    if (gatePrefab == null)
                        gatePrefab = wall.prefab;
                    break;
            }

            if (wallPrefab != null && towerPrefab != null && gatePrefab != null)
                break;
        }


        if (wallPrefab == null)
        {
            Debug.LogWarning("Nie znaleziono prefabów typu Wall.");
            return;
        }

        if (towerPrefab == null)
        {
            Debug.LogWarning("Nie znaleziono prefabów typu Tower.");
            return;
        }

        if (gatePrefab == null)
        {
            Debug.LogWarning("Nie znaleziono prefabów typu Gate.");
            return;
        }


        // Usunięcie starych murów
        if (wallsRoot != null)
            DestroyImmediate(wallsRoot);
        wallsRoot = new GameObject("WallsRoot");
        wallsRoot.transform.SetParent(terrain.transform);

        // Pomiar długości segmentu muru
        GameObject temp = Instantiate(wallPrefab);
        float segmentLength = GetSegmentLength(temp) * 0.97f;
        DestroyImmediate(temp);

        float mapWidth = tileSize * mapWidthInTiles;
        float mapLength = tileSize * mapLengthInTiles;

        // === ŚCIANY ===

        // Południe
        Vector3 southStart = new Vector3(tiles[2, 0].TileCenter.x, 0, tiles[2, 0].TileCenter.y);
        southStart.y = terrain.SampleHeight(southStart) + terrain.transform.position.y;
        SpawnWallLine(southStart, Vector3.right, mapWidth, segmentLength, Quaternion.identity, wallPrefab);

        // Północ
        Vector3 northStart = new Vector3(tiles[2, mapLengthInTiles - 1].TileCenter.x, 0, tiles[2, mapLengthInTiles - 1].TileCenter.y);
        northStart.y = terrain.SampleHeight(northStart) + terrain.transform.position.y;
        SpawnWallLineWithGate(northStart, Vector3.right, mapWidth, segmentLength, Quaternion.Euler(0, 180, 0), wallPrefab, gatePrefab);

        // Zachód
        Vector3 westStart = new Vector3(tiles[0, 0].TileCenter.x, 0, tiles[0, 0].TileCenter.y);
        westStart.y = terrain.SampleHeight(westStart) + terrain.transform.position.y;
        SpawnWallLine(westStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab);

        // Wschód
        Vector3 eastStart = new Vector3(tiles[mapWidthInTiles - 1, 0].TileCenter.x, 0, tiles[mapWidthInTiles - 1, 0].TileCenter.y);
        eastStart.y = terrain.SampleHeight(eastStart) + terrain.transform.position.y;
        SpawnWallLine(eastStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab);

        // === WIEŻE (narożniki) ===
        PlaceTowerAtTile(tiles[0, 0], towerPrefab);                                      // SW
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, 0], towerPrefab);                   // SE
        PlaceTowerAtTile(tiles[0, mapLengthInTiles - 1], towerPrefab);                 // NW
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, mapLengthInTiles - 1], towerPrefab); // NE

        MarkWallTiles();

        Debug.Log("Postawiono mury i wieże.");
    }
    float GetSegmentLength(GameObject go)
    {
        var renderer = go.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Prefab nie ma Renderera.");
            return tileSize;
        }

        Vector3 size = renderer.bounds.size;
        return Mathf.Max(size.x, size.z);
    }
    void SpawnWallLine(Vector3 origin, Vector3 direction, float totalLength, float segmentLength, Quaternion rotation, GameObject prefab)
    {
        int count = Mathf.FloorToInt(totalLength / segmentLength);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = origin + direction * segmentLength * i;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            Instantiate(prefab, pos, rotation, wallsRoot.transform);
        }
    }
    void SpawnWallLineWithGate(Vector3 origin, Vector3 direction, float totalLength, float segmentLength, Quaternion rotation, GameObject wallPrefab, GameObject gatePrefab)
    {
        int count = Mathf.FloorToInt(totalLength / segmentLength);
        int gateIndex = count / 2;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = origin + direction * segmentLength * i;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            GameObject prefabToUse = (i == gateIndex) ? gatePrefab : wallPrefab;

            if (i == gateIndex)
            {
                pos += direction * 2f; // twardy offset tylko dla bramy
            }
            Instantiate(prefabToUse, pos, rotation, wallsRoot.transform);
        }
    }

    void PlaceTowerAtTile(Tile tile, GameObject prefab)
    {
        Vector3 pos = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

        GameObject go = Instantiate(prefab, pos, Quaternion.identity, wallsRoot.transform);
        tile.IsPartOfBuilding = true;
        tile.Building = go;
    }
    void MarkWallTiles()
    {
        int count = 0;

        foreach (Transform child in wallsRoot.transform)
        {
            var buildingData = child.GetComponent<BuildingData>();
            if (buildingData == null)
                continue;

            bool success = buildingData.AssignOccupiedTiles(tileSize, tiles);
            if (success) count++;
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

            var prefab = PickPrefab(buildings);
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

    void SpawnDecorations()
    {
        var shuffled = new List<Tile>(allTiles);
        Shuffle(shuffled);
        int decorationsPlaced = 0;
        foreach (var tile in shuffled)
        {
            if (tile.IsBuilding || tile.IsPartOfBuilding || tile.IsPath)
                continue;
            float currentDensity = decorations.Count > 0 ? decorations[0].weight : 0.1f;
            if (Random.value > currentDensity)
                continue;
            var prefab = PickPrefab(decorations);
            if (prefab == null)
                continue;
            Vector3 pos3 = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos3.y = terrain.SampleHeight(pos3) + terrain.transform.position.y;
            // Losowa rotacja
            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];
            Quaternion rotation = Quaternion.Euler(-90, randomAngle, 0);    //-90 bo mam assety obrócone i nie chce mi się tego poprawiać

            var go = Instantiate(prefab, pos3, rotation, terrain.transform);
            tile.IsPath = true;
            tile.TileObject = go;
            decorationsPlaced++;
        }
        Debug.Log($"Zbudowano {decorationsPlaced} dekoracji.");
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

    private GameObject PickPrefab(List<BuildingSetup> prefabList)
    {
        var available = prefabList.FindAll(b => b.currentCount < b.maxCount);
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

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
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
