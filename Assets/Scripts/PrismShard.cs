using UnityEngine;

public class PrismShard : MonoBehaviour
{
    [Header("Visuals")]
    public float spinSpeed = 100f;
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed = 2f;

    private Vector3 startPos;

    [Header("Rewards")]
    public float speedBoostAmount = 10f; // Gives a tiny dash of momentum

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Make the shard spin and hover to look like a retro Y2K collectible
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Give the player a tiny forward speed boost as a reward
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(other.transform.forward * speedBoostAmount, ForceMode.Impulse);
            }

            // 2. Tell the Quest Manager we got a shard
            QuestManager.Instance.CollectShard();

            // 3. Play a sound/particle here (Optional)
            // AudioSource.PlayClipAtPoint(collectSound, transform.position);

            // 4. Delete the shard
            Destroy(gameObject);
        }
    }
}