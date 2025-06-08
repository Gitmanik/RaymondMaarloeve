using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores building data and handles tile occupation logic.
/// </summary>
public class BuildingData : MonoBehaviour
{
    /// <summary>Main tile assigned to this building.</summary>
    public Tile HisMainTile = null;

    /// <summary>NPC associated with this building, if any.</summary>
    public NPC HisNPC = null;

    /// <summary>Grid position of the main tile.</summary>
    public Vector2Int HisMainTileGridPosition = new Vector2Int(-1, -1);

    /// <summary>Type of this building.</summary>
    [SerializeField] public BuildingType HisType = BuildingType.None;

    /// <summary>All tiles occupied by this building.</summary>
    public List<Tile> HisTiles = new();

    /// <summary>Total count of occupied tiles.</summary>
    public int HisTileCount = 0;

    /// <summary>Types of buildings supported in the game.</summary>
    public enum BuildingType
    {
        None,
        House,
        Church,
        Well,
        Blacksmith,
        Tavern,
        Scaffold,
        Wall,
        Armoury,
        Barracks,
        Fortress,
        Tower,
        Gate,
        Other
    }

    /// <summary>
    /// Attempts to assign tiles as occupied based on render bounds and tile grid.
    /// </summary>
    /// <param name="tileSize">Size of one tile.</param>
    /// <param name="tiles">Tile array (grid).</param>
    /// <returns>True if successful, false if collision or error occurred.</returns>
    public bool AssignOccupiedTiles(float tileSize, Tile[,] tiles)
    {
        HisTiles.Clear();

        if (MapGenerator.Instance == null || MapGenerator.Instance.terrain == null)
        {
            Debug.LogWarning("MapGenerator or terrain is missing.");
            return false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);

        Vector3 min = combinedBounds.min;
        Vector3 max = combinedBounds.max;

        int startX = Mathf.FloorToInt((min.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int endX = Mathf.FloorToInt((max.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int startZ = Mathf.FloorToInt((min.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);
        int endZ = Mathf.FloorToInt((max.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);

        // Check collision before marking
        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                if (x < 0 || z < 0 || x >= tiles.GetLength(0) || z >= tiles.GetLength(1))
                    return false;

                if (tiles[x, z].IsPartOfBuilding)
                    return false;
            }

        // Mark tiles as occupied
        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Building = gameObject;
                HisTiles.Add(tile);
                HisTileCount++;
            }

        // Assign main tile unless Wall or Tower
        if (HisType != BuildingType.Wall && HisType != BuildingType.Tower)
        {
            Vector3 center = combinedBounds.center;
            Tile closest = null;
            float minDist = float.MaxValue;
            foreach (var tile in HisTiles)
            {
                Vector3 tilePos = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
                float dist = Vector3.Distance(tilePos, center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = tile;
                }
            }

            HisMainTile = closest;
            HisMainTileGridPosition = new Vector2Int(HisMainTile.GridPosition.x, HisMainTile.GridPosition.y);
            MapGenerator.Instance.buildingsMainTile.Add(HisMainTile);
        }

        Debug.Log($"{name} successfully assigned {HisTileCount} tiles.");
        return true;
    }

    /// <summary>
    /// Forcefully marks the tiles as occupied, ignoring any collisions or overlaps.
    /// Typically used for walls and gates.
    /// </summary>
    /// <param name="tileSize">Size of one tile.</param>
    /// <param name="tiles">Tile array (grid).</param>
    public void AssignWallTilesForcefully(float tileSize, Tile[,] tiles)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);

        Vector3 min = combinedBounds.min;
        Vector3 max = combinedBounds.max;

        int startX = Mathf.Max(0, Mathf.FloorToInt((min.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize));
        int endX = Mathf.Min(tiles.GetLength(0) - 1, Mathf.FloorToInt((max.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize));
        int startZ = Mathf.Max(0, Mathf.FloorToInt((min.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize));
        int endZ = Mathf.Min(tiles.GetLength(1) - 1, Mathf.FloorToInt((max.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize));

        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Building = gameObject;
                HisTiles.Add(tile);
                HisTileCount++;
            }

        if (HisType == BuildingType.Gate)
        {
            Vector3 center = combinedBounds.center;
            Tile closest = null;
            float minDist = float.MaxValue;
            foreach (var tile in HisTiles)
            {
                Vector3 tilePos = new Vector3(tile.TileCenter.x, 0, tile.TileCenter.y);
                float dist = Vector3.Distance(tilePos, center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = tile;
                }
            }

            HisMainTile = closest;
            HisMainTileGridPosition = new Vector2Int(HisMainTile.GridPosition.x, HisMainTile.GridPosition.y);
            MapGenerator.Instance.buildingsMainTile.Add(HisMainTile);
        }

        Debug.Log($"{name} forcefully assigned {HisTileCount} tiles.");
    }
}
