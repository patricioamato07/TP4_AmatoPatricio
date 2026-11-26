using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public static readonly Vector2Int[] Dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    [Min(1)] public int width = 10;
    [Min(1)] public int height = 8;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    readonly Dictionary<Vector2Int, Unit> occupants = new();
    readonly Dictionary<int, List<GameObject>> highlights = new();
    readonly Dictionary<int, Material> highlightMats = new();

    void Awake() { Instance = this; }

    // ---------- Conversión ----------
    public Vector3 CellToWorld(Vector2Int c) =>
        origin + new Vector3((c.x + 0.5f) * cellSize, 0f, (c.y + 0.5f) * cellSize);

    public Vector2Int WorldToCell(Vector3 p)
    {
        var l = p - origin;
        return new Vector2Int(Mathf.FloorToInt(l.x / cellSize), Mathf.FloorToInt(l.z / cellSize));
    }

    public Vector3 Center => CellToWorld(new Vector2Int(width / 2, height / 2));
    public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
    public static int Distance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    public bool TryGetMouseCell(Camera cam, out Vector2Int cell)
    {
        var plane = new Plane(Vector3.up, origin);
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float d))
        {
            cell = WorldToCell(ray.GetPoint(d));
            return InBounds(cell);
        }
        cell = default;
        return false;
    }

    // ---------- Ocupación ----------
    public void Place(Unit u, Vector2Int c) { occupants[c] = u; u.Cell = c; }
    public void Move(Unit u, Vector2Int to) { Remove(u); Place(u, to); }
    public void Remove(Unit u) { if (occupants.TryGetValue(u.Cell, out var o) && o == u) occupants.Remove(u.Cell); }
    public Unit GetUnitAt(Vector2Int c) => occupants.TryGetValue(c, out var u) ? u : null;
    public bool IsFree(Vector2Int c) => InBounds(c) && !occupants.ContainsKey(c);

    // ---------- Pathfinding (BFS) ----------
    // Devuelve cameFrom: las claves son las celdas alcanzables (incluye el inicio).
    public Dictionary<Vector2Int, Vector2Int> GetReachable(Vector2Int start, int range)
    {
        var came = new Dictionary<Vector2Int, Vector2Int> { [start] = start };
        var dist = new Dictionary<Vector2Int, int> { [start] = 0 };
        var q = new Queue<Vector2Int>();
        q.Enqueue(start);
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (dist[c] >= range) continue;
            foreach (var d in Dirs)
            {
                var n = c + d;
                if (!InBounds(n) || dist.ContainsKey(n) || occupants.ContainsKey(n)) continue;
                dist[n] = dist[c] + 1;
                came[n] = c;
                q.Enqueue(n);
            }
        }
        return came;
    }

    public List<Vector2Int> BuildPath(Dictionary<Vector2Int, Vector2Int> came, Vector2Int goal)
    {
        var path = new List<Vector2Int>();
        if (!came.ContainsKey(goal)) return path;
        var c = goal;
        while (came[c] != c) { path.Add(c); c = came[c]; }
        path.Reverse();
        return path;
    }

    // ---------- Resaltado (canal 0 = rango, 1 = área en ejecución, 2 = hover) ----------
    public void Highlight(int channel, IEnumerable<Vector2Int> cells, Color color)
    {
        Clear(channel);
        if (!highlightMats.TryGetValue(channel, out var mat))
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            highlightMats[channel] = mat;
        }
        mat.color = color;
        var list = new List<GameObject>();
        foreach (var c in cells)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            go.name = "Highlight";
            go.transform.SetParent(transform);
            go.transform.position = CellToWorld(c) + Vector3.up * (0.02f + 0.01f * channel);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = Vector3.one * cellSize * 0.92f;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            list.Add(go);
        }
        highlights[channel] = list;
    }

    public void Clear(int channel)
    {
        if (!highlights.TryGetValue(channel, out var list)) return;
        foreach (var go in list) if (go) Destroy(go);
        list.Clear();
    }

    public void ClearAll() { foreach (var k in new List<int>(highlights.Keys)) Clear(k); }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.35f);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Gizmos.DrawWireCube(CellToWorld(new Vector2Int(x, y)), new Vector3(cellSize, 0.01f, cellSize));
    }
}
