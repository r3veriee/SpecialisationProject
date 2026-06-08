using UnityEngine;

public class TimeExtensionGate : MonoBehaviour
{
    public float bonusTime = 5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SideQuestManager.Instance.AddQuestTime(bonusTime, gameObject);
        }
    }
}