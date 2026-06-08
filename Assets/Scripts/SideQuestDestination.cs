using UnityEngine;

public class SideQuestDestination : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SideQuestManager.Instance != null)
            {
                SideQuestManager.Instance.CompleteQuest();
            }

            gameObject.SetActive(false);
        }
    }
}