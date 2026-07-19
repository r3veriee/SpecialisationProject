using UnityEngine;
using System.Collections;

public class TowerArrivalTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ArrivalSequence(other.gameObject));
        }
    }

    IEnumerator ArrivalSequence(GameObject player)
    {
        CheckpointSystem checkpoint = player.GetComponent<CheckpointSystem>();

        // Fade out to black
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

        // Secretly swap the level geometry while the screen is black
        if (QuestManager.Instance != null) QuestManager.Instance.RevealTrials();

        yield return new WaitForSeconds(0.5f);

        // Fade back in to reveal the Trident hub
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
    }
}