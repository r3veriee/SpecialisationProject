using UnityEngine;

public class SpireApexTrigger : MonoBehaviour
{
    [Header("Antivirus")]
    public AntivirusGrid gridToDisable;

    [Header("Exhaustion Settings")]
    public float exhaustedSpeedLimit = 5f;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (gridToDisable != null) gridToDisable.DisableGrid();

            CheckpointSystem cp = other.GetComponent<CheckpointSystem>();
            if (cp != null) cp.currentRespawnPos = transform.position;

            MovementTech move = other.GetComponent<MovementTech>();
            if (move != null)
            {
                move.canDash = false;
                move.dashCooldownTimer = 9999f;
                move.walkSpeed = exhaustedSpeedLimit;
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