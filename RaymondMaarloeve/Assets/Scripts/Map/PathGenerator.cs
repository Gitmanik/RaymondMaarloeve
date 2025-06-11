using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static utility class that handles procedural generation of walking paths between buildings,
/// including optimization of path order and painting those paths on terrain.
/// </summary>
public static class PathGenerator
{
    /// <summary>
    /// Resets the entire terrain texture to the base layer (layer 0),
    /// effectively removing all previous path markings.
    /// </summary>
    /// <param name="terrain">The terrain object to clear.</param>
    public static void ClearMap(Terrain terrain)
    {
        var data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        float[,,] alphas = new float[w, h, layers];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                alphas[x, y, 0] = 1f;

        data.SetAlphamaps(0, 0, alphas);
    }

    /// <summary>
    /// Connects buildings via optimized paths, paints paths on terrain, and rotates buildings to face connected routes.
    /// </summary>
    /// <param name="tiles">The full grid of terrain tiles.</param>
    /// <param name="buildingTiles">A list of tiles containing buildings to be connected.</param>
    /// <param name="terrain">The terrain object to paint paths on.</param>
    public static void GeneratePaths(Tile[,] tiles, List<Tile> buildingTiles, Terrain terrain)
    {
        // Step 1: Find entrance-neighbor tiles
        List<Tile> connectionTiles = new();
        foreach (var buildingTile in buildingTiles)
        {
            Tile entryTile = GetEntranceNeighbor(buildingTile, tiles);
            if (entryTile != null)
                connectionTiles.Add(entryTile);
            else
                Debug.LogWarning($"PathGenerator: No entrance found for building: {buildingTile.Prefab?.name}");
        }

        int n = connectionTiles.Count;
        if (n < 2) return;

        // Step 2: Calculate Manhattan distances between all entry tiles
        float[,] d = new float[n, n];
        Tile[,] bestStartTile = new Tile[n, n];
        Tile[,] bestGoalTile = new Tile[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                var startTile = connectionTiles[i];
                var endTile = connectionTiles[j];

                d[i, j] = (i == j) ? 0f :
                    Mathf.Abs(startTile.TileCenter.x - endTile.TileCenter.x) +
                    Mathf.Abs(startTile.TileCenter.y - endTile.TileCenter.y);

                bestStartTile[i, j] = startTile;
                bestGoalTile[i, j] = endTile;
            }

        // Step 3: Use Nearest Neighbor and 2-opt to optimize visit order
        var nnPath = NearestNeighbor(d, n);
        var optPath = TwoOpt(nnPath, d);

        // Step 4: Build actual tile-based path
        foreach (var t in tiles) t.IsPath = false;
        var fullPath = new List<Tile>();
        Tile lastTile = null;

        for (int k = 0; k < optPath.Count - 1; k++)
        {
            Tile p = (k == 0) ? bestStartTile[optPath[k], optPath[k + 1]] : lastTile;
            Tile q = bestGoalTile[optPath[k], optPath[k + 1]];

            var segment = FindPath(p, q);
            if (segment.Count == 0) continue;
            if (k > 0) segment.RemoveAt(0); // prevent duplicate node

            fullPath.AddRange(segment);
            lastTile = fullPath[^1];
            foreach (var t in segment) t.IsPath = true;
        }

        // Step 5: Paint the path and align buildings
        foreach (var t in tiles)
        {
            if (t.IsBuilding)
            {
                foreach (var nb in t.Neighbors)
                {
                    if (nb != null && nb.IsPath)
                    {
                        PaintPath(terrain, t.TileCenter, nb.TileCenter, 0.7f, 1);
                        RotateBuilding(t.Prefab, t.TileCenter, nb.TileCenter);
                        t.FrontWallCenter = (t.TileCenter + nb.TileCenter) / 2f;
                        break;
                    }
                }
            }

            if (t.IsPath)
            {
                foreach (var nb in t.Neighbors)
                {
                    if (nb != null && nb.IsPath)
                        PaintPath(terrain, t.TileCenter, nb.TileCenter, 2.0f, 1);
                }
            }
        }
    }

