using UnityEngine;

public class TurretHazard : MonoBehaviour
{
    public Transform player;
    public float range = 50f;
    public float chargeTime = 3f;
    public LayerMask visionBlockers;

    private float currentCharge = 0f;
    private LineRenderer laserSight;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        laserSight = GetComponent<LineRenderer>();
        if (laserSight != null) laserSight.enabled = false;
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
                // Turn the laser on when it spots the player
                laserSight.enabled = true;

                laserSight.SetPosition(0, transform.position);
                laserSight.SetPosition(1, player.position);
                laserSight.startColor = Color.Lerp(Color.yellow, Color.red, currentCharge / chargeTime);
                laserSight.endColor = laserSight.startColor;
            }

            if (currentCharge >= chargeTime) Fire();
        }
        else
        {
            currentCharge = 0f; // Reset charge if line of sight is broken

            // Turn the laser completely off when the player hides
            if (laserSight != null) laserSight.enabled = false;
        }
    }

    void Fire()
    {
        // Try to trigger the cinematic death sequence
        DeathSequenceManager deathManager = FindObjectOfType<DeathSequenceManager>();
        if (deathManager != null)
        {
            deathManager.TriggerDeath();
        }
        else
        {
            // Fallback normal respawn
            CheckpointSystem cp = player.GetComponent<CheckpointSystem>();
            if (cp != null) cp.Respawn();
        }

        // Reset the turret
        currentCharge = 0f;
        if (laserSight != null) laserSight.enabled = false;
    }
}