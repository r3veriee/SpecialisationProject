using UnityEngine;

public class BoostHoop : MonoBehaviour
{
    public float velocityMultiplier = 1.5f; // 50% speed boost
    public float maxSpeedCap = 200f; // High cap to allow crazy tech chains

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 boostedVelocity = rb.linearVelocity * velocityMultiplier;

                if (boostedVelocity.magnitude > maxSpeedCap)
                {
                    boostedVelocity = boostedVelocity.normalized * maxSpeedCap;
                }

                rb.linearVelocity = boostedVelocity;
            }
        }
    }
}