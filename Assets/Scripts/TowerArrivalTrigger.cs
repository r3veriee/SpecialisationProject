using UnityEngine;

public class TowerArrivalTrigger : MonoBehaviour
{
    private bool hasArrived = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasArrived)
        {
            hasArrived = true;

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.RevealTrials();
            }
        }
    }
}