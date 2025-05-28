using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Tile HisMainTile = null;
    public NPC HisNPC = null;
    public BuildingType HisType = BuildingType.None;
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


    public void AssignOccupiedTiles(float tileSize, Tile[,] tiles)
    {
        HisTiles.Clear();

        // Zakładamy, że MapGenerator.Instance i terrain są dostępne
        if (MapGenerator.Instance == null || MapGenerator.Instance.terrain == null)
        {
            Debug.LogWarning("MapGenerator or terrain missing.");
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combinedBounds.Encapsulate(renderers[i].bounds);

        Vector3 min = combinedBounds.min;
        Vector3 max = combinedBounds.max;

        // Zakres tile’i (światowe pozycje → indeksy)
        int startX = Mathf.FloorToInt((min.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int endX = Mathf.FloorToInt((max.x - MapGenerator.Instance.transform.position.x + MapGenerator.Instance.mapWidth / 2f) / tileSize);
        int startZ = Mathf.FloorToInt((min.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);
        int endZ = Mathf.FloorToInt((max.z - MapGenerator.Instance.transform.position.z + MapGenerator.Instance.mapLength / 2f) / tileSize);

        // Zaznacz zajęte tile’e
        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                if (x < 0 || z < 0 || x >= MapGenerator.Instance.mapWidthInTiles || z >= MapGenerator.Instance.mapLengthInTiles)
                    continue;

                Tile tile = tiles[x, z];
                if (tile != null)
        if (HisType != BuildingType.Wall && HisType != BuildingType.Gate && HisType != BuildingType.Tower)
        {
                {
                    tile.IsPartOfBuilding = true;
                    tile.Building = gameObject;
                    HisTiles.Add(tile);
                }
            }
        }
    }


}
