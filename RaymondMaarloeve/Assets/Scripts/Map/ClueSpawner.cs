using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for spawning a clue near the building of the murderer NPC.
/// </summary>
public class ClueSpawner
{
    private readonly Terrain terrain;
    private readonly int tileSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClueSpawner"/> class.
    /// </summary>
    /// <param name="terrain">Reference to the terrain used for clue placement.</param>
    /// <param name="tileSize">Size of a tile in world units.</param>
    public ClueSpawner(Terrain terrain, int tileSize)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
    }

    /// <summary>
    /// Spawns a clue next to the murderer's house entrance on a suitable free tile.
    /// </summary>
    /// <param name="tiles">2D grid of map tiles.</param>
    /// <param name="allTiles">Flat list of all tiles (not used here, but required for consistency).</param>
    /// <param name="clues">List of clue prefabs to choose from.</param>
    public void SpawnClues(Tile[,] tiles, List<Tile> allTiles, List<ClueSetup> clues)
    {
        var murdererHouse = GameManager.Instance.murdererNPC.HisBuilding;
        Tile clueTile = GetEntranceNeighborTile(murdererHouse, tiles);

        if (clueTile == null)
        {
            Debug.LogWarning("ClueSpawner: No valid tile found near the entrance.");
            return;
        }

        Vector3 position = new Vector3(clueTile.TileCenter.x, 0, clueTile.TileCenter.y);
        position.y = terrain.SampleHeight(position) + terrain.transform.position.y;

        int randomAngle = Random.Range(0, 360);
        Quaternion rotation = Quaternion.Euler(-90, randomAngle, 0);

        var prefab = clues[Random.Range(0, clues.Count)].prefab;
        var go = Object.Instantiate(prefab, position, rotation, terrain.transform);
        clueTile.IsClue = true;
        clueTile.Prefab = go;

        Debug.Log($"ClueSpawner: Spawned clue ({prefab.name}) at tile {clueTile.TileCenter}");
    }

    /// <summary>
    /// Finds the tile closest to the entrance of the given building that is not occupied.
    /// </summary>
    /// <param name="murdererHouse">GameObject representing the murderer's house.</param>
    /// <param name="tiles">2D grid of map tiles.</param>
    /// <returns>Closest unoccupied tile near the entrance or null if none found.</returns>
    private static Tile GetEntranceNeighborTile(GameObject murdererHouse, Tile[,] tiles)
    {
        Transform entrance = murdererHouse.transform.Find("Entrance");
        if (entrance == null)
        {
            Debug.LogWarning($"ClueSpawner: Missing 'Entrance' transform on building: {murdererHouse.name}");
            return null;
        }

        Vector3 entranceWorldPos = entrance.position;
        float minDistance = float.MaxValue;
        Tile closestTile = null;

        foreach (var tile in tiles)
        {
            if (tile == null || tile.IsPartOfBuilding || tile.IsPath || tile.IsDecoration)
                continue;

            Vector3 tileWorldPos = new Vector3(tile.TileCenter.x, entranceWorldPos.y, tile.TileCenter.y);
            float distance = Vector3.SqrMagnitude(entranceWorldPos - tileWorldPos);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
}
