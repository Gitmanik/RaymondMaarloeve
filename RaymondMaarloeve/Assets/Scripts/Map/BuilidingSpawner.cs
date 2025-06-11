using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for spawning buildings on available map tiles, based on weighted probability and placement rules.
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
    /// <param name="terrain">Terrain used for height sampling.</param>
    /// <param name="tileSize">Size of a tile in world units.</param>
    /// <param name="mapWidthInTiles">Total map width in tiles.</param>
    /// <param name="mapLengthInTiles">Total map length in tiles.</param>
    /// <param name="wallsMargin">Minimum number of tiles to leave empty near the map edge.</param>
    public BuildingSpawner(Terrain terrain, int tileSize, int mapWidthInTiles, int mapLengthInTiles, int wallsMargin)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
        this.mapWidthInTiles = mapWidthInTiles;
        this.mapLengthInTiles = mapLengthInTiles;
        this.wallsMargin = wallsMargin;
    }

    /// <summary>
    /// Attempts to spawn buildings on the map based on tile availability and weighted prefab selection.
    /// </summary>
    /// <param name="tiles">2D grid of map tiles.</param>
    /// <param name="allTiles">Flat list of all tiles for randomization.</param>
    /// <param name="buildings">List of building prefabs and their configuration.</param>
    /// <returns>List of spawned building GameObjects.</returns>
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
            if (gridPos.x < wallsMargin || gridPos.y < wallsMargin ||
                gridPos.x >= mapWidthInTiles - wallsMargin || gridPos.y >= mapLengthInTiles - wallsMargin)
                continue;

            if (tile.IsBuilding || tile.IsPartOfBuilding)
                continue;

            var prefab = PickPrefab(buildings);
            if (prefab == null) continue;

            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

            Vector3 pos = new(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            var go = Object.Instantiate(prefab, pos, rotation, terrain.transform);
            var buildingData = go.GetComponent<BuildingData>();
            if (buildingData == null)
            {
                Object.Destroy(go);
                continue;
            }

            bool success = buildingData.AssignOccupiedTiles(tileSize, tiles);
            if (!success)
            {
                Object.Destroy(go);
                continue;
            }

            spawnedBuildings.Add(go);
            tile.IsBuilding = true;
            tile.Prefab = go;

            buildingsPlaced++;
        }

        Debug.Log($"BuildingSpawner: Spawned {buildingsPlaced} buildings.");
        return spawnedBuildings;
    }

    /// <summary>
    /// Randomly selects a prefab from the available list based on weighted probability.
    /// </summary>
    /// <param name="prefabList">List of building setups.</param>
    /// <returns>Selected prefab GameObject or null if none available.</returns>
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
    /// Randomly shuffles the given list in place.
    /// </summary>
    /// <typeparam name="T">Type of list elements.</typeparam>
    /// <param name="list">List to shuffle.</param>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
