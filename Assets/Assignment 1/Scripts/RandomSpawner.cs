using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private List<GameObject> spawnPrefabs;
    [SerializeField] private int numberOfObjectsToSpawn = 40;
    [SerializeField] private int maxAttempts = 2000;

    // NEW: weights, same order as the prefab list above (higher = spawns more often)
    [Header("Bonus: Weighted Probabilities")]
    [Tooltip("One weight per prefab, same order as Spawn Prefabs. Higher = more common. 0 = never spawns.")]
    [SerializeField] private List<float> prefabWeights = new List<float>();

    // NEW: minimum distance
    [Header("Bonus: Spacing")]
    [Tooltip("Minimum distance between two spawned objects. 0 = no limit.")]
    [SerializeField] private float minDistance = 2f;

    [Header("Areas")]
    [SerializeField] private Transform baseRoot;   // drag "Base"
    [SerializeField] private Transform houseRoot;  // drag "House"
    [SerializeField] private float edgeMargin = 1f;    // keep this far inside the Base edge
    [SerializeField] private float housePadding = 1f;  // keep this far away from the house

    [Header("Ground")]
    [SerializeField] private float groundYOffset = 0f;    // raise (+) or lower (-) objects placed on found ground
    [SerializeField] private float fallbackYOffset = 0f;  // used only when no ground is found under a point

    [Header("Parenting")]
    [SerializeField] private string parentName = "Environment Prefabs";

    private List<Vector2> baseHull;
    private List<Vector2> houseHull;
    private Vector2 sampleMin, sampleMax;
    private float spawnY;

    // NEW: keeps the weights list the same length as the prefab list (new entries start at 1)
    void OnValidate()
    {
        if (spawnPrefabs == null) return;
        if (prefabWeights == null) prefabWeights = new List<float>();

        while (prefabWeights.Count < spawnPrefabs.Count)
            prefabWeights.Add(1f);
        while (prefabWeights.Count > spawnPrefabs.Count)
            prefabWeights.RemoveAt(prefabWeights.Count - 1);
    }

    void Start()
    {
        if (baseRoot == null)
        {
            Debug.LogError("RandomSpawner: Assign Base Root!");
            return;
        }

        baseHull = BuildHull(baseRoot, out sampleMin, out sampleMax, out spawnY);
        if (houseRoot != null)
            houseHull = BuildHull(houseRoot, out _, out _, out _);

        GameObject parentObj = GameObject.Find(parentName);
        if (parentObj == null) parentObj = new GameObject(parentName);

        SpawnObjects(parentObj.transform);
    }

    void SpawnObjects(Transform parent)
    {
        if (spawnPrefabs == null || spawnPrefabs.Count == 0)
        {
            Debug.LogWarning("RandomSpawner: No prefabs assigned!");
            return;
        }

        int spawned = 0, attempts = 0;
        int noGroundCount = 0;
        var placed = new List<Vector2>(); // NEW: positions used so far
        float minDistSqr = minDistance * minDistance; // NEW

        while (spawned < numberOfObjectsToSpawn && attempts < maxAttempts)
        {
            attempts++;

            Vector2 p = new Vector2(
                Random.Range(sampleMin.x, sampleMax.x),
                Random.Range(sampleMin.y, sampleMax.y));

            // must be at least edgeMargin inside the Base
            if (!IsInsideHull(p, baseHull, edgeMargin)) continue;

            // reject if inside the (padded) house
            if (houseHull != null && IsInsideHull(p, houseHull, -housePadding)) continue;

            // NEW: reject if too close to an already spawned object
            if (minDistance > 0f && IsTooClose(p, placed, minDistSqr)) continue;

            // find the ground height at this point, fall back to the old height if none found
            float y;
            if (TryGetGroundY(p, parent, out float groundY))
                y = groundY + groundYOffset;
            else
            {
                y = spawnY + fallbackYOffset;
                noGroundCount++;
            }

            GameObject prefab = PickWeightedPrefab(); // CHANGED: was a uniform random pick
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, new Vector3(p.x, y, p.y), rot, parent);
            placed.Add(p); // NEW
            spawned++;
        }

        if (spawned < numberOfObjectsToSpawn)
            Debug.LogWarning($"RandomSpawner: only spawned {spawned}/{numberOfObjectsToSpawn}. Try a smaller Min Distance or fewer objects.");

        if (noGroundCount > 0)
            Debug.LogWarning($"RandomSpawner: no ground collider found under {noGroundCount} objects, used the Base height instead.");
    }

    // NEW: true if p is closer than the minimum distance to any already placed position
    private static bool IsTooClose(Vector2 p, List<Vector2> placed, float minDistSqr)
    {
        for (int i = 0; i < placed.Count; i++)
        {
            if ((placed[i] - p).sqrMagnitude < minDistSqr) return true;
        }
        return false;
    }

    // NEW: weight of prefab i (missing or empty prefab = 0, missing weight = 1)
    private float GetWeight(int i)
    {
        if (spawnPrefabs[i] == null) return 0f;
        if (prefabWeights == null || i >= prefabWeights.Count) return 1f;
        return Mathf.Max(0f, prefabWeights[i]);
    }

    // NEW: picks a prefab with probability proportional to its weight
    private GameObject PickWeightedPrefab()
    {
        float total = 0f;
        for (int i = 0; i < spawnPrefabs.Count; i++)
            total += GetWeight(i);

        // all weights 0: fall back to a uniform pick so nothing breaks
        if (total <= 0f)
            return spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];

        float roll = Random.Range(0f, total);
        float running = 0f;
        int lastValid = 0;
        for (int i = 0; i < spawnPrefabs.Count; i++)
        {
            float w = GetWeight(i);
            if (w <= 0f) continue;
            running += w;
            lastValid = i;
            if (roll < running) return spawnPrefabs[i];
        }
        return spawnPrefabs[lastValid];
    }

    // shoots a ray straight down and returns the height of the first real ground surface
    private bool TryGetGroundY(Vector2 p, Transform spawnedParent, out float groundY)
    {
        groundY = 0f;
        Vector3 origin = new Vector3(p.x, spawnY + 50f, p.y);
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 300f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform t = hit.transform;
            if (t.IsChildOf(baseRoot)) continue;                          // ignore the Base cubes
            if (houseRoot != null && t.IsChildOf(houseRoot)) continue;    // ignore the house
            if (t.IsChildOf(spawnedParent)) continue;                     // ignore already spawned objects

            groundY = hit.point.y;
            return true;
        }
        return false;
    }

    // ---------- geometry helpers ----------

    // World-space XZ convex hull of all renderers under 'root'
    private static List<Vector2> BuildHull(Transform root, out Vector2 min, out Vector2 max, out float topY)
    {
        var pts = new List<Vector2>();
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);
        topY = float.MinValue;

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            Bounds lb = r.localBounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = lb.center + Vector3.Scale(lb.extents, new Vector3(
                    (i & 1) == 0 ? -1 : 1,
                    (i & 2) == 0 ? -1 : 1,
                    (i & 4) == 0 ? -1 : 1));
                Vector3 w = r.transform.TransformPoint(corner);

                pts.Add(new Vector2(w.x, w.z));
                min = Vector2.Min(min, new Vector2(w.x, w.z));
                max = Vector2.Max(max, new Vector2(w.x, w.z));
                topY = Mathf.Max(topY, w.y);
            }
        }
        return ConvexHull(pts);
    }

    // true if p is at least 'margin' away from every edge, on the inside (negative margin = grow the shape)
    private static bool IsInsideHull(Vector2 p, List<Vector2> hull, float margin)
    {
        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a = hull[i];
            Vector2 b = hull[(i + 1) % hull.Count];
            Vector2 e = b - a;
            float dist = (e.x * (p.y - a.y) - e.y * (p.x - a.x)) / e.magnitude;
            if (dist < margin) return false;
        }
        return true;
    }

    // Andrew's monotone chain, returns counter-clockwise hull
    private static List<Vector2> ConvexHull(List<Vector2> pts)
    {
        pts.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        var h = new List<Vector2>();

        for (int i = 0; i < pts.Count; i++)
        {
            while (h.Count >= 2 && Cross(h[h.Count - 2], h[h.Count - 1], pts[i]) <= 0)
                h.RemoveAt(h.Count - 1);
            h.Add(pts[i]);
        }
        int lower = h.Count + 1;
        for (int i = pts.Count - 2; i >= 0; i--)
        {
            while (h.Count >= lower && Cross(h[h.Count - 2], h[h.Count - 1], pts[i]) <= 0)
                h.RemoveAt(h.Count - 1);
            h.Add(pts[i]);
        }
        h.RemoveAt(h.Count - 1);
        return h;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    // Draws the hulls in the Scene view during Play mode so you can verify them
    void OnDrawGizmosSelected()
    {
        DrawHull(baseHull, Color.green);
        DrawHull(houseHull, Color.red);
    }

    private void DrawHull(List<Vector2> hull, Color c)
    {
        if (hull == null) return;
        Gizmos.color = c;
        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a = hull[i], b = hull[(i + 1) % hull.Count];
            Gizmos.DrawLine(new Vector3(a.x, spawnY, a.y), new Vector3(b.x, spawnY, b.y));
        }
    }
}
