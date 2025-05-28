using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Tile HisMainTile = null;
    public NPC HisNPC = null;
    [SerializeField] public BuildingType HisType = BuildingType.None;

    //public int WidthInTiles = 1; //{ get; private set; }
    //public int LengthInTiles = 1; //{ get; private set; }

    public List<Tile> HisTiles = new(); // <- trzymamy referencje do tile’i, które budynek zajmuje

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
                if (x < 0 || z < 0 || x >= tiles.GetLength(0) || z >= tiles.GetLength(1))
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
                tile.Building = gameObject;
                HisTiles.Add(tile);
            }

        // 3️ Wyznacz main tile – jeśli nie Wall ani Gate
        if (HisType != BuildingType.Wall && HisType != BuildingType.Gate && HisType != BuildingType.Tower)
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
            MapGenerator.Instance.buildingsMainTile.Add(HisMainTile);
        }

        return true;
    }

}
