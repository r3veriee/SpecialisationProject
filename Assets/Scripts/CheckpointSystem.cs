//using UnityEngine;
//using UnityEngine.UI; // Needed for the Image component
//using System.Collections; // Needed for Coroutines

//[RequireComponent(typeof(Rigidbody))]
//public class CheckpointSystem : MonoBehaviour
//{
//    [Header("Respawn Settings")]
//    public Vector3 currentRespawnPos;

//    [Header("Fade Settings")]
//    public Image fadeScreen; // Drag your black UI Image here
//    public float fadeSpeed = 3f;

//    private Rigidbody rb;
//    private MovementTech movementScript;
//    private bool isRespawning = false; // Prevents triggering it twice

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        movementScript = GetComponent<MovementTech>();
//        currentRespawnPos = transform.position;
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Checkpoint"))
//        {
//            currentRespawnPos = other.transform.position;
//            other.enabled = false;
//        }
//        else if (other.CompareTag("Killzone") && !isRespawning)
//        {
//            StartCoroutine(RespawnRoutine());
//        }
//    }

//    IEnumerator RespawnRoutine()
//    {
//        isRespawning = true;

//        // 1. Fade out to black
//        float alpha = 0f;
//        while (alpha < 1f)
//        {
//            alpha += Time.deltaTime * fadeSpeed;
//            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
//            yield return null; // Wait for the next frame
//        }

//        // 2. Teleport and kill momentum while the screen is pitch black
//        transform.position = currentRespawnPos;
//        if (rb != null)
//        {
//            rb.linearVelocity = Vector3.zero;
//            rb.angularVelocity = Vector3.zero;
//        }
//        if (movementScript != null) movementScript.ResetDash();

//        // Optional: Wait half a second in the dark for dramatic effect
//        yield return new WaitForSeconds(0.2f);

//        // 3. Fade back in to the game
//        while (alpha > 0f)
//        {
//            alpha -= Time.deltaTime * fadeSpeed;
//            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
//            yield return null;
//        }

//        isRespawning = false;
//    }
//}

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
            StartCoroutine(RespawnRoutine());
        }
    }

    public void Respawn()
    {
        if (!isRespawning) StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        transform.position = currentRespawnPos;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (movementScript != null) movementScript.ResetDash();

        yield return new WaitForSeconds(0.2f);

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            if (fadeScreen != null) fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        isRespawning = false;
    }
}