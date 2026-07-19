using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrismShard : MonoBehaviour
{
    [Header("Visuals")]
    public float spinSpeed = 100f;
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed = 2f;

    [Header("Decay Buff")]
    public float decayReductionAmount = 0.15f;

    private Vector3 startPos;
    private bool isCollected = false;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            StartCoroutine(CollectSequence(other.gameObject));
        }
    }

    IEnumerator CollectSequence(GameObject player)
    {
        // Hide the Shard visually immediately
        GetComponent<MeshRenderer>().enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Stop the SideQuest (Trial 2) timer
        if (SideQuestManager.Instance != null) SideQuestManager.Instance.CompleteQuest();

        // Quest & Buffs
        if (QuestManager.Instance != null) QuestManager.Instance.CollectShard();
        ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
        if (decayScript != null)
        {
            decayScript.ReduceDecayPotency(decayReductionAmount);
            decayScript.TriggerColourBurst(1.5f);
        }

        // Fade to Black
        CheckpointSystem checkpoint = player.GetComponent<CheckpointSystem>();
        if (checkpoint != null && checkpoint.fadeScreen != null)
        {
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * checkpoint.fadeSpeed;
                checkpoint.fadeScreen.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
        else yield return new WaitForSeconds(0.5f); // Fallback delay

        // Teleport
        if (checkpoint != null)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.position = checkpoint.currentRespawnPos;
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Extra safety fallback just in case the Rigidbody is acting up
                player.transform.position = checkpoint.currentRespawnPos;
            }
        }

        MovementTech movement = player.GetComponent<MovementTech>();
        if (movement != null) movement.ResetDash();

        // Fade back in
        if (checkpoint != null && checkpoint.fadeScreen != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * checkpoint.fadeSpeed;
                checkpoint.fadeScreen.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // Destroy the shard
        Destroy(gameObject);
    }
}