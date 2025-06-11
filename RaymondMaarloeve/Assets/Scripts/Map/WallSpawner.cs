using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for spawning the outer walls, towers, and gate around the map boundary.
/// </summary>
public class WallSpawner
{
    private readonly List<WallsSetup> walls;
    private readonly Terrain terrain;
    private readonly int tileSize;
    private readonly int mapWidthInTiles;
    private readonly int mapLengthInTiles;

    private GameObject wallPrefab;
    private GameObject towerPrefab;
    private GameObject gatePrefab;

    private GameObject gate;

    /// <summary>
    /// Initializes a new instance of the <see cref="WallSpawner"/> class.
    /// </summary>
    /// <param name="walls">List of wall prefabs with their configurations.</param>
    /// <param name="terrain">Terrain reference for height sampling.</param>
    /// <param name="tileSize">Size of a single tile in world units.</param>
    /// <param name="mapWidthInTiles">Number of tiles in width.</param>
    /// <param name="mapLengthInTiles">Number of tiles in length.</param>
    public WallSpawner(List<WallsSetup> walls, Terrain terrain, int tileSize, int mapWidthInTiles, int mapLengthInTiles)
    {
        this.walls = walls;
        this.terrain = terrain;
        this.tileSize = tileSize;
        this.mapWidthInTiles = mapWidthInTiles;
        this.mapLengthInTiles = mapLengthInTiles;
    }

    /// <summary>
    /// Spawns the perimeter walls, towers, and gate around the map.
    /// </summary>
    /// <param name="tiles">2D tile array of the map.</param>
    /// <returns>Parent GameObject containing all wall elements.</returns>
    public GameObject SpawnWalls(Tile[,] tiles)
    {
        IdentifyWallPrefabs();
        if (wallPrefab == null || towerPrefab == null || gatePrefab == null)
        {
            Debug.LogWarning("WallSpawner: Missing required prefabs (Wall, Tower, or Gate)");
            return null;
        }

        GameObject wallsRoot = new GameObject("WallsRoot");
        wallsRoot.transform.SetParent(terrain.transform);

        GameObject temp = Object.Instantiate(wallPrefab);
        float segmentLength = GetSegmentLength(temp) * 0.97f;
        Object.DestroyImmediate(temp);

        float mapWidth = tileSize * mapWidthInTiles;
        float mapLength = tileSize * mapLengthInTiles;

        // Place corner towers
        PlaceBuildingAtTile(tiles[0, 0], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[mapWidthInTiles - 1, 0], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[0, mapLengthInTiles - 1], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[mapWidthInTiles - 1, mapLengthInTiles - 1], towerPrefab, wallsRoot, Quaternion.identity);

        // Place gate
        gate = PlaceBuildingAtTile(tiles[mapWidthInTiles / 2, mapLengthInTiles - 1], gatePrefab, wallsRoot, Quaternion.Euler(0, 180, 0));
        Renderer gateRenderer = gate.GetComponentInChildren<Renderer>();
        Bounds gateBounds = gateRenderer.bounds;

        Vector3 gateLeft = new Vector3(gateBounds.min.x, gateBounds.center.y, gateBounds.center.z);
        Vector3 gateRight = new Vector3(gateBounds.max.x, gateBounds.center.y, gateBounds.center.z);

        // North Wall (split around the gate)
        GameObject northWall = new GameObject("NorthWall");
        northWall.transform.parent = wallsRoot.transform;
        SpawnWallLine(gateRight, Vector3.right, (mapWidth - gateBounds.size.x) / 2, segmentLength, Quaternion.Euler(0, 180, 0), wallPrefab, northWall);
        SpawnWallLine(gateLeft, Vector3.left, mapWidth / 2, segmentLength, Quaternion.identity, wallPrefab, northWall);

        // South Wall
        GameObject southWall = new GameObject("SouthWall");
        southWall.transform.parent = wallsRoot.transform;
        Vector3 southStart = ToWorld(tiles[2, 0].TileCenter);
        SpawnWallLine(southStart, Vector3.right, mapWidth, segmentLength, Quaternion.identity, wallPrefab, southWall);

        // West Wall
        GameObject westWall = new GameObject("WestWall");
        westWall.transform.parent = wallsRoot.transform;
        Vector3 westStart = ToWorld(tiles[0, 0].TileCenter);
        SpawnWallLine(westStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab, westWall);

        // East Wall
        GameObject eastWall = new GameObject("EastWall");
        eastWall.transform.parent = wallsRoot.transform;
        Vector3 eastStart = ToWorld(tiles[mapWidthInTiles - 1, 1].TileCenter);
        SpawnWallLine(eastStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 270, 0), wallPrefab, eastWall);

