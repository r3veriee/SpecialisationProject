using UnityEngine;

public class TutorialEndGate : MonoBehaviour
{
    public TutorialTimeTrial tutorialManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialManager != null)
            {
                tutorialManager.StopTutorialTimer();
            }

            gameObject.SetActive(false);
        }
    }
}