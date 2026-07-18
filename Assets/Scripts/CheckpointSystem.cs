using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CheckpointSystem : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Vector3 currentRespawnPos;

    [Header("Fade Settings")]
    public Image fadeScreen;
    public float fadeSpeed = 3f;

    private Rigidbody rb;
    private MovementTech movementScript;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<MovementTech>();
        currentRespawnPos = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            currentRespawnPos = other.transform.position;
            other.enabled = false;
        }
        else if (other.CompareTag("Killzone") && !isRespawning)
        {
            DeathSequenceManager deathManager = FindObjectOfType<DeathSequenceManager>();
            if (deathManager != null)
            {
                deathManager.TriggerDeath();
            }
            else
            {
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    public void Respawn()
    {
        if (!isRespawning) StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Instantly fail any active side quests the moment the player dies
        if (SideQuestManager.Instance != null) SideQuestManager.Instance.FailQuest();

        // Fade out to black
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Safely teleport the Rigidbody and kill all momentum so don't slide off the respawn point
        if (rb != null)
        {
            rb.position = currentRespawnPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = currentRespawnPos;
        }

        if (movementScript != null) movementScript.ResetDash();

        yield return new WaitForSeconds(0.2f);

        // Fade back in
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        isRespawning = false;
    }
}