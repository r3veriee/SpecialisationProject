using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementTech : MonoBehaviour
{
    [Header("Core Movement Stats")]
    public float walkSpeed = 16f;
    public float acceleration = 4f;
    public float groundFriction = 8f;

    [Header("Tech Stats (The Trinity)")]
    public float dashForce = 45f;
    public float jumpForce = 14f;
    public float hyperMultiplier = 3.0f;
    public float hyperVerticalMultiplier = 0.4f;
    public float superMultiplier = 1.5f;
    public float superVerticalMultiplier = 1.5f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1.0f;
    public float wavedashAngleThreshold = -0.15f;

    [Header("Mirror's Edge Slide")]
    public float slideBoost = 10f;       // Instant forward push when sliding
    public float slideFriction = 1f;     // Ice physics while sliding
    public float normalHeight = 2f;
    public float slideHeight = 1f;
    public CapsuleCollider playerCollider;

    [Header("Input & Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.1f;
    public float jumpBufferTime = 0.2f;
    public float groundCoyoteTime = 0.15f;
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;

    [Header("Camera Tilt & Slide Effects")]
    public float wallRunTilt = 15f;
    public float tiltSpeed = 6f;
    public float standingCamHeight = 0.8f;
    public float slidingCamHeight = -0.2f;
    public float crouchTransitionSpeed = 10f;
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

    [Tooltip("Scales down upward dash power so you don't fly out of the map. 1 = Full Jetpack.")]
    public float upwardDashMultiplier = 0.4f;

    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private bool isCrouching;
    private bool wasCrouching;
    private bool isGrounded;
    private bool isDashing;
    private bool techActive;
    private bool canDash = true;
    private bool isJumpHeld;
    private bool isDashTechDownward; // NEW: Remembers if we meant to hyper!

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

        if ((isGrounded || isWallrunning) && dashCooldownTimer <= 0) canDash = true;
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        // --- UPDATED: Lock in the Hyper if we press crouch AT ALL during the dash ---
        if (isDashing && isCrouching)
        {
            isDashTechDownward = true;
        }

        bool validJumpInput = jumpBufferCounter > 0 || (isJumpHeld && isDashing);

        if (validJumpInput && groundCoyoteCounter > 0 && jumpCooldownTimer <= 0)
        {
            bool intentToHyper = isDashTechDownward || isCrouching;

            if (isGrounded || (isDashing && !intentToHyper)) ExecuteJumpLogic();
        }

        ApplyMovementPhysics();

        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        else isDashing = false;

        wasCrouching = isCrouching;
    }

    void HandleLook()
    {
        lookInput = controls.Player.Look.ReadValue<Vector2>();
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float targetTilt = 0f;
        if (isWallrunning)
        {
            if (wallLeft) targetTilt = -wallRunTilt;
            else if (wallRight) targetTilt = wallRunTilt;
        }
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));

        float targetCamHeight = isCrouching ? slidingCamHeight : standingCamHeight;
        Vector3 camLocalPos = playerCamera.localPosition;
        camLocalPos.y = Mathf.Lerp(camLocalPos.y, targetCamHeight, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.localPosition = camLocalPos;
    }

    void StartDash()
    {
        if (isDashing || !canDash || dashCooldownTimer > 0) return;

        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        isDashTechDownward = false; // NEW: Reset the flag for the new dash!

        Vector3 dashVelocity = playerCamera.forward * dashForce;

        if (playerCamera.forward.y > 0)
        {
            dashVelocity.y *= upwardDashMultiplier;
        }

        rb.linearVelocity = Vector3.zero;
        rb.linearVelocity = dashVelocity;
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

            // --- BINARY TECH: Crouched = Hyper, Standing = Super ---
            if (isDashTechDownward || isCrouching)
            {
                // HYPER / WAVEDASH
                transform.position += Vector3.up * 0.15f;
                rb.linearVelocity = (forwardDir * (dashForce * hyperMultiplier * 0.5f)) + (Vector3.up * jumpForce * hyperVerticalMultiplier);
            }
            else
            {
                // SUPER JUMP
                rb.linearVelocity = (forwardDir * (dashForce * superMultiplier)) + (Vector3.up * jumpForce * superVerticalMultiplier);
            }
            dashTimer = 0;
            isDashing = false;
        }
        else
        {
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

        if (isCrouching)
        {
            playerCollider.height = slideHeight;
            playerCollider.center = new Vector3(0, (slideHeight - normalHeight) / 2f, 0);
        }
        else
        {
            playerCollider.height = normalHeight;
            playerCollider.center = Vector3.zero;
        }

        if (!isDashing)
        {
            float appliedFriction;

            if (techActive)
            {
                appliedFriction = groundFriction * 0.02f;
                if (isGrounded && currentHorizontal.magnitude <= walkSpeed + 1f && jumpCooldownTimer <= 0) techActive = false;
            }
            else if (isWallrunning)
            {
                float yVel = rb.linearVelocity.y;
                if (yVel < -wallRunGravity) yVel = -wallRunGravity;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, yVel, rb.linearVelocity.z);

                appliedFriction = 0f;

                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                rb.AddForce(-wallNormal * 2f, ForceMode.Acceleration);
            }
            else
            {
                if (isCrouching && isGrounded && currentHorizontal.magnitude > walkSpeed * 0.5f)
                {
                    appliedFriction = slideFriction;

                    if (!wasCrouching)
                    {
                        rb.AddForce(playerCamera.forward * slideBoost, ForceMode.Impulse);
                    }
                }
                else
                {
                    appliedFriction = isGrounded ? groundFriction : groundFriction * 0.3f;
                }
            }

            if (!isWallrunning)
            {
                Vector3 velocityChange = (targetVelocity - currentHorizontal);

                bool isSliding = isCrouching && isGrounded && currentHorizontal.magnitude > walkSpeed * 0.5f;
                float currentAccelRate;

                if (techActive || isSliding)
                {
                    currentAccelRate = appliedFriction;
                }
                else if (moveInput.magnitude > 0.1f)
                {
                    currentAccelRate = acceleration;
                }
                else
                {
                    currentAccelRate = appliedFriction;
                }

                rb.AddForce(velocityChange * currentAccelRate, ForceMode.Acceleration);
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
        if (groundCheckPoint != null) isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded && isDashing && rb.linearVelocity.y < 0) rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (isGrounded) groundCoyoteCounter = groundCoyoteTime;
        else groundCoyoteCounter -= Time.deltaTime;
    }

    void CheckWallRun()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, wallLayer);

        if (jumpCooldownTimer > 0) { isWallrunning = false; return; }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        bool isNextToWall = wallLeft || wallRight;

        if (isWallrunning)
        {
            if (!isNextToWall || isGrounded || flatVel.magnitude < (minWallRunSpeed * 0.5f))
            {
                isWallrunning = false;
            }
            else if (jumpBufferCounter > 0)
            {
                ExecuteWallJump();
                isWallrunning = false;
                jumpBufferCounter = 0;
            }
            else
            {
                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;
                rb.linearVelocity = new Vector3(wallForward.x * flatVel.magnitude, rb.linearVelocity.y, wallForward.z * flatVel.magnitude);
            }
        }
        else
        {
            if (isNextToWall && !isGrounded && flatVel.magnitude >= minWallRunSpeed)
            {
                isWallrunning = true;
                canDash = true;

                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                Vector3 wallForward = Vector3.ProjectOnPlane(flatVel, wallNormal).normalized;

                rb.linearVelocity = new Vector3(wallForward.x * flatVel.magnitude, 0f, wallForward.z * flatVel.magnitude);
            }
        }
    }

    public void ResetDash()
    {
        canDash = true;
        dashCooldownTimer = 0f;
    }
}