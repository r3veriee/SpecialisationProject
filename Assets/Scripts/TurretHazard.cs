using UnityEngine;
using UnityEngine.UI;

public class TurretHazard : MonoBehaviour
{
    public Transform player;
    public Camera mainCamera;
    public float range = 50f;
    public float chargeTime = 3f;

    public float fireCooldown = 4.5f;
    public LayerMask visionBlockers;

    public Image chargeFillImage;
    public float uiHeightOffset = 2.5f;
    public float edgePadding = 60f;

    private float currentCharge = 0f;
    private float cooldownTimer = 0f;
    private LineRenderer laserSight;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (mainCamera == null) mainCamera = Camera.main;

        laserSight = GetComponent<LineRenderer>();
        if (laserSight != null) laserSight.enabled = false;

        if (chargeFillImage != null) chargeFillImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // CHECK VISION
        if (distToPlayer <= range && !Physics.Raycast(transform.position, dirToPlayer, distToPlayer, visionBlockers))
        {
            transform.rotation = Quaternion.LookRotation(dirToPlayer);
            currentCharge += Time.deltaTime;

            if (laserSight != null)
            {
                laserSight.enabled = true;
                laserSight.SetPosition(0, transform.position);
                laserSight.SetPosition(1, player.position);
                laserSight.startColor = Color.Lerp(Color.yellow, Color.red, currentCharge / chargeTime);
                laserSight.endColor = laserSight.startColor;
            }

            // MAIN CANVAS UI TRACKING
            if (chargeFillImage != null && mainCamera != null)
            {
                chargeFillImage.gameObject.SetActive(true);

                Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + (Vector3.up * uiHeightOffset));
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

                if (screenPos.z < 0)
                {
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                bool isOffScreen = screenPos.z < 0 ||
                                   screenPos.x < edgePadding || screenPos.x > Screen.width - edgePadding ||
                                   screenPos.y < edgePadding || screenPos.y > Screen.height - edgePadding;

                if (isOffScreen)
                {
                    Vector3 dir = (screenPos - screenCenter).normalized;
                    screenPos = screenCenter + (dir * 10000f);
                    screenPos.x = Mathf.Clamp(screenPos.x, edgePadding, Screen.width - edgePadding);
                    screenPos.y = Mathf.Clamp(screenPos.y, edgePadding, Screen.height - edgePadding);
                }

                chargeFillImage.transform.position = screenPos;
                chargeFillImage.fillAmount = currentCharge / chargeTime;
                chargeFillImage.color = Color.Lerp(Color.yellow, Color.red, currentCharge / chargeTime);
            }

            if (currentCharge >= chargeTime) Fire();
        }
        else
        {
            // IF LINE OF SIGHT IS BROKEN
            currentCharge = 0f;
            if (laserSight != null) laserSight.enabled = false;
            if (chargeFillImage != null) chargeFillImage.gameObject.SetActive(false);
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

        // START THE COOLDOWN
        currentCharge = 0f;
        cooldownTimer = fireCooldown;

        if (laserSight != null) laserSight.enabled = false;
        if (chargeFillImage != null) chargeFillImage.gameObject.SetActive(false);
    }
}