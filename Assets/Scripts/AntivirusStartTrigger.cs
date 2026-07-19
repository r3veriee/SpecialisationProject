using UnityEngine;

public class AntivirusStartTrigger : MonoBehaviour
{
    public AntivirusGrid grid;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (grid != null)
            {
                // Reset it just in case, then start the chase
                grid.ResetGrid();
                grid.StartChasing();
            }
        }
    }
}