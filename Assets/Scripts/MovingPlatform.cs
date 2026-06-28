using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointStart;
    public Transform pointEnd;
    public float speed = 8f;

    private Vector3 target;
    private Vector3 previousPosition;

    private List<Transform> objectsOnPlatform = new List<Transform>();

    void Start()
    {
        target = pointEnd.position;
        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Move the platform
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

        // Calculate the exact distance moved this frame
        Vector3 distanceMoved = transform.position - previousPosition;
        previousPosition = transform.position;

        // Force the player to move that exact same distance
        foreach (Transform obj in objectsOnPlatform)
        {
            if (obj != null) obj.position += distanceMoved;
        }

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = target == pointStart.position ? pointEnd.position : pointStart.position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !objectsOnPlatform.Contains(other.transform))
        {
            objectsOnPlatform.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (objectsOnPlatform.Contains(other.transform))
        {
            objectsOnPlatform.Remove(other.transform);
        }
    }
}