        AssignWallTileOccupation(wallsRoot, tiles);
        return wallsRoot;
    }

    /// <summary>
    /// Assigns tile occupation metadata to all wall-related structures.
    /// </summary>
    private void AssignWallTileOccupation(GameObject wallsRoot, Tile[,] tiles)
    {
        int count = 0;
        foreach (Transform child in wallsRoot.transform)
        {
            var bd = child.GetComponent<BuildingData>();
            if (bd != null)
            {
                if (bd.HisType == BuildingData.BuildingType.Wall ||
                    bd.HisType == BuildingData.BuildingType.Gate ||
                    bd.HisType == BuildingData.BuildingType.Tower)
                {
                    bd.AssignWallTilesForcefully(tileSize, tiles);
                    count++;
                }
                else
                {
                    if (bd.AssignOccupiedTiles(tileSize, tiles)) count++;
                }
            }

            foreach (Transform wallPart in child)
            {
                var buildingData = wallPart.GetComponent<BuildingData>();
                if (buildingData == null) continue;

                if (buildingData.HisType == BuildingData.BuildingType.Wall ||
                    buildingData.HisType == BuildingData.BuildingType.Gate ||
                    buildingData.HisType == BuildingData.BuildingType.Tower)
                {
                    buildingData.AssignWallTilesForcefully(tileSize, tiles);
                    count++;
                }
                else
                {
                    if (buildingData.AssignOccupiedTiles(tileSize, tiles)) count++;
                }
            }
        }

        Debug.Log($"WallSpawner: Marked wall tiles (AssignOccupiedTiles) – {count} objects.");
    }

    /// <summary>
    /// Identifies and caches wall, tower, and gate prefabs from the provided list.
    /// </summary>
    private void IdentifyWallPrefabs()
    {
        foreach (var wall in walls)
        {
            if (wall.prefab == null) continue;
            var data = wall.prefab.GetComponent<BuildingData>();
            if (data == null) continue;

            switch (data.HisType)
            {
                case BuildingData.BuildingType.Wall:
                    if (wallPrefab == null) wallPrefab = wall.prefab;
                    break;
                case BuildingData.BuildingType.Tower:
                    if (towerPrefab == null) towerPrefab = wall.prefab;
                    break;
                case BuildingData.BuildingType.Gate:
                    if (gatePrefab == null) gatePrefab = wall.prefab;
                    break;
            }

            if (wallPrefab && towerPrefab && gatePrefab) break;
        }
    }

    /// <summary>
    /// Returns the physical size of a wall prefab.
    /// </summary>
    private float GetSegmentLength(GameObject go)
    {
        var renderer = go.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("WallSpawner: Prefab is missing Renderer component.");
            return tileSize;
        }
        Vector3 size = renderer.bounds.size;
        return Mathf.Max(size.x, size.z);
    }

    /// <summary>
    /// Converts a tile-centered 2D position into a world-space position using terrain height sampling.
    /// </summary>
    private Vector3 ToWorld(Vector2 tileCenter)
    {
        Vector3 pos = new(tileCenter.x, 0, tileCenter.y);
        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
        return pos;
    }

    /// <summary>
    /// Spawns a continuous line of wall segments between two points.
    /// </summary>
    private void SpawnWallLine(Vector3 origin, Vector3 direction, float totalLength, float segmentLength, Quaternion rotation, GameObject prefab, GameObject parent)
    {
        int count = Mathf.FloorToInt(totalLength / segmentLength);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = origin + direction * segmentLength * i;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
            Object.Instantiate(prefab, pos, rotation, parent.transform);
        }
    }

    /// <summary>
    /// Places a gate or tower at a given tile with rotation.
    /// </summary>
    private GameObject PlaceBuildingAtTile(Tile tile, GameObject prefab, GameObject parent, Quaternion rotation)
    {
        Vector3 pos = ToWorld(tile.TileCenter);
        GameObject go = Object.Instantiate(prefab, pos, rotation, parent.transform);
        tile.IsPartOfBuilding = true;
        tile.Prefab = go;

        return go;
    }
}
