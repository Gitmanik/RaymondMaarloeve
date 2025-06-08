using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for spawning building prefabs onto valid map tiles.
/// Handles prefab selection, rotation, placement, and tile occupation.
/// </summary>
public class BuildingSpawner
{
    private readonly Terrain terrain;
    private readonly int tileSize;
    private readonly int mapWidthInTiles;
    private readonly int mapLengthInTiles;
    private readonly int wallsMargin;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildingSpawner"/> class.
    /// </summary>
    public BuildingSpawner(Terrain terrain, int tileSize, int mapWidthInTiles, int mapLengthInTiles, int wallsMargin)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
        this.mapWidthInTiles = mapWidthInTiles;
        this.mapLengthInTiles = mapLengthInTiles;
        this.wallsMargin = wallsMargin;
    }

    /// <summary>
    /// Attempts to spawn buildings on tiles that are unoccupied and within boundaries.
    /// </summary>
    /// <param name="tiles">2D tile grid of the map.</param>
    /// <param name="allTiles">Flattened list of all tiles.</param>
    /// <param name="buildings">Available building prefab configurations.</param>
    /// <returns>List of successfully spawned building GameObjects.</returns>
    public List<GameObject> SpawnBuildings(Tile[,] tiles, List<Tile> allTiles, List<BuildingSetup> buildings)
    {
        var spawnedBuildings = new List<GameObject>();
        var shuffled = new List<Tile>(allTiles);
        Shuffle(shuffled);

        int buildingsPlaced = 0;
        int minimumBuildings = 6;

        foreach (var tile in shuffled)
        {
            float currentDensity = buildingsPlaced < minimumBuildings ? 1f : buildings[0].weight;
            if (Random.value > currentDensity)
                continue;

            Vector2Int gridPos = tile.GridPosition;

            // Skip edge tiles near the wall margin
            if (gridPos.x < wallsMargin || gridPos.y < wallsMargin ||
                gridPos.x >= mapWidthInTiles - wallsMargin || gridPos.y >= mapLengthInTiles - wallsMargin)
                continue;

            if (tile.IsBuilding || tile.IsPartOfBuilding)
                continue;

            var prefab = PickPrefab(buildings);
            if (prefab == null) continue;

            // Random rotation in 90-degree increments
            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

            Vector3 pos3 = new(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos3.y = terrain.SampleHeight(pos3) + terrain.transform.position.y;

            var go = Object.Instantiate(prefab, pos3, rotation, terrain.transform);
            var buildingData = go.GetComponent<BuildingData>();
            if (buildingData == null)
            {
                Object.Destroy(go);
                Debug.LogWarning("Spawned building is missing BuildingData component. Destroyed.");
                continue;
            }

            bool success = buildingData.AssignOccupiedTiles(tileSize, tiles);
            if (!success)
            {
                Object.Destroy(go);
                Debug.Log("Building could not occupy tiles. Destroyed.");
                continue;
            }

            spawnedBuildings.Add(go);
            tile.IsBuilding = true;
            tile.Building = go;

            buildingsPlaced++;
        }

        Debug.Log($"Placed {buildingsPlaced} buildings.");
        return spawnedBuildings;
    }

    /// <summary>
    /// Selects a prefab from the available list based on weight and max count constraints.
    /// </summary>
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

    /// <summary>
    /// Randomly shuffles the elements in the provided list using Fisher-Yates algorithm.
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
