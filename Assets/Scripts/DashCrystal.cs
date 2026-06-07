using UnityEngine;
using System.Collections;

public class DashCrystal : MonoBehaviour
{
    [Header("Settings")]
    public float respawnTime = 2.5f; // How long it takes to come back

    [Header("References")]
    public MeshRenderer crystalMesh; // Drag the crystal's MeshRenderer here
    public Collider crystalCollider; // Drag the crystal's Collider here

    [Header("Visuals")]
    public float spinSpeed = 150f;
    public float hoverAmplitude = 0.3f;
    public float hoverSpeed = 3f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Hover and spin animation
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the player's movement script and give them their dash back!
            MovementTech playerMovement = other.GetComponent<MovementTech>();
            if (playerMovement != null)
            {
                playerMovement.ResetDash();
                StartCoroutine(CrystalRespawnRoutine());
            }
        }
    }

    IEnumerator CrystalRespawnRoutine()
    {
        // 1. Shatter/Hide the crystal
        crystalMesh.enabled = false;
        crystalCollider.enabled = false;

        // (Optional) Instantiate a particle explosion here!

        // 2. Wait for the cooldown
        yield return new WaitForSeconds(respawnTime);

        // 3. Respawn the crystal
        crystalMesh.enabled = true;
        crystalCollider.enabled = true;
    }
}