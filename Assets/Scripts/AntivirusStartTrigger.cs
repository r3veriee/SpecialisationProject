using UnityEngine;

public class AntivirusStartTrigger : MonoBehaviour
{
    public AntivirusGrid grid;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && grid != null)
        {
            grid.ResetGrid();
            grid.StartChasing();
        }
    }
}