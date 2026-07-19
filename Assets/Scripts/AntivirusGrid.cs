using UnityEngine;

public class AntivirusGrid : MonoBehaviour
{
    [Header("Antivirus Settings")]
    public float riseSpeed = 8f;

    private bool isMoving = false;
    private Vector3 startPosition;
    private MeshRenderer mesh;
    private Collider col;

    void Start()
    {
        // Remember exactly where it started so it can reset
        startPosition = transform.position;
        mesh = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime, Space.World);
        }
    }

    public void StartChasing() => isMoving = true;

    public void ResetGrid()
    {
        // Snaps the grid back to the bottom and waits
        isMoving = false;
        transform.position = startPosition;
    }

    public void DisableGrid()
    {
        // Permanently shuts it off so the player can drop down the hole safely
        isMoving = false;
        if (mesh != null) mesh.enabled = false;
        if (col != null) col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Trigger the Cinematic Death
            DeathSequenceManager deathManager = FindObjectOfType<DeathSequenceManager>();
            if (deathManager != null)
            {
                deathManager.TriggerDeath();
            }
            else
            {
                // Fallback just in case the manager is missing from the scene
                CheckpointSystem cp = other.GetComponent<CheckpointSystem>();
                if (cp != null) cp.Respawn();
            }

            // Reset the grid instantly while the screen fades to black
            // This ensures it doesn't instantly spawn-kill the player when they wake up at the bottom
            ResetGrid();
        }
    }
}