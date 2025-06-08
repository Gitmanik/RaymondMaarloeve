using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for spawning all walls, towers, and gates around the map perimeter.
/// Handles prefab assignment, tile marking, and visual placement logic.
/// </summary>
public class WallSpawner
{
    // Wall configuration data
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
    /// Initializes a new instance of the WallSpawner class.
    /// </summary>
    public WallSpawner(List<WallsSetup> walls, Terrain terrain, int tileSize, int mapWidthInTiles, int mapLengthInTiles)
    {
        this.walls = walls;
        this.terrain = terrain;
        this.tileSize = tileSize;
        this.mapWidthInTiles = mapWidthInTiles;
        this.mapLengthInTiles = mapLengthInTiles;
    }

    /// <summary>
    /// Spawns walls, towers, and a gate around the edges of the map.
    /// </summary>
    public GameObject SpawnWalls(Tile[,] tiles)
    {
        IdentifyWallPrefabs();
        if (wallPrefab == null || towerPrefab == null || gatePrefab == null)
        {
            Debug.LogWarning("Missing prefabs for Wall, Tower, or Gate.");
            return null;
        }

        GameObject wallsRoot = new GameObject("WallsRoot");
        wallsRoot.transform.SetParent(terrain.transform);

        GameObject temp = Object.Instantiate(wallPrefab);
        float segmentLength = GetSegmentLength(temp) * 0.97f;
        Object.DestroyImmediate(temp);

        float mapWidth = tileSize * mapWidthInTiles;
        float mapLength = tileSize * mapLengthInTiles;

        // Spawn towers at each map corner
        PlaceBuildingAtTile(tiles[0, 0], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[mapWidthInTiles - 1, 0], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[0, mapLengthInTiles - 1], towerPrefab, wallsRoot, Quaternion.identity);
        PlaceBuildingAtTile(tiles[mapWidthInTiles - 1, mapLengthInTiles - 1], towerPrefab, wallsRoot, Quaternion.identity);

        // Spawn central gate at top middle
        gate = PlaceBuildingAtTile(tiles[mapWidthInTiles / 2, mapLengthInTiles - 1], gatePrefab, wallsRoot, Quaternion.Euler(0, 180, 0));
        Renderer gateRenderer = gate.GetComponentInChildren<Renderer>();
        Bounds gateBounds = gateRenderer.bounds;
        Vector3 gateLeft = new Vector3(gateBounds.min.x, gateBounds.center.y, gateBounds.center.z);
        Vector3 gateRight = new Vector3(gateBounds.max.x, gateBounds.center.y, gateBounds.center.z);

        // Spawn wall segments on either side of the gate
        GameObject northWall = new GameObject("NorthWall");
        northWall.transform.parent = wallsRoot.transform;
        SpawnWallLine(gateRight, Vector3.right, (mapWidth - gateBounds.size.x) / 2, segmentLength, Quaternion.Euler(0, 180, 0), wallPrefab, northWall);
        SpawnWallLine(gateLeft, Vector3.left, mapWidth / 2, segmentLength, Quaternion.identity, wallPrefab, northWall);

        // South wall
        GameObject southWall = new GameObject("SouthWall");
        southWall.transform.parent = wallsRoot.transform;
        Vector3 southStart = ToWorld(tiles[2, 0].TileCenter);
        SpawnWallLine(southStart, Vector3.right, mapWidth, segmentLength, Quaternion.identity, wallPrefab, southWall);

        // West wall
        GameObject westWall = new GameObject("WestWall");
        westWall.transform.parent = wallsRoot.transform;
        Vector3 westStart = ToWorld(tiles[0, 0].TileCenter);
        SpawnWallLine(westStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab, westWall);

        // East wall
        GameObject eastWall = new GameObject("EastWall");
        eastWall.transform.parent = wallsRoot.transform;
        Vector3 eastStart = ToWorld(tiles[mapWidthInTiles - 1, 1].TileCenter);
        SpawnWallLine(eastStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 270, 0), wallPrefab, eastWall);

        // Mark tiles occupied by wall elements
        AssignWallTileOccupation(wallsRoot, tiles);
        return wallsRoot;
    }

    /// <summary>
    /// Marks tiles occupied by wall-related objects (walls, gates, towers).
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
                    bool success = bd.AssignOccupiedTiles(tileSize, tiles);
                    if (success) count++;
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
                    bool success = buildingData.AssignOccupiedTiles(tileSize, tiles);
                    if (success) count++;
                }
            }
        }

        Debug.Log($"Marked wall tiles (AssignOccupiedTiles) – {count} objects total.");
    }

    /// <summary>
    /// Selects the wall, tower, and gate prefabs from the provided wall list.
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
    /// Estimates the length of a single wall segment based on its Renderer bounds.
    /// </summary>
    private float GetSegmentLength(GameObject go)
    {
        var renderer = go.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Prefab has no Renderer.");
            return tileSize;
        }

        Vector3 size = renderer.bounds.size;
        return Mathf.Max(size.x, size.z);
    }

    /// <summary>
    /// Converts tile center coordinates to world-space position on terrain.
    /// </summary>
    private Vector3 ToWorld(Vector2 tileCenter)
    {
        Vector3 pos = new(tileCenter.x, 0, tileCenter.y);
        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
        return pos;
    }

    /// <summary>
    /// Spawns a line of wall segments in a given direction from a start point.
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
    /// Instantiates a building prefab at a given tile's position and marks the tile as occupied.
    /// </summary>
    private GameObject PlaceBuildingAtTile(Tile tile, GameObject prefab, GameObject parent, Quaternion rotation)
    {
        Vector3 pos = ToWorld(tile.TileCenter);
        GameObject go = Object.Instantiate(prefab, pos, rotation, parent.transform);
        tile.IsPartOfBuilding = true;
        tile.Building = go;
        return go;
    }
}
