using System.Collections.Generic;
using UnityEngine;

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

    public WallSpawner(List<WallsSetup> walls, Terrain terrain, int tileSize, int mapWidthInTiles, int mapLengthInTiles)
    {
        this.walls = walls;
        this.terrain = terrain;
        this.tileSize = tileSize;
        this.mapWidthInTiles = mapWidthInTiles;
        this.mapLengthInTiles = mapLengthInTiles;
    }

    public GameObject SpawnWalls(Tile[,] tiles)
    {
        IdentifyWallPrefabs();
        if (wallPrefab == null || towerPrefab == null || gatePrefab == null)
        {
            Debug.LogWarning("Brakuje prefabów (Wall/Tower/Gate)");
            return null;
        }

        GameObject wallsRoot = new GameObject("WallsRoot");
        wallsRoot.transform.SetParent(terrain.transform);

        GameObject temp = Object.Instantiate(wallPrefab);
        float segmentLength = GetSegmentLength(temp) * 0.97f;
        Object.DestroyImmediate(temp);

        float mapWidth = tileSize * mapWidthInTiles;
        float mapLength = tileSize * mapLengthInTiles;

        // South Wall
        GameObject southWall = new GameObject("SouthWall");
        southWall.transform.parent = wallsRoot.transform;
        Vector3 southStart = ToWorld(tiles[2, 0].TileCenter);
        SpawnWallLine(southStart, Vector3.right, mapWidth, segmentLength, Quaternion.identity, wallPrefab, southWall);

        // North Wall (with gate)
        GameObject northWall = new GameObject("NorthWall");
        northWall.transform.parent = wallsRoot.transform;
        Vector3 northStart = ToWorld(tiles[1, mapLengthInTiles - 1].TileCenter);
        SpawnWallLineWithGate(northStart, Vector3.right, mapWidth, segmentLength, Quaternion.Euler(0, 180, 0), wallPrefab, gatePrefab, northWall);

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

        // Towers at corners
        PlaceTowerAtTile(tiles[0, 0], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, 0], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[0, mapLengthInTiles - 1], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, mapLengthInTiles - 1], towerPrefab, wallsRoot);

        AssignWallTileOccupation(wallsRoot, tiles);
        return wallsRoot;
    }

    private void AssignWallTileOccupation(GameObject wallsRoot, Tile[,] tiles)
    {
        int count = 0;
        foreach (Transform child in wallsRoot.transform)
        {
            // 1. SprawdŸ czy sam 'child' ma BuildingData
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

            // 2. Potem sprawdŸ dzieci (jeœli istniej¹)
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

        Debug.Log($"Zaznaczono tile zajête przez mury (AssignOccupiedTiles) – {count} obiektów.");
    }

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

    private float GetSegmentLength(GameObject go)
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

    private Vector3 ToWorld(Vector2 tileCenter)
    {
        Vector3 pos = new(tileCenter.x, 0, tileCenter.y);
        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
        return pos;
    }

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

    private void SpawnWallLineWithGate(Vector3 origin, Vector3 direction, float totalLength, float segmentLength, Quaternion rotation, GameObject wallPrefab, GameObject gatePrefab, GameObject parent)
    {
        int count = Mathf.FloorToInt(totalLength / segmentLength);
        int gateIndex = count / 2;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = origin + direction * segmentLength * i;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            GameObject prefabToUse = (i == gateIndex) ? gatePrefab : wallPrefab;
            if (i == gateIndex) pos += direction * 2f;

            Object.Instantiate(prefabToUse, pos, rotation, parent.transform);
        }
    }

    private void PlaceTowerAtTile(Tile tile, GameObject prefab, GameObject parent)
    {
        Vector3 pos = ToWorld(tile.TileCenter);
        GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity, parent.transform);
        tile.IsPartOfBuilding = true;
        tile.Building = go;
    }
}
