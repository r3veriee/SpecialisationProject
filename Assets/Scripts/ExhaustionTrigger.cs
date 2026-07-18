using UnityEngine;

public class ExhaustionTrigger : MonoBehaviour
{
    public float exhaustedSpeedLimit = 3f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MovementTech movement = other.GetComponent<MovementTech>();
            if (movement != null)
            {
                movement.canDash = false;
                movement.dashCooldownTimer = 999f;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // only clamp horizontal speed so they can still fall down the hole normally
                Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (flatVel.magnitude > exhaustedSpeedLimit)
                {
                    Vector3 clampedVel = flatVel.normalized * exhaustedSpeedLimit;
                    rb.linearVelocity = new Vector3(clampedVel.x, rb.linearVelocity.y, clampedVel.z);
                }
            }
        }
    }
}