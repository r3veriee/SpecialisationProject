using UnityEngine;
using TMPro;
using System.Collections;

public class DeathSequenceManager : MonoBehaviour
{
    [Header("Cameras & Visuals")]
    public Camera firstPersonCam;
    public Camera thirdPersonCam;
    public GameObject playerMesh;
    public ParticleSystem disintegrateVFX;

    [Header("UI Taunts")]
    public TextMeshProUGUI tauntText;
    public string[] taunts = {
        "SUB-OPTIMAL TRAJECTORY.",
        "MOMENTUM.EXE STOPPED WORKING.",
        "PATHETIC.",
        "GRAVITY: 1. YOU: 0."
    };

    [Header("References")]
    public MovementTech movementScript;
    public CheckpointSystem checkpointSystem;

    private bool isDead = false;

    public void TriggerDeath()
    {
        if (!isDead && checkpointSystem != null)
            StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;

        // Freeze the player entirely
        movementScript.enabled = false;
        Rigidbody rb = movementScript.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;

        // Camera swap (Look at the body)
        firstPersonCam.enabled = false;
        thirdPersonCam.enabled = true;

        // Disintegrate
        if (playerMesh != null) playerMesh.SetActive(false);
        if (disintegrateVFX != null) disintegrateVFX.Play();

        // Print the Taunt
        if (tauntText != null)
        {
            tauntText.text = taunts[Random.Range(0, taunts.Length)];
            tauntText.gameObject.SetActive(true);
        }

        // Wait for the drama to sink in
        yield return new WaitForSeconds(2.0f);

        // Tell CheckpointSystem to start fading and teleporting
        checkpointSystem.Respawn();

        // Calculate EXACTLY how long it takes for the CheckpointSystem to fade to black
        // (1 divided by the fadeSpeed gives us the time in seconds)
        float timeToFadeToBlack = 1f / checkpointSystem.fadeSpeed;

        // Wait until the screen is completely black
        yield return new WaitForSeconds(timeToFadeToBlack + 0.1f);

        // WHILE THE SCREEN IS BLACK: Reset the cameras and player model invisibly
        if (tauntText != null) tauntText.gameObject.SetActive(false);
        if (playerMesh != null) playerMesh.SetActive(true);

        thirdPersonCam.enabled = false;
        firstPersonCam.enabled = true;

        rb.useGravity = true;
        movementScript.enabled = true;

        isDead = false;
    }
}