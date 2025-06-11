using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component responsible for tracking a building's tile occupation, type, and association with NPCs.
/// Provides methods for assigning occupied tiles and marking the main tile for the building.
/// </summary>
public class BuildingData : MonoBehaviour
{
    /// <summary>
    /// The main tile this building is associated with (e.g., for pathfinding or interaction).
    /// </summary>
    public Tile HisMainTile = null;

    /// <summary>
    /// List of NPCs associated with this building.
    /// </summary>
    public List<NPC> HisNPC = null;

    /// <summary>
    /// Grid coordinates of the main tile.
    /// </summary>
    public Vector2Int HisMainTileGridPosition = new Vector2Int(-1, -1);

    /// <summary>
    /// Type of building represented by this component.
    /// </summary>
    [SerializeField] public BuildingType HisType = BuildingType.None;

    /// <summary>
    /// All tiles occupied by this building.
    /// </summary>
    public List<Tile> HisTiles = new();

    /// <summary>
    /// The number of tiles currently occupied by this building.
    /// </summary>
    public int HisTileCount = 0;

    /// <summary>
    /// Available building type enumerations.
    /// </summary>
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
        Tree,
        Bush,
        Stone,
        Small_Decoration,
        Clue,
        Other
    }

    /// <summary>
    /// Attempts to assign the tiles that this building occupies, based on its bounding box.
    /// If any tile is already occupied, the method fails.
    /// </summary>
    /// <param name="tileSize">World size of one tile.</param>
    /// <param name="tiles">2D array of all tiles on the map.</param>
    /// <returns>True if assignment succeeded; false if a collision occurred or data is missing.</returns>
    public bool AssignOccupiedTiles(float tileSize, Tile[,] tiles)
    {
        HisTiles.Clear();

        if (MapGenerator.Instance == null || MapGenerator.Instance.terrain == null)
        {
            Debug.LogWarning("BuildingData: MapGenerator or terrain is missing.");
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

        // Check for collisions
        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                if (x < 0 || z < 0 || x >= tiles.GetLength(0) || z >= tiles.GetLength(1))
                    return false;

                if (tiles[x, z].IsPartOfBuilding)
                    return false;
            }
        }

        // Mark tiles as occupied
        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Prefab = gameObject;
                HisTiles.Add(tile);
                HisTileCount++;
            }
        }

        // Assign main tile for buildings (excluding Wall and Tower)
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
            HisMainTileGridPosition = new Vector2Int(closest.GridPosition.x, closest.GridPosition.y);
            MapGenerator.Instance.buildingsMainTile.Add(closest);
        }

        return true;
    }

    /// <summary>
    /// Assigns tiles as occupied without checking for collisions.
    /// Intended for walls, gates, and towers.
    /// </summary>
    /// <param name="tileSize">Size of one tile in world units.</param>
    /// <param name="tiles">The full tile grid.</param>
    public void AssignWallTilesForcefully(float tileSize, Tile[,] tiles)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);

        Vector3 min = combinedBounds.min;
        Vector3 max = combinedBounds.max;

        int startX = Mathf.FloorToInt((min.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int endX = Mathf.FloorToInt((max.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int startZ = Mathf.FloorToInt((min.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);
        int endZ = Mathf.FloorToInt((max.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);

        startX = Mathf.Max(0, startX);
        startZ = Mathf.Max(0, startZ);
        endX = Mathf.Min(tiles.GetLength(0) - 1, endX);
        endZ = Mathf.Min(tiles.GetLength(1) - 1, endZ);

        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Prefab = gameObject;
                HisTiles.Add(tile);
                HisTileCount++;
            }
        }

        // Assign main tile only for Gate objects
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
            HisMainTileGridPosition = new Vector2Int(closest.GridPosition.x, closest.GridPosition.y);
            MapGenerator.Instance.buildingsMainTile.Add(closest);
        }
    }
}
