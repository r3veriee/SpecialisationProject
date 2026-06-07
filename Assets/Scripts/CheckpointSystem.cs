using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CheckpointSystem : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("The position the player will return to if they fall.")]
    public Vector3 currentRespawnPos;

    private Rigidbody rb;
    private MovementTech movementScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<MovementTech>();

        // Default respawn point is exactly where the player starts the level
        currentRespawnPos = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. UPDATE CHECKPOINT
        if (other.CompareTag("Checkpoint"))
        {
            // Save this new safe location
            currentRespawnPos = other.transform.position;

            // Turn off the checkpoint trigger so it doesn't fire again
            other.enabled = false;
        }

        // 2. HIT THE VOID / UNREACHABLE AREA
        else if (other.CompareTag("Killzone"))
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        // 1. Teleport the player back to the last saved checkpoint
        transform.position = currentRespawnPos;

        // 2. CRITICAL: Instantly kill all physics momentum! 
        // (Otherwise, you teleport but keep falling at 50mph)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. Give the player their Dash back so they are ready to try the jump again
        if (movementScript != null)
        {
            movementScript.ResetDash();
        }
    }
}