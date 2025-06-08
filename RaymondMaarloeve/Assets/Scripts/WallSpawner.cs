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

        Vector3 southStart = ToWorld(tiles[2, 0].TileCenter);
        SpawnWallLine(southStart, Vector3.right, mapWidth, segmentLength, Quaternion.identity, wallPrefab, wallsRoot);

        Vector3 northStart = ToWorld(tiles[2, mapLengthInTiles - 1].TileCenter);
        SpawnWallLineWithGate(northStart, Vector3.right, mapWidth, segmentLength, Quaternion.Euler(0, 180, 0), wallPrefab, gatePrefab, wallsRoot);

        Vector3 westStart = ToWorld(tiles[0, 0].TileCenter);
        SpawnWallLine(westStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab, wallsRoot);

        Vector3 eastStart = ToWorld(tiles[mapWidthInTiles - 1, 0].TileCenter);
        SpawnWallLine(eastStart, Vector3.forward, mapLength, segmentLength, Quaternion.Euler(0, 90, 0), wallPrefab, wallsRoot);

        PlaceTowerAtTile(tiles[0, 0], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, 0], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[0, mapLengthInTiles - 1], towerPrefab, wallsRoot);
        PlaceTowerAtTile(tiles[mapWidthInTiles - 1, mapLengthInTiles - 1], towerPrefab, wallsRoot);

        return wallsRoot;
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
