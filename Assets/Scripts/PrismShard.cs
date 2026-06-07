//using UnityEngine;

//public class PrismShard : MonoBehaviour
//{
//    [Header("Visuals")]
//    public float spinSpeed = 100f;
//    public float hoverAmplitude = 0.5f;
//    public float hoverSpeed = 2f;

//    private Vector3 startPos;

//    [Header("Rewards")]
//    public float speedBoostAmount = 10f; // Gives a tiny dash of momentum

//    void Start()
//    {
//        startPos = transform.position;
//    }

//    void Update()
//    {
//        // Make the shard spin and hover to look like a retro Y2K collectible
//        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
//        transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude, 0f);
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            // 1. Give the player a tiny forward speed boost as a reward
//            Rigidbody playerRb = other.GetComponent<Rigidbody>();
//            if (playerRb != null)
//            {
//                playerRb.AddForce(other.transform.forward * speedBoostAmount, ForceMode.Impulse);
//            }

//            // 2. Tell the Quest Manager we got a shard
//            QuestManager.Instance.CollectShard();

//            // 3. Play a sound/particle here (Optional)
//            // AudioSource.PlayClipAtPoint(collectSound, transform.position);
//            // Find the Chromatic Decay script on the player/camera and trigger a 1.5 second colour burst!
//            ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
//            if (decayScript != null)
//            {
//                decayScript.TriggerColourBurst(1.5f);
//            }
//            // 4. Delete the shard
//            Destroy(gameObject);
//        }
//    }
//}

using UnityEngine;

public class PrismShard : MonoBehaviour
{
    public float spinSpeed = 100f;
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed = 2f;
    public float speedBoostAmount = 10f;

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
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null) playerRb.AddForce(other.transform.forward * speedBoostAmount, ForceMode.Impulse);

            if (QuestManager.Instance != null) QuestManager.Instance.CollectShard();

            ChromaticDecay decayScript = FindObjectOfType<ChromaticDecay>();
            if (decayScript != null) decayScript.TriggerColourBurst(1.5f);

            Destroy(gameObject);
        }
    }
}