    /// <summary>
    /// Finds the closest tile adjacent to the building's "Entrance" transform.
    /// </summary>
    private static Tile GetEntranceNeighbor(Tile buildingTile, Tile[,] tiles)
    {
        if (buildingTile.Prefab == null) return null;
        Transform entrance = buildingTile.Prefab.transform.Find("Entrance");

        if (entrance == null)
        {
            Debug.LogWarning($"PathGenerator: Missing Entrance in prefab: {buildingTile.Prefab.name}");
            return null;
        }

        Vector3 pos = entrance.position;
        float minDist = float.MaxValue;
        Tile closest = null;

        foreach (var tile in tiles)
        {
            if (tile == null || tile.IsPartOfBuilding) continue;

            Vector3 tileWorld = new(tile.TileCenter.x, pos.y, tile.TileCenter.y);
            float dist = Vector3.SqrMagnitude(pos - tileWorld);
            if (dist < minDist)
            {
                minDist = dist;
                closest = tile;
            }
        }

        return closest;
    }

    /// <summary>
    /// Greedy Nearest-Neighbor algorithm for building order selection.
    /// </summary>
    private static List<int> NearestNeighbor(float[,] d, int n)
    {
        var path = new List<int> { 0 };
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
            path.Add(next);
            unvisited.Remove(next);
            current = next;
        }

        return path;
    }

    /// <summary>
    /// Applies 2-opt optimization to improve an existing path.
    /// </summary>
    private static List<int> TwoOpt(List<int> path, float[,] d)
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
                    if (PathLength(newPath, d) < PathLength(path, d))
                    {
                        path = newPath;
                        improved = true;
                    }
                }
            }
        }
        return path;
    }

    /// <summary>
    /// Calculates total path length for a given node visit order.
    /// </summary>
    private static float PathLength(List<int> path, float[,] d)
    {
        float sum = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            sum += d[path[i], path[i + 1]];
        return sum;
    }

    /// <summary>
    /// Performs simple BFS to find a walkable tile path.
    /// </summary>
    private static List<Tile> FindPath(Tile start, Tile goal)
    {
        var prev = new Dictionary<Tile, Tile>();
        var queue = new Queue<Tile>();
        prev[start] = start;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == goal) break;
            foreach (var nb in cur.Neighbors)
            {
                if (nb == null || nb.IsBuilding || nb.IsPartOfBuilding || prev.ContainsKey(nb))
                    continue;
                prev[nb] = cur;
                queue.Enqueue(nb);
            }
        }

        var path = new List<Tile>();
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

    /// <summary>
    /// Draws a painted path between two tiles on the terrain.
    /// </summary>
    private static void PaintPath(Terrain terrain, Vector2 start, Vector2 end, float radius, int layer)
    {
        var data = terrain.terrainData;
        var tPos = terrain.transform.position;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        var alphas = data.GetAlphamaps(0, 0, w, h);

        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (radius * 0.5f));
        float mapRad = (radius / data.size.x) * w;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 pt = Vector2.Lerp(start, end, t);

            int mapZ = (int)(((pt.x - tPos.x) / data.size.x) * w);
            int mapX = (int)(((pt.y - tPos.z) / data.size.z) * h);

            PaintCircle(alphas, mapX, mapZ, mapRad, layer, data.alphamapLayers);
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    /// <summary>
    /// Paints a circular brush area onto the terrain splatmap.
    /// </summary>
    public static void PaintCircle(float[,,] alphas, int cx, int cz, float rad, int layerIndex, int totalLayers)
    {
        int r = Mathf.CeilToInt(rad);
        for (int dz = -r; dz <= r; dz++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int px = cx + dx;
                int pz = cz + dz;
                if (px < 0 || pz < 0 || px >= alphas.GetLength(0) || pz >= alphas.GetLength(1)) continue;
                if (dx * dx + dz * dz <= rad * rad)
                    for (int l = 0; l < totalLayers; l++)
                        alphas[px, pz, l] = (l == layerIndex) ? 1f : 0f;
            }
        }
    }

    /// <summary>
    /// Rotates a building GameObject to face from → to direction using LookRotation.
    /// </summary>
    private static void RotateBuilding(GameObject building, Vector2 from, Vector2 to)
    {
        float y = building.transform.position.y;
        Vector3 posFrom = new(from.x, y, from.y);
        Vector3 posTo = new(to.x, y, to.y);

        Vector3 dir = (posTo - posFrom).normalized;
        if (dir.sqrMagnitude < 0.001f) return;

        building.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
