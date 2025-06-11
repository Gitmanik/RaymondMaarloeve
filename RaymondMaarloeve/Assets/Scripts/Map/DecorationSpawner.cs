using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for placing decorative elements (e.g. trees, rocks) on available tiles in the terrain.
/// </summary>
public class DecorationSpawner
{
    private readonly Terrain terrain;
    private readonly int tileSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecorationSpawner"/> class.
    /// </summary>
    /// <param name="terrain">Reference to the terrain object for height placement.</param>
    /// <param name="tileSize">Size of a single tile in world units.</param>
    public DecorationSpawner(Terrain terrain, int tileSize)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
    }

    /// <summary>
    /// Spawns decorative objects randomly on unoccupied tiles.
    /// </summary>
    /// <param name="tiles">2D grid of map tiles.</param>
    /// <param name="allTiles">Flat list of all tiles for random sampling.</param>
    /// <param name="decorations">List of decoration prefabs with weight configuration.</param>
    /// <returns>Total number of decorations placed.</returns>
    public int SpawnDecorations(Tile[,] tiles, List<Tile> allTiles, List<BuildingSetup> decorations)
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
            if (prefab == null) continue;

            Vector3 pos = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];

            var buildingData = prefab.GetComponent<BuildingData>();
            Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);
            if (buildingData != null && buildingData.HisType == BuildingData.BuildingType.Tree)
            {
                rotation = Quaternion.Euler(-90, randomAngle, 0);
            }

            var go = Object.Instantiate(prefab, pos, rotation, terrain.transform);
            tile.IsDecoration = true;
            tile.Prefab = go;

            decorationsPlaced++;
        }

        Debug.Log($"DecorationSpawner: Placed {decorationsPlaced} decorations.");
        return decorationsPlaced;
    }

    /// <summary>
    /// Picks a prefab from the list based on weighted probability.
    /// </summary>
    /// <param name="prefabList">List of prefab configurations.</param>
    /// <returns>Selected GameObject or null if none available.</returns>
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
    /// Randomly shuffles a list in place using Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">Element type of the list.</typeparam>
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
