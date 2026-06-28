using UnityEngine;

public class DeadlyHazard : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointSystem cp = other.GetComponent<CheckpointSystem>();
            if (cp != null) cp.Respawn();
        }
    }
}