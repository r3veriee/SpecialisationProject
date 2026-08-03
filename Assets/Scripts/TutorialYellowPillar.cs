using UnityEngine;

public class TutorialYellowPillar : MonoBehaviour
{
    public float timeToAdd = 5f;
    public TutorialTimeTrial tutorialManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialManager != null)
            {
                tutorialManager.AddTime(timeToAdd);
            }

            gameObject.SetActive(false);
        }
    }
}