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

    [Header("UI Taunts & HUD")]
    public TextMeshProUGUI tauntText;

    [Tooltip("Drag any HUD elements you want to hide during death here (like the Dash Reticle)")]
    public GameObject[] hudElementsToHide;

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

        // turn off other hud elements
        foreach (GameObject hudItem in hudElementsToHide)
        {
            if (hudItem != null) hudItem.SetActive(false);
        }

        firstPersonCam.enabled = false;
        thirdPersonCam.enabled = true;

        if (playerMesh != null) playerMesh.SetActive(false);
        if (disintegrateVFX != null) disintegrateVFX.Play();

        if (tauntText != null)
        {
            tauntText.text = taunts[Random.Range(0, taunts.Length)];
            tauntText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2.0f);

        checkpointSystem.Respawn();

        float timeToFadeToBlack = 1f / checkpointSystem.fadeSpeed;

        yield return new WaitForSeconds(timeToFadeToBlack + 0.1f);

        if (tauntText != null) tauntText.gameObject.SetActive(false);
        if (playerMesh != null) playerMesh.SetActive(true);

        foreach (GameObject hudItem in hudElementsToHide)
        {
            if (hudItem != null) hudItem.SetActive(true);
        }

        thirdPersonCam.enabled = false;
        firstPersonCam.enabled = true;

        rb.useGravity = true;
        movementScript.enabled = true;

        isDead = false;
    }
}