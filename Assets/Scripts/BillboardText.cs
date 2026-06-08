using UnityEngine;

public class BillboardText : MonoBehaviour
{
    [Header("Target Camera")]
    [SerializeField] private Camera mainCam;

    void LateUpdate()
    {
        if (mainCam == null) return;
        transform.forward = mainCam.transform.forward;
    }
}