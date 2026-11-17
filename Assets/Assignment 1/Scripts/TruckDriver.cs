using System.Collections.Generic;
using UnityEngine;

public class TruckDriver : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private Transform trashbagsRoot;   // drag "Trashbags" here
    [SerializeField] private bool loopRoute = false;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;            // units per second
    [SerializeField] private float rotationSpeed = 180f;  // degrees per second
    [SerializeField] private float reachDistance = 0.3f;

    // Positions are stored up front, because destroyed bags become null
    private readonly List<Vector3> waypoints = new List<Vector3>();
    private int currentIndex = 0;

    void Start()
    {
        if (trashbagsRoot == null)
        {
            Debug.LogError("TruckDriver: Assign the Trashbags object!");
            enabled = false;
            return;
        }

        foreach (Transform bag in trashbagsRoot)
            waypoints.Add(bag.position);
    }

    void Update()
    {
        if (currentIndex >= waypoints.Count) return;

        Vector3 target = waypoints[currentIndex];
        target.y = transform.position.y;   // stay on the road, don't tilt

        Vector3 toTarget = target - transform.position;

        if (toTarget.magnitude <= reachDistance)
        {
            currentIndex++;
            if (loopRoute && currentIndex >= waypoints.Count)
                currentIndex = 0;
            return;
        }

        // Turn towards the target, then move
        Quaternion targetRotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);
    }
}