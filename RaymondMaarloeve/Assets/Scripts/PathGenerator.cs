// PathGenerator.cs

using System.Collections.Generic;
using UnityEngine;

public static class PathGenerator
{
    public static void ClearMap(Terrain terrain)
    {
        var data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        // Utworzenie nowej tablicy [w,h,layers]
        float[,,] alphas = new float[w, h, layers];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                alphas[x, y, 0] = 1f;      // pierwsza warstwa
                                           // pozostałe warstwy (1..layers-1) już są 0
            }
        }

        // Nadpisanie całego terenu
        data.SetAlphamaps(0, 0, alphas);
    }
    /// <summary>
    /// Wyznacza trasy między buildingTiles i maluje je na terrain.
    /// </summary>
    public static void GeneratePaths(Tile[,] tiles, List<Tile> buildingTiles, Terrain terrain)
    {
        // === KROK 1: Znalezienie punktów połączeń (najbliższych kafelków przy wejściu) ===
        List<Tile> connectionTiles = new List<Tile>();

        foreach (var buildingTile in buildingTiles)
        {
            Tile entryTile = GetEntranceNeighbor(buildingTile, tiles);
            if (entryTile != null)
            {
                connectionTiles.Add(entryTile);
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono wejścia dla budynku: {buildingTile.Building?.name}");
            }
        }

        int n = connectionTiles.Count;
        if (n < 2) return;  // za mało punktów do łączenia

        // === KROK 2: Obliczanie odległości między punktami wejściowymi ===
        float[,] d = new float[n, n];
        Tile[,] bestStartTile = new Tile[n, n];
        Tile[,] bestGoalTile = new Tile[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                var startTile = connectionTiles[i];
                var endTile = connectionTiles[j];

                if (i == j)
                {
                    d[i, j] = 0f;
                    bestStartTile[i, j] = startTile;
                    bestGoalTile[i, j] = endTile;
                    continue;
                }

                float dist = Mathf.Abs(startTile.TileCenter.x - endTile.TileCenter.x)
                           + Mathf.Abs(startTile.TileCenter.y - endTile.TileCenter.y);

                d[i, j] = dist;
                bestStartTile[i, j] = startTile;
                bestGoalTile[i, j] = endTile;
            }
        }

        // === KROK 3: Optymalizacja kolejności odwiedzania punktów ===
        var nnPath = NearestNeighbor(d, n);
        var optPath = TwoOpt(nnPath, d);

        // === KROK 4: Budowanie pełnej ścieżki ===
        foreach (var t in tiles) t.IsPath = false;
        var fullPath = new List<Tile>();
        Tile lastTile = null;

        for (int k = 0; k < optPath.Count - 1; k++)
        {
            Tile p = (k == 0)
                ? bestStartTile[optPath[k], optPath[k + 1]]
                : lastTile;
            Tile q = bestGoalTile[optPath[k], optPath[k + 1]];

            var segment = FindPath(p, q);
            if (segment.Count == 0) continue;
            if (k > 0) segment.RemoveAt(0);

            fullPath.AddRange(segment);
            lastTile = fullPath[fullPath.Count - 1];
            foreach (var t in segment) t.IsPath = true;
        }

        // === KROK 5: Rysowanie ścieżek i ustawianie wejść ===
        foreach (var t in tiles)
        {
            if (t.IsBuilding)
            {
                foreach (var nb in t.Neighbors)
                {
                    if (nb != null && nb.IsPath)
                    {
                        PaintPath(terrain, t.TileCenter, nb.TileCenter, 0.7f, 1);
                        RotateBuilding(t.Building, t.TileCenter, nb.TileCenter);
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
                    {
                        PaintPath(terrain, t.TileCenter, nb.TileCenter, 2.0f, 1);
                    }
                }
            }
        }
    }
    private static Tile GetEntranceNeighbor(Tile buildingTile, Tile[,] tiles)
    {
        if (buildingTile.Building == null)
            return null;

        Transform entrance = buildingTile.Building.transform.Find("Entrance");
        if (entrance == null)
        {
            Debug.LogWarning($"Brak 'entrance' w budynku: {buildingTile.Building.name}");
            return null;
        }

        Vector3 entranceWorldPos = entrance.position;
        float minDist = float.MaxValue;
        Tile closest = null;

        foreach (var tile in tiles)
        {
            if (tile == null || tile.IsPartOfBuilding) continue;

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


    private static List<int> NearestNeighbor(float[,] d, int n)
    {
        var path = new List<int> { 0 };
        var unv = new HashSet<int>();
        for (int i = 1; i < n; i++) unv.Add(i);
        int cur = 0;

        while (unv.Count > 0)
        {
            int next = -1;
            float best = float.MaxValue;
            foreach (int v in unv)
            {
                if (d[cur, v] < best)
                {
                    best = d[cur, v];
                    next = v;
                }
            }
            path.Add(next);
            unv.Remove(next);
            cur = next;
        }

        return path;
    }

    private static List<int> TwoOpt(List<int> path, float[,] d)
    {
        bool improved = true;
        int len = path.Count;

        while (improved)
        {
            improved = false;
            for (int i = 1; i < len - 2; i++)
            {
                for (int j = i + 1; j < len; j++)
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

    private static float PathLength(List<int> path, float[,] d)
    {
        float sum = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            sum += d[path[i], path[i + 1]];
        return sum;
    }

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

    private static void PaintPath(Terrain terrain, Vector2 start, Vector2 end, float radius, int layer)
    {
        var data = terrain.terrainData;
        var tPos = terrain.transform.position;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;
        var alphas = data.GetAlphamaps(0, 0, w, h);

        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (radius * 0.5f));
        float mapRad = (radius / data.size.x) * w;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 pt = Vector2.Lerp(start, end, t);

            // odbicie X↔Z, jak w oryginalnym skrypcie
            int mapZ = (int)(((pt.x - tPos.x) / data.size.x) * w);
            int mapX = (int)(((pt.y - tPos.z) / data.size.z) * h);

            PaintCircle(alphas, mapX, mapZ, mapRad, layer, layers);
        }

        data.SetAlphamaps(0, 0, alphas);
    }

    public static void PaintCircle(float[,,] alphas, int cx, int cz, float rad, int li, int lt)
    {
        int r = Mathf.CeilToInt(rad);
        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int px = cx + dx;
                int pz = cz + dz;
                if (px < 0 || pz < 0 || px >= alphas.GetLength(0) || pz >= alphas.GetLength(1)) continue;
                if (dx * dx + dz * dz <= rad * rad)
                    for (int l = 0; l < lt; l++)
                        alphas[px, pz, l] = (l == li) ? 1f : 0f;
            }
    }
    private static void RotateBuilding(GameObject building, Vector2 from, Vector2 to)
    {
        float yPos = building.transform.position.y;
        Vector3 posFrom = new Vector3(from.x, yPos, from.y);
        Vector3 posTo = new Vector3(to.x, yPos, to.y);

        Vector3 dir = (posTo - posFrom).normalized;
        if (dir.sqrMagnitude < 0.001f) return;

        // 3) LookRotation
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

        // 4) Offset zależny od orientacji modelu
        //    - jeśli front modelu == +Z, offset = 0
        //    - jeśli front modelu == +X, offset = -90
        //    - jeśli front modelu == -Z, offset = 180
        //    - jeśli front modelu == -X, offset = +90
        //float angleOffset = -90f;
        //Quaternion offset = Quaternion.Euler(0, angleOffset, 0);
        //building.transform.rotation = lookRot;// * offset;

    }
}
