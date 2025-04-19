//MapGenerator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    public NavMeshSurface surface;
    public Terrain terrain;
    public int tileSize = 10;
    public int mapWidthInTiles = 10, mapLengthInTiles = 10;
    [Range(0f, 1f)]
    public float buildingsDensity = 0.2f;

    Tile[,] tiles;

    public List<Building> buildings = new();
    public List<GameObject> spawnedBuildings = new();

    private List<Tile> buildingTiles;
    private int n;
    private float[,] d;
    private Vector2[,] bestStart, bestGoal;
    private List<int> nnPath, optPath;


    private float[,,] baseAlphaMap;
    //private float[,,] currentAlphaMap;
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapLength;

    void Awake()
    {
        Instance = this;
        tiles = new Tile[mapWidthInTiles, mapLengthInTiles];

        baseAlphaMap = terrain.terrainData.GetAlphamaps(0, 0,
        terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        Vector3 newSize = new Vector3(mapWidthInTiles * tileSize, terrain.terrainData.size.y, mapLengthInTiles * tileSize);
        terrain.terrainData.size = newSize;

        Vector3 centerOffset = new Vector3(mapWidthInTiles * tileSize / 2f, 0, mapLengthInTiles * tileSize / 2f);
        terrain.transform.position = transform.position - centerOffset;
    }

    public void GenerateMap()
    {
        Debug.Log("Generating map");
        mapWidth = tileSize * mapWidthInTiles;
        mapLength = tileSize * mapLengthInTiles;
        buildingTiles = new List<Tile>();


        //Tiles init
        for (int x = 0; x < mapWidthInTiles; x++)   
        for (int z = 0; z < mapLengthInTiles; z++)
        {
            tiles[x, z] = new Tile();
            tiles[x, z].GridPosition = new Vector2Int(x * tileSize, z * tileSize);
            tiles[x, z].TileCenter = new Vector2(transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize / 2f,
                                                    transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f);
            tiles[x, z].PosXWallCenter = new Vector2(transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize,
                                                    transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f);
            var list = new List<Tile>();
            if (x > 0) list.Add(tiles[x - 1, z]);
            if (z > 0) list.Add(tiles[x , z -1]);
            if (x < mapWidthInTiles-1) list.Add(tiles[x + 1, z]);
            if (z < mapLengthInTiles-1) list.Add(tiles[x, z + 1]);


            tiles[x, z].Neighbors = list.ToArray();

            if (Random.value > buildingsDensity) continue; //spawn building decision

            Vector3 buildingPosition = new Vector3(
                transform.position.x - mapWidth / 2 + tiles[x, z].GridPosition.x + tileSize / 2f,
                0,
                transform.position.z - mapLength / 2 + tiles[x, z].GridPosition.y + tileSize / 2f
            );

            GameObject buildingPrefab = PickBuilding();

            if (buildingPrefab == null) continue; //if all bulidings spawned

            GameObject bd = Instantiate(buildingPrefab, buildingPosition, Quaternion.identity, terrain.transform);
            spawnedBuildings.Add(bd);
            tiles[x, z].IsBuilding = true;
            tiles[x, z].Building = bd;      
            buildingTiles.Add(tiles[x, z]);

        }

        //Manhattan distance matrix between buildings
        n = buildingTiles.Count;
        d = new float[n, n];
        bestStart = new Vector2[n, n];
        bestGoal = new Vector2[n, n];
        for (int i = 0; i < n; i++)
        {
            Tile startTile = buildingTiles[i];
            for (int j = 0; j < n; j++)
            {
                Tile endTile = buildingTiles[j];
                if (i == j)
                {
                    d[i, j] = 0f;
                    bestStart[i, j] = startTile.TileCenter;
                    bestGoal[i, j] = startTile.TileCenter;
                    continue;
                }

                float bestDist = float.MaxValue;
                Vector2 bp = default, bq = default;
                foreach (Tile p in startTile.Neighbors)
                {
                    if(p == null) continue;
                    if (p.IsBuilding) continue;
                    foreach (Tile q in endTile.Neighbors)
                    {
                        if (q == null) continue;
                        if (q.IsBuilding) continue;
                        float dist = Mathf.Abs(p.TileCenter.x - q.TileCenter.x)
                                   + Mathf.Abs(p.TileCenter.y - q.TileCenter.y);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bp = p.TileCenter;
                            bq = q.TileCenter;
                        }
                    }
                }
                if (bestDist == float.MaxValue)
                    bestDist = float.MaxValue / 2f;

                d[i, j] = bestDist;
                bestStart[i, j] = bp;
                bestGoal[i, j] = bq;
            }
        }
        nnPath = NearestNeighbor();
        optPath = TwoOpt(nnPath);

        var fullPath = new List<Vector2Int>();
        for (int k = 0; k < optPath.Count - 1; k++)
        {
            // światowe współrzędne punktów sparowanych
            Vector2 wFrom = bestStart[optPath[k], optPath[k + 1]];
            Vector2 wTo = bestGoal[optPath[k], optPath[k + 1]];

            // zamiana world→grid indices
            Vector2Int gFrom = WorldToGrid(wFrom);
            Vector2Int gTo = WorldToGrid(wTo);

            // BFS omijający budynki
            var segment = BFSPath(gFrom, gTo);
            if (fullPath.Count == 0)
                fullPath.AddRange(segment);
            else
                fullPath.AddRange(segment.Skip(1));  // pomiń powtórzony węzeł
        }

        // 2) Narysuj każdy ruch bokiem
        foreach (var (a, b) in fullPath.Zip(fullPath.Skip(1), (a, b) => (a, b)))
        {
            Vector2 cA = tiles[a.x, a.y].TileCenter;
            Vector2 cB = tiles[b.x, b.y].TileCenter;
            PaintPath(cA, cB, 1.0f, 1);
        }



        foreach (var tile in tiles)
        {
            if (tile.IsBuilding)
            {
                PaintPath(tile.TileCenter, tile.PosXWallCenter, 0.7f, 1);
            }

        }
        surface.BuildNavMesh();
    }
    private GameObject PickBuilding()
    {
        List<Building> available = buildings.FindAll(b => b.currentCount < b.maxCount);
        if (available.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var b in available) totalWeight += b.weight;

        float rnd = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var b in available)
        {
            cumulative += b.weight;
            if (rnd <= cumulative)
            {
                b.currentCount++;
                return b.prefab;
            }
        }

        return null;
    }
    private void OnDestroy()
    {
        terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    }

    //private void CleanMap()
    //{
    //    terrain.terrainData.SetAlphamaps(0, 0, baseAlphaMap);
    //}
    public void PaintPath(Vector2 start, Vector2 end, float radius, int textureLayerIndex)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int alphaWidth = data.alphamapWidth;
        int alphaHeight = data.alphamapHeight;
        int numLayers = data.alphamapLayers;

        float[,,] alphas = data.GetAlphamaps(0, 0, alphaWidth, alphaHeight);

        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (radius * 0.5f)); // krok co pół promienia
        float mapRadius = (radius / data.size.x) * alphaWidth;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            //WARNING coords are switched, in other way paths are painted in mirror Tile. IDK where is bug
            int mapZ = (int)(((point.x - terrainPos.x) / data.size.x) * alphaWidth);
            int mapX = (int)(((point.y - terrainPos.z) / data.size.z) * alphaHeight);

            PaintCircle(alphas, mapX, mapZ, mapRadius, textureLayerIndex, numLayers);
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    private void PaintCircle(float[,,] alphas, int centerX, int centerZ, float radius, int texIndex, int numTextures)
    {
        int r = Mathf.CeilToInt(radius);
        for (int z = -r; z <= r; z++)
        {
            for (int x = -r; x <= r; x++)
            {
                int px = centerX + x;
                int pz = centerZ + z;

                if (px < 0 || pz < 0 || px >= alphas.GetLength(0) || pz >= alphas.GetLength(1)) continue;

                if (x * x + z * z <= radius * radius)
                {
                    for (int i = 0; i < numTextures; i++)
                        alphas[px, pz, i] = (i == texIndex) ? 1f : 0f;
                }
            }
        }
    }
    private float PathLength(List<int> path)
    {
        float len = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            len += d[path[i], path[i + 1]];
        return len;
    }

    private List<int> NearestNeighbor()
    {
        var Path = new List<int> { 0 };
        var unvisited = new HashSet<int>();
        for (int i = 1; i < n; i++) unvisited.Add(i);
        int current = 0;

        while (unvisited.Count > 0)
        {
            int next = -1;
            float best = float.MaxValue;
            foreach (int v in unvisited)
            {
                if (d[current, v] < best)
                {
                    best = d[current, v];
                    next = v;
                }
            }
            Path.Add(next);
            unvisited.Remove(next);
            current = next;
        }
        return Path;
    }

    private List<int> TwoOpt(List<int> path)
    {
        bool improved = true;
        while (improved)
        {
            improved = false;
            for (int i = 1; i < path.Count - 2; i++)
            {
                for (int j = i + 1; j < path.Count; j++)
                {
                    if (j - i == 1) continue;
                    var newPath = new List<int>(path);
                    newPath.Reverse(i, j - i);
                    if (PathLength(newPath) < PathLength(path))
                    {
                        path = newPath;
                        improved = true;
                    }
                }
            }
        }
        return path;
    }
    private Vector2Int WorldToGrid(Vector2 world)
    {
        // odwrotność obliczenia TileCenter → indeks kafelka
        float fx = (world.x - (transform.position.x - mapWidth / 2) - tileSize / 2f) / tileSize;
        float fz = (world.y - (transform.position.z - mapLength / 2) - tileSize / 2f) / tileSize;
        return new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(fx), 0, mapWidthInTiles - 1),
                              Mathf.Clamp(Mathf.RoundToInt(fz), 0, mapLengthInTiles - 1));
    }

    private List<Vector2Int> BFSPath(Vector2Int start, Vector2Int goal)
    {
        var dirs = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        prev[start] = start;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == goal) break;
            foreach (var d in dirs)
            {
                var nb = cur + d;
                if (nb.x < 0 || nb.x >= mapWidthInTiles || nb.y < 0 || nb.y >= mapLengthInTiles) continue;
                if (prev.ContainsKey(nb) || tiles[nb.x, nb.y].IsBuilding) continue;
                prev[nb] = cur;
                queue.Enqueue(nb);
            }
        }

        var path = new List<Vector2Int>();
        if (!prev.ContainsKey(goal)) return path;
        var node = goal;
        while (true)
        {
            path.Add(node);
            if (node == start) break;
            node = prev[node];
        }
        path.Reverse();
        return path;
    }

}
public class Tile
{
    public Vector2Int GridPosition;
    public Vector2 TileCenter;
    public Vector2 PosXWallCenter;
    public Tile[] Neighbors = new Tile[0];
    public GameObject TileObject;
    public GameObject Building;
    public bool IsBuilding = false;
}

[Serializable]
public class Building
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 0.1f;
    public int maxCount = 3;
    public int occurenceRadius = 5;
    [HideInInspector] public int currentCount = 0;
    //[HideInInspector] public Vector3 builidingCoordinates = Vector3.zero;
}

