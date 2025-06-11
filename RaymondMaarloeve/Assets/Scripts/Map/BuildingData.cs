using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Tile HisMainTile = null;
    public List<NPC> HisNPC = null;
    public Vector2Int HisMainTileGridPosition = new Vector2Int(-1, -1); // <- grid position of the main tile
    [SerializeField] public BuildingType HisType = BuildingType.None;

    //public int WidthInTiles = 1; //{ get; private set; }
    //public int LengthInTiles = 1; //{ get; private set; }

    public List<Tile> HisTiles = new(); // <- trzymamy referencje do tile’i, które budynek zajmuje
    public int HisTileCount = 0;
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


    public bool AssignOccupiedTiles(float tileSize, Tile[,] tiles)
    {
        HisTiles.Clear();

        if (MapGenerator.Instance == null || MapGenerator.Instance.terrain == null)
        {
            Debug.LogWarning("MapGenerator or terrain missing.");
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
        // 1️ Sprawdź kolizję przed oznaczaniem
        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                if (x < 0 || z < 0 ||  x >= tiles.GetLength(0) || z >= tiles.GetLength(1))
                    return false;

                if (tiles[x, z].IsPartOfBuilding)
                    return false;
            }

        // 2️ Jeśli wolne – oznacz wszystkie tile jako zajęte
        for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Prefab = gameObject;
                HisTiles.Add(tile);
                HisTileCount++;
            }

        // 3️ Wyznacz main tile – jeśli nie Wall ani Gate
        if (HisType != BuildingType.Wall /*&& HisType != BuildingType.Gate*/ && HisType != BuildingType.Tower)
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

        return true;
    }

    public void AssignWallTilesForcefully(float tileSize, Tile[,] tiles)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return;

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);
        //Debug.LogWarning($"{name} bounds size = {combinedBounds.size}, tileSize = {tileSize}");


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
            for (int z = startZ; z <= endZ; z++)
            {
                Tile tile = tiles[x, z];
                tile.IsPartOfBuilding = true;
                tile.Prefab = gameObject;
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
    }

}
