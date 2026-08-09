using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Tuned Launch Settings")]
    [Tooltip("How fast it shoots the player horizontally")]
    public float horizontalForce = 40f;

    [Tooltip("How high it shoots the player vertically")]
    public float verticalForce = 15f;

    [Tooltip("If true, ignores how fast the player was falling and forces a perfect arc.")]
    public bool overrideVelocity = true;

    [Header("Feel")]
    public ParticleSystem bounceParticles;
    public AudioSource bounceSound;

    [Header("Safety")]
    public float padCooldown = 0.2f;
    private float cooldownTimer = 0f;

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (cooldownTimer > 0) return;

        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 padDirection = transform.up;
                Vector3 flatDirection = new Vector3(padDirection.x, 0, padDirection.z).normalized;

                Vector3 finalLaunchVelocity;

                if (flatDirection.magnitude < 0.1f)
                {
                    finalLaunchVelocity = Vector3.up * verticalForce;
                }
                else
                {

                    finalLaunchVelocity = (flatDirection * horizontalForce) + (Vector3.up * verticalForce);
                }

                if (overrideVelocity)
                {
                    playerRb.linearVelocity = Vector3.zero;
                }

                playerRb.AddForce(finalLaunchVelocity, ForceMode.VelocityChange);

                MovementTech movement = other.GetComponent<MovementTech>();
                if (movement != null)
                {
                    movement.dashCooldownTimer = 0f;
                    movement.canDash = true;
                }

                if (bounceParticles != null) bounceParticles.Play();
                AudioManager.Instance.PlaySFX(SFXType.BouncePad);

                cooldownTimer = padCooldown;
            }
        }
    }
}