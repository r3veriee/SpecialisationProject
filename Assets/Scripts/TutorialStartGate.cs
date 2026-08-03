using UnityEngine;

public class TutorialStartGate : MonoBehaviour
{
    public TutorialTimeTrial tutorialManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialManager != null)
            {
                tutorialManager.StartTutorialTimer();
            }
            gameObject.SetActive(false);
        }
    }
}