using UnityEngine;

public class DeadlyHazard : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Try to find the Death Manager for the cool cinematic sequence
            DeathSequenceManager deathManager = FindObjectOfType<DeathSequenceManager>();
            if (deathManager != null)
            {
                deathManager.TriggerDeath();
            }
            else
            {
                // Fallback: If no manager is in the scene, just do a normal respawn
                CheckpointSystem cp = other.GetComponent<CheckpointSystem>();
                if (cp != null) cp.Respawn();
            }
        }
    }
}