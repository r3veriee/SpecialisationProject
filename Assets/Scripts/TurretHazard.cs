using UnityEngine;
using UnityEngine.UI;

public class TurretHazard : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private LineRenderer laserSight;

    [Header("Fixed UI Settings")]
    [Tooltip("Drag your UI Warning Ring PREFAB here!")]
    public GameObject warningUIPrefab;
    [Tooltip("The exact name of the Layout Group on your Canvas")]
    public string containerName = "TurretWarningContainer";

    private Image myUI;

    [Header("Stats")]
    public float range = 50f;
    public float chargeTime = 3f;
    public float fireCooldown = 4.5f;
    public LayerMask visionBlockers;

    private float currentCharge = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        laserSight = GetComponent<LineRenderer>();
        if (laserSight != null) laserSight.enabled = false;

        // Find the horizontal layout group container on the Canvas
        GameObject container = GameObject.Find(containerName);
        if (container != null && warningUIPrefab != null)
        {
            // Spawn a personal UI ring for this turret and put it in the container
            GameObject spawnedUI = Instantiate(warningUIPrefab, container.transform);
            myUI = spawnedUI.GetComponent<Image>();

            if (myUI != null) myUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            if (myUI != null)
            {
                myUI.gameObject.SetActive(true);
                myUI.fillAmount = cooldownTimer / fireCooldown;
                myUI.color = Color.cyan;
            }

            if (laserSight != null) laserSight.enabled = false;
            return;
        }

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = distToPlayer <= range && !Physics.Raycast(transform.position, dirToPlayer, distToPlayer, visionBlockers);

        if (hasLineOfSight)
        {
            currentCharge += Time.deltaTime;

            if (laserSight != null)
            {
                laserSight.enabled = true;
                laserSight.SetPosition(0, transform.position);
                laserSight.SetPosition(1, player.position);
            }

            if (myUI != null)
            {
                myUI.gameObject.SetActive(true);
                myUI.fillAmount = currentCharge / chargeTime;
                myUI.color = Color.Lerp(Color.yellow, Color.red, currentCharge / chargeTime);
            }

            if (currentCharge >= chargeTime) Fire();
        }
        else
        {
            currentCharge = 0f;
            if (laserSight != null) laserSight.enabled = false;

            if (cooldownTimer <= 0 && myUI != null)
            {
                myUI.gameObject.SetActive(false);
            }
        }
    }

    void Fire()
    {
        DeathSequenceManager deathManager = FindObjectOfType<DeathSequenceManager>();
        if (deathManager != null) deathManager.TriggerDeath();
        else
        {
            CheckpointSystem cp = player.GetComponent<CheckpointSystem>();
            if (cp != null) cp.Respawn();
        }

        currentCharge = 0f;
        cooldownTimer = fireCooldown;
    }
}