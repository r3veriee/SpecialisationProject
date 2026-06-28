using UnityEngine;

public class TurretHazard : MonoBehaviour
{
    public Transform player;
    public float range = 50f;
    public float chargeTime = 3f; // Faster for your fast-paced game
    public LayerMask visionBlockers;

    private float currentCharge = 0f;
    private LineRenderer laserSight;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        laserSight = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Check range AND make sure no walls are in the way
        if (distToPlayer <= range && !Physics.Raycast(transform.position, dirToPlayer, distToPlayer, visionBlockers))
        {
            transform.rotation = Quaternion.LookRotation(dirToPlayer);
            currentCharge += Time.deltaTime;

            if (laserSight != null)
            {
                laserSight.SetPosition(0, transform.position);
                laserSight.SetPosition(1, player.position);
                laserSight.startColor = Color.Lerp(Color.yellow, Color.red, currentCharge / chargeTime);
            }

            if (currentCharge >= chargeTime) Fire();
        }
        else
        {
            currentCharge = 0f; // Reset if line of sight is broken!
            if (laserSight != null) laserSight.SetPosition(1, transform.position);
        }
    }

    void Fire()
    {
        CheckpointSystem cp = player.GetComponent<CheckpointSystem>();
        if (cp != null) cp.Respawn();
        currentCharge = 0f;
    }
}