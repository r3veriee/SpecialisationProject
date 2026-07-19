using UnityEngine;

public class TowerArrivalTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (QuestManager.Instance != null) QuestManager.Instance.RevealTrials();
        }
    }
}