using UnityEngine;

public class Fly : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private float margin = 1.5f;                       // distance kept outside the truck
    [SerializeField] private Vector2 extraHeightRange = new Vector2(0.5f, 2.5f); // above the truck's roof
    [SerializeField] private Vector2 speedRange = new Vector2(60f, 120f);  // degrees per second
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float bobSpeed = 4f;

    private float radiusX, radiusZ, height, angularSpeed, angle, bobPhase;
    private Vector3 center;

    void Start()
    {
        Transform truck = transform.parent;

        // Measure the truck in its own local space (ignoring flies)
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        bool first = true;

        if (truck != null)
        {
            foreach (Renderer r in truck.GetComponentsInChildren<Renderer>())
            {
                if (r.GetComponentInParent<Fly>() != null) continue;

                Bounds lb = r.localBounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = lb.center + Vector3.Scale(lb.extents, new Vector3(
                        (i & 1) == 0 ? -1 : 1,
                        (i & 2) == 0 ? -1 : 1,
                        (i & 4) == 0 ? -1 : 1));
                    Vector3 p = truck.InverseTransformPoint(r.transform.TransformPoint(corner));

                    if (first) { b = new Bounds(p, Vector3.zero); first = false; }
                    else b.Encapsulate(p);
                }
            }
        }

        if (first) b = new Bounds(Vector3.zero, new Vector3(3f, 3f, 8f)); // fallback

        // Ellipse that fully encloses the truck's footprint, plus a margin
        center = b.center;
        radiusX = b.extents.x * 1.4142f + margin;
        radiusZ = b.extents.z * 1.4142f + margin;
        height = b.max.y + Random.Range(extraHeightRange.x, extraHeightRange.y);

        angularSpeed = Random.Range(speedRange.x, speedRange.y);
        angle = Random.Range(0f, 360f);
        bobPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        angle = (angle + angularSpeed * Time.deltaTime) % 360f;
        float rad = angle * Mathf.Deg2Rad;

        float y = height + Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmount;
        transform.localPosition = new Vector3(
            center.x + Mathf.Cos(rad) * radiusX,
            y,
            center.z + Mathf.Sin(rad) * radiusZ);

        // Face the direction of travel along the ellipse
        Vector3 tangent = new Vector3(-Mathf.Sin(rad) * radiusX, 0f, Mathf.Cos(rad) * radiusZ);
        transform.localRotation = Quaternion.LookRotation(tangent);
    }
}