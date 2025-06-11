using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClueSpawner
{
    private readonly Terrain terrain;
    private readonly int tileSize;

    public ClueSpawner(Terrain terrain, int tileSize)
    {
        this.terrain = terrain;
        this.tileSize = tileSize;
    }

    public void SpawnClues(Tile[,] tiles, List<Tile> allTiles, List<ClueSetup> Clues)
    {
        var MurdererNPCBuilding = GameManager.Instance.murdererNPC.HisBuilding;
        Tile ClueTile  = GetEntranceNeighborTile(MurdererNPCBuilding, tiles); //FreeTileNextToMurdererHouseTile

        //int decorationsPlaced = 0;

        Vector3 pos3 = new Vector3(ClueTile.TileCenter.x, 0, ClueTile.TileCenter.y);
        pos3.y = terrain.SampleHeight(pos3) + terrain.transform.position.y;

        int randomAngle = Random.Range(0, 360);
        Quaternion rotation = Quaternion.Euler(-90, randomAngle, 0);


        var prefab = Clues[Random.Range(0,Clues.Count)].prefab;
        var go = Object.Instantiate(prefab, pos3, rotation, terrain.transform);
        ClueTile.IsClue = true;
        ClueTile.Prefab = go;



        Debug.Log($"Clue Generator: Clue ({prefab.name})jest na tile {ClueTile.TileCenter}");
    }

    private static Tile GetEntranceNeighborTile(GameObject MurdererHouse, Tile[,] tiles)
    {
        

        Transform entrance = MurdererHouse.transform.Find("Entrance");
        if (entrance == null)
        {
            Debug.LogWarning($"Brak 'entrance' w budynku: {MurdererHouse.name}");
            return null;
        }

        Vector3 entranceWorldPos = entrance.position;
        float minDist = float.MaxValue;
        Tile closest = null;

        foreach (var tile in tiles)
        {
            if (tile == null || tile.IsPartOfBuilding || tile.IsPath || tile.IsDecoration) continue;

            Vector3 tileWorldPos = new Vector3(tile.TileCenter.x, entranceWorldPos.y, tile.TileCenter.y);
            float dist = Vector3.SqrMagnitude(entranceWorldPos - tileWorldPos);

            if (dist < minDist)
            {
                minDist = dist;
                closest = tile;
            }
        }

        return closest;
    }
}
