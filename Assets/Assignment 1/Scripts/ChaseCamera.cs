using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    [SerializeField] private Transform target;                          // drag the Garbage Truck here
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -12f); // x = sideways, y = up, z = behind (negative)
    [SerializeField] private float lookHeight = 2f;                     // aim this far above the truck's pivot
    [SerializeField] private float positionSmoothing = 4f;              // higher = camera sticks closer
    [SerializeField] private float rotationSmoothing = 6f;              // higher = camera turns faster

    void Start()
    {
        if (target == null) return;
        // Start already in position instead of flying in from somewhere else
        transform.position = DesiredPosition();
        transform.rotation = DesiredRotation(transform.position);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1 - e^(-k*dt) gives the same smoothing at any framerate
        float posT = 1f - Mathf.Exp(-positionSmoothing * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, DesiredPosition(), posT);
        transform.rotation = Quaternion.Slerp(transform.rotation, DesiredRotation(transform.position), rotT);
    }

    private Vector3 DesiredPosition()
    {
        // The offset turns with the truck, so the camera stays behind it
        return target.position + target.rotation * offset;
    }

    private Quaternion DesiredRotation(Vector3 fromPosition)
    {
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;
        return Quaternion.LookRotation(lookPoint - fromPosition);
    }
}