//using UnityEngine;

//public class TimerTrigger : MonoBehaviour
//{
//    public enum TriggerType { StartGate, EndGate }
//    public TriggerType gateType;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            if (gateType == TriggerType.StartGate)
//            {
//                TimeTrialManager.Instance.StartTimer();
//            }
//            else if (gateType == TriggerType.EndGate)
//            {
//                TimeTrialManager.Instance.StopTimer();
//            }

//            // Optional: Turn off this gate so it doesn't trigger twice in one run
//            GetComponent<Collider>().enabled = false;
//        }
//    }
//}

using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    public enum TriggerType { StartGate, EndGate }
    public TriggerType gateType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gateType == TriggerType.StartGate) TimeTrialManager.Instance.StartTimer();
            else if (gateType == TriggerType.EndGate) TimeTrialManager.Instance.StopTimer();
            GetComponent<Collider>().enabled = false;
        }
    }
}