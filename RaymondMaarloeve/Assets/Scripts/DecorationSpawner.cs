using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Spawns decorative elements randomly across the map on valid tiles.
/// Decorations do not affect gameplay but enhance visual variety.
/// </summary>
public class DecorationSpawner
{
    private readonly Terrain terrain;
    private readonly int tileSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecorationSpawner"/> class.
    /// </summary>
    public DecorationSpawner(Terrain terrain, int tileSize)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
    }

    /// <summary>
    /// Spawns decorative prefabs on suitable tiles.
    /// Avoids tiles already used for buildings or paths.
    /// </summary>
    /// <param name="tiles">2D array of all tiles.</param>
    /// <param name="allTiles">Flattened list of all tiles.</param>
    /// <param name="decorations">List of possible decoration prefabs.</param>
    /// <returns>Total number of decorations placed.</returns>
    public int SpawnDecorations(Tile[,] tiles, List<Tile> allTiles, List<BuildingSetup> decorations)
    {
        var shuffled = new List<Tile>(allTiles);
        Shuffle(shuffled);

        int decorationsPlaced = 0;

        foreach (var tile in shuffled)
        {
            // Skip tiles that are already occupied or reserved
            if (tile.IsBuilding || tile.IsPartOfBuilding || tile.IsPath)
                continue;

            float currentDensity = decorations.Count > 0 ? decorations[0].weight : 0.1f;
            if (Random.value > currentDensity)
                continue;

            var prefab = PickPrefab(decorations);
            if (prefab == null) continue;

            Vector3 pos3 = new(tile.TileCenter.x, 0, tile.TileCenter.y);
            pos3.y = terrain.SampleHeight(pos3) + terrain.transform.position.y;

            int[] angles = { 0, 90, 180, 270 };
            int randomAngle = angles[Random.Range(0, angles.Length)];
            Quaternion rotation = Quaternion.Euler(-90, randomAngle, 0);

            var go = Object.Instantiate(prefab, pos3, rotation, terrain.transform);

            tile.IsPath = true; // Mark tile to prevent reuse
            tile.Building = go;

            decorationsPlaced++;
        }

        Debug.Log($"Spawned {decorationsPlaced} decorations.");
        return decorationsPlaced;
    }

    /// <summary>
    /// Selects a decoration prefab from the list based on its weight and availability.
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
    /// Shuffles a generic list using the Fisher-Yates algorithm.
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
