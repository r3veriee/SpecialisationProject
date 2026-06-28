using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float bounceForce = 60f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Kill current momentum to guarantee a mathematically perfect arc
                rb.linearVelocity = Vector3.zero;

                // Launch in the direction the top of the pad is facing
                rb.linearVelocity = transform.up * bounceForce;

                // Reset dash so they can style in mid-air
                MovementTech moveScript = other.GetComponent<MovementTech>();
                if (moveScript != null) moveScript.ResetDash();
            }
        }
    }
}