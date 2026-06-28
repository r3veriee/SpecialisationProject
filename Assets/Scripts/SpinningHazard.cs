using UnityEngine;

public class SpinningHazard : MonoBehaviour
{
    public float spinSpeed = 120f; // Degrees per second
    public Vector3 spinAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(spinAxis * spinSpeed * Time.deltaTime);
    }
}