using UnityEngine;

public class PrismShard : MonoBehaviour
{
    [Header("Visuals")]
    public float spinSpeed = 100f;
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed = 2f;

    [Header("Decay Buff")]
    public float decayReductionAmount = 0.15f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (QuestManager.Instance != null) QuestManager.Instance.CollectShard();

            ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
            if (decayScript != null)
            {
                decayScript.ReduceDecayPotency(decayReductionAmount);
                decayScript.TriggerColourBurst(1.5f);
            }

            Destroy(gameObject);
        }
    }
}