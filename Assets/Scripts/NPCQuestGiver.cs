using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    public float startingTime = 15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SideQuestManager.Instance.StartNPCQuest(startingTime, gameObject);
        }
    }
}