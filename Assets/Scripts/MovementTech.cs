using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementTech : MonoBehaviour
{
    [Header("Core Movement Stats")]
    public float walkSpeed = 16f;
    public float groundFriction = 6f;

    [Header("Tech Stats (The Trinity)")]
    public float dashForce = 45f;
    public float jumpForce = 14f;
    public float hyperMultiplier = 3.0f;
    public float superMultiplier = 1.5f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1.0f;
    [Tooltip("Camera downward angle required for Wavedash. -0.15 is slightly down.")]
    public float wavedashAngleThreshold = -0.15f;

    [Header("Input & Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;
    public float jumpBufferTime = 0.2f;
    public float groundCoyoteTime = 0.15f;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;

    [Header("Camera Tilt Effects")]
    public float wallRunTilt = 15f;
    public float tiltSpeed = 6f;
    private float currentTilt = 0f;

    [Header("Wallrun (Mirror's Edge Flow)")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.7f;
    public float minWallRunSpeed = 6f;
    public float wallRunGravity = 4f;
    public float wallJumpForce = 14f;
    public float wallJumpForwardBoost = 8f;
    private bool isWallrunning;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    // State Tracking
    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private bool isCrouching;
    private bool isGrounded;
    private bool isDashing;
    private bool techActive;
    private bool canDash = true;
    private bool isJumpHeld;

    private float dashTimer;
    private float dashCooldownTimer;
    private float jumpBufferCounter;
    private float groundCoyoteCounter;
    private float jumpCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controls = new PlayerControls();

        controls.Player.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        controls.Player.Dash.performed += ctx => StartDash();
        controls.Player.Crouch.performed += ctx => isCrouching = true;
        controls.Player.Crouch.canceled += ctx => isCrouching = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        isJumpHeld = controls.Player.Jump.ReadValue<float>() > 0.5f;

        CheckGrounded();
        CheckWallRun();
        HandleLook();

        if ((isGrounded || isWallrunning) && dashCooldownTimer <= 0)
        {
            canDash = true;
        }

        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        // --- THE ANTI-BUNNYHOP FIX ---
        // Holding space is ONLY allowed to trigger a jump if you are actively dashing (Wavedashing)
        bool validJumpInput = jumpBufferCounter > 0 || (isJumpHeld && isDashing);

        // GROUND JUMP TRIGGER
        if (validJumpInput && groundCoyoteCounter > 0 && jumpCooldownTimer <= 0)
        {
            bool intentToTechDown = isCrouching || playerCamera.forward.y < wavedashAngleThreshold;

            if (isGrounded || (isDashing && !intentToTechDown))
            {
                ExecuteJumpLogic();
            }
        }

        ApplyMovementPhysics();

        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        else isDashing = false;
    }

    void HandleLook()
    {
        lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float targetTilt = 0f;
        if (isWallrunning)
        {
            if (wallLeft) targetTilt = -wallRunTilt;
            else if (wallRight) targetTilt = wallRunTilt;
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        transform.Rotate(Vector3.up * mouseX);
    }

    void StartDash()
    {
        if (isDashing || !canDash || dashCooldownTimer > 0) return;

        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.linearVelocity = playerCamera.forward * dashForce;
    }

    void ExecuteJumpLogic()
    {
        jumpBufferCounter = 0;
        groundCoyoteCounter = 0;
        jumpCooldownTimer = 0.2f;

        if (isDashing)
        {
            techActive = true;

            Vector3 forwardDir = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;

            // HYPER OR WAVEDASH
            if (isCrouching || playerCamera.forward.y < wavedashAngleThreshold)
            {
                // --- THE ANTI-DRAG FIX ---
                // Physically yank the player out of the floor slightly to prevent collision braking
                transform.position += Vector3.up * 0.15f;

                // Slightly increased vertical pop (0.4f instead of 0.3f) so you clear the ground easily
                rb.linearVelocity = (forwardDir * (dashForce * hyperMultiplier * 0.5f)) + (Vector3.up * jumpForce * 0.8f);
            }
            // SUPER JUMP
            else
            {
                rb.linearVelocity = (forwardDir * (dashForce * superMultiplier)) + (Vector3.up * jumpForce * superMultiplier);
            }

            dashTimer = 0;
            isDashing = false;
        }
        else
        {
            // STANDARD JUMP
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void ExecuteWallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * jumpForce + wallNormal * wallJumpForce + playerCamera.forward * wallJumpForwardBoost;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        jumpCooldownTimer = 0.2f;
    }

    void ApplyMovementPhysics()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 targetVelocity = moveDirection * walkSpeed;
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (!isDashing)
        {
            float appliedFriction;

            if (techActive)
            {
                appliedFriction = groundFriction * 0.02f;

                if (isGrounded && currentHorizontal.magnitude <= walkSpeed + 1f && jumpCooldownTimer <= 0)
                {
                    techActive = false;
                }
            }
            else if (isWallrunning)
            {
                float yVel = rb.linearVelocity.y;
                if (yVel < -wallRunGravity) yVel = -wallRunGravity;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);
                appliedFriction = 0f;

                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                rb.AddForce(-wallNormal * 15f, ForceMode.Force);
            }
            else
            {
                appliedFriction = isGrounded ? groundFriction : groundFriction * 0.3f;
            }

            if (!isWallrunning)
            {
                Vector3 velocityChange = (targetVelocity - currentHorizontal);
                rb.AddForce(velocityChange * appliedFriction, ForceMode.Acceleration);
            }
        }
    }

    void CheckGrounded()
    {
        if (jumpCooldownTimer > 0)
        {
            isGrounded = false;
            groundCoyoteCounter = 0;
            return;
        }

        bool wasGrounded = isGrounded;

        if (groundCheckPoint != null)
        {
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }

        // Instantly kill downward momentum upon hitting the floor while dashing
        if (isGrounded && !wasGrounded && isDashing && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        if (isGrounded)
        {
            groundCoyoteCounter = groundCoyoteTime;
        }
        else
        {
            groundCoyoteCounter -= Time.deltaTime;
        }
    }

    void CheckWallRun()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, wallLayer);

        if (jumpCooldownTimer > 0)
        {
            isWallrunning = false;
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if ((wallLeft || wallRight) && !isGrounded && flatVel.magnitude >= minWallRunSpeed)
        {
            if (isJumpHeld)
            {
                if (!isWallrunning)
                {
                    isWallrunning = true;
                    canDash = true;

                    Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                    Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;

                    rb.linearVelocity = new Vector3(wallForward.x * flatVel.magnitude, rb.linearVelocity.y, wallForward.z * flatVel.magnitude);
                }
            }
            else if (isWallrunning)
            {
                ExecuteWallJump();
                isWallrunning = false;
            }
        }
        else
        {
            isWallrunning = false;
        }
    }
    public void ResetDash()
    {
        canDash = true;
        dashCooldownTimer = 0f;
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}