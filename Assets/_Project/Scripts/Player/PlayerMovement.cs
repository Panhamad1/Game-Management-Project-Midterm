using System;
using UnityEngine;

/// <summary>
/// Upgraded physical locomotion system with realistic AAA kinetic sliding, sprinting, and bunny-hopping:
/// - Three-tier locomotion: Walk (7.0 m/s), Sprint (12.0 m/s on Shift), and Crouch Walk (3.8 m/s)
/// - Absolute floor anchoring: Capsule bottom is locked at local Y = -1.0; zero ground clipping
/// - Dynamic Power Slide: High-speed momentum slide when running forward (Shift + Crouch or W + Crouch)
/// - Slope Surfing & Hill Launches: Downhill slopes add real gravitational acceleration; launching off
///   hills preserves ballistic launch momentum into the air
/// - Jump-to-Slide Chain: Holding crouch in mid-air immediately catches landing momentum into a slide
/// - Slide-to-Jump Chain (Bunny-Hop): Space out of a slide preserves 100% horizontal momentum in air
/// - Recoil-Convert Boost: Blasting Push during a slide redirects recoil into forward speed (+10 m/s)
/// - Overhead Clearance Sensor: Prevents standing up inside low ducts or tunnels
/// - 35 m/s soft cap
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Locomotion Tuning")]
    [Tooltip("Standard walking speed.")]
    [SerializeField] private float walkSpeed = 6.5f;

    [Tooltip("Sprint running speed (held Shift).")]
    [SerializeField] private float sprintSpeed = 11.5f;

    [Tooltip("Slow crouch walking speed when not sliding.")]
    [SerializeField] private float crouchSpeed = 3.5f;

    [Tooltip("Ground directional acceleration rate.")]
    [SerializeField] private float groundAcceleration = 55.0f;

    [Tooltip("Ground opposing-input braking deceleration.")]
    [SerializeField] private float groundBraking = 45.0f;

    [Tooltip("Aerodynamic momentum decay when traveling above sprint speed on the ground.")]
    [SerializeField] private float groundMomentumDrag = 3.5f;

    [Tooltip("Air steering acceleration.")]
    [SerializeField] private float airAcceleration = 26f;

    [Tooltip("Vertical impulse force on Space. 8.7m/s gives precisely 1.6m vertical clearance under 2.4x gravity.")]
    [SerializeField] private float jumpImpulse = 8.7f;

    [Tooltip("Bunny-hop velocity increment when jumping out of a slide or chaining hops.")]
    [SerializeField] private float bHopBoost = 2.5f;

    [Header("Slide Dynamics Tuning")]
    [Tooltip("Minimum horizontal speed required to initiate a slide (requires sprint).")]
    [SerializeField] private float runThreshold = 7.5f;

    [Tooltip("Initial velocity boost added on top of running speed when entering a slide.")]
    [SerializeField] private float slideBoost = 4.5f;

    [Tooltip("Minimum entry speed for a slide.")]
    [SerializeField] private float minSlideEntrySpeed = 14.0f;

    [Tooltip("Minimum speed before slide smoothly drops back into a crouch walk.")]
    [SerializeField] private float minSlideSpeed = 3.5f;

    [Tooltip("Base ground friction deceleration per second on flat ground.")]
    [SerializeField] private float slideFriction = 2.2f;

    [Tooltip("Aerodynamic drag factor proportional to speed (natural glide curve).")]
    [SerializeField] private float slideDrag = 0.035f;

    [Tooltip("Acceleration factor when sliding downhill.")]
    [SerializeField] private float slopeAcceleration = 28.0f;

    [Tooltip("Steering agility while sliding (degrees per second).")]
    [SerializeField] private float slideSteerRate = 160.0f;

    [Tooltip("Additional forward impulse when Push recoil is converted while sliding.")]
    [SerializeField] private float slideRecoilBoost = 10.0f;

    [Tooltip("Maximum allowed speed (soft cap).")]
    [SerializeField] private float maxSpeed = 35.0f;

    [Tooltip("Minimum guaranteed slide duration upon triggering (seconds).")]
    [SerializeField] private float minSlideDuration = 0.35f;

    [Header("Capsule Dimensions (Feet Anchored at -1.0)")]
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.15f;
    [SerializeField] private float feetLocalY = -1.0f;
    [SerializeField] private float standingEyeY = 0.65f;
    [SerializeField] private float crouchEyeY = 0.05f; // 1.05m above feet, never touches floor!
    [SerializeField] private float crouchTransitionSpeed = 12.0f;
    [SerializeField] private LayerMask overheadClearanceMask;

    [Header("Jump Game Feel")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.35f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Gravity Scaling")]
    [SerializeField] private float standardGravityScale = 2.4f;
    [SerializeField] private float slowMoGravityScale = 0.5f;

    [Header("Component References")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerAudioEffects audioEffects;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private Transform groundedObject;
    private bool isSlowMotionActive;
    private float previousVerticalVelocity;
    private Vector3 groundNormal = Vector3.up;

    // Slide & Crouch State
    private bool isCrouching;
    private bool isSliding;
    private bool wantsCrouch;
    private Vector3 slideDirection = Vector3.forward;
    private float currentSlideSpeed;
    private float airborneSlideBufferTimer = 0f;
    private float slideBufferTimer = 0f;
    private float slideTimer = 0f;
    private bool airborneRunPreserved = false;
    private bool hasAirRocketJumped = false;
    private bool isGrappling = false;
    private Vector3 grappleAnchorPoint = Vector3.zero;
    private bool isJumpHeld = false;

    // Checkpoint & Hazard
    [SerializeField] private Vector3 currentCheckpointPos = new Vector3(0f, 7.6f, -2.5f);
    [SerializeField] private float voidKillY = -8.0f;

    // Timing
    private float lastGroundedTime = -10f;
    private float lastJumpPressedTime = -10f;
    private float footstepTimer = 0f;

    // Component References
    [SerializeField] private PullAbility pullAbility;
    private PlayerMovementTelemetry telemetry;

    // Public API Contracts
    public bool IsGrounded => isGrounded || (Time.time - lastGroundedTime <= coyoteTime);
    public Transform GroundedObject => groundedObject;
    public Rigidbody Rb => rb;
    public Vector2 CurrentMoveInput => moveInput;
    public float CurrentSpeed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
    public bool IsSliding => isSliding;
    public bool IsCrouching => isCrouching;
    public bool IsSprinting => isSprinting;
    public bool IsGrappling => isGrappling;
    public Vector3 SlideDirection => slideDirection;
    public Vector3 CurrentCheckpoint => currentCheckpointPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        PhysicsMaterial frictionlessMat = new PhysicsMaterial("PlayerFrictionless")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounciness = 0f,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        capsule.material = frictionlessMat;

        // Initialize capsule so bottom is anchored at feetLocalY (-1.0f)
        capsule.height = standingHeight;
        capsule.center = new Vector3(0f, feetLocalY + (standingHeight * 0.5f), 0f);

        if (mouseLook == null) mouseLook = GetComponentInChildren<MouseLook>();
        if (audioEffects == null) audioEffects = GetComponent<PlayerAudioEffects>();
        if (pullAbility == null) pullAbility = GetComponentInChildren<PullAbility>();
        if (telemetry == null) telemetry = GetComponent<PlayerMovementTelemetry>();

        if (groundLayerMask.value == 0)
        {
            groundLayerMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
        }
        if (overheadClearanceMask.value == 0)
        {
            overheadClearanceMask = LayerMask.GetMask("Environment");
        }
    }

    private void Start()
    {
        EnsureSafeSpawnPosition();
    }

    /// <summary>
    /// Guarantees the player never spawns or falls into the void below the map geometry.
    /// Dynamically finds the highest walkable platform surface and snaps the player's feet safely on top.
    /// </summary>
    public void EnsureSafeSpawnPosition()
    {
        // Check 1: If Chamber 1 StartHub exists in scene, snap directly to its top surface
        GameObject startHub = GameObject.Find("Chamber1_StartHub");
        if (startHub != null)
        {
            float hubTop = startHub.transform.position.y + (startHub.transform.localScale.y * 0.5f);
            if (transform.position.y < hubTop || (transform.position - startHub.transform.position).sqrMagnitude > 400f)
            {
                Vector3 safeHubPos = new Vector3(startHub.transform.position.x, hubTop + Mathf.Abs(feetLocalY) + 0.1f, startHub.transform.position.z - 2.5f);
                SnapToPosition(safeHubPos);
                return;
            }
        }

        // Check 2: Cast high raycast down to detect if an overhead platform exists that we spawned underneath
        Vector3 skyOrigin = new Vector3(transform.position.x, 35.0f, transform.position.z);
        if (Physics.Raycast(skyOrigin, Vector3.down, out RaycastHit skyHit, 50.0f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (skyHit.point.y > transform.position.y)
            {
                Vector3 safePos = new Vector3(transform.position.x, skyHit.point.y + Mathf.Abs(feetLocalY) + 0.1f, transform.position.z);
                SnapToPosition(safePos);
                return;
            }
        }

        // Check 3: Check downward from slightly above current position to snap cleanly onto floor
        Vector3 groundOrigin = transform.position + Vector3.up * 2.5f;
        if (Physics.Raycast(groundOrigin, Vector3.down, out RaycastHit groundHit, 30.0f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 safePos = new Vector3(transform.position.x, groundHit.point.y + Mathf.Abs(feetLocalY) + 0.05f, transform.position.z);
            SnapToPosition(safePos);
        }
        else
        {
            currentCheckpointPos = transform.position;
        }
    }

    private void SnapToPosition(Vector3 safePos)
    {
        transform.position = safePos;
        currentCheckpointPos = safePos;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        CheckGroundStatus();
        ProcessBufferedJump();
        ProcessLedgeMantle();
        UpdateCrouchAndSlideState();
        ApplyMovementVelocity();
        ApplyCustomGravity();

        if (transform.position.y < voidKillY)
        {
            RespawnAtCheckpoint();
        }

        previousVerticalVelocity = rb.linearVelocity.y;
        wasGroundedLastFrame = isGrounded;
    }

    private void Update()
    {
        UpdateDimensions();

        if (slideBufferTimer > 0f) slideBufferTimer -= Time.deltaTime;

        if (mouseLook != null)
        {
            mouseLook.UpdateCameraMotion(moveInput, isGrounded, CurrentSpeed);
        }

        if (audioEffects != null)
        {
            audioEffects.UpdateSlideAudio(isSliding, CurrentSpeed);
            audioEffects.UpdateWindRush(CurrentSpeed);
        }

        ProcessFootstepAudio();
    }

    private void UpdateDimensions()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        if (Mathf.Abs(capsule.height - targetHeight) > 0.005f)
        {
            capsule.height = Mathf.MoveTowards(capsule.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            // Anchor capsule bottom permanently at feetLocalY (-1.0f) so player NEVER sinks into floor!
            capsule.center = new Vector3(0f, feetLocalY + (capsule.height * 0.5f), 0f);
        }

        if (mouseLook != null)
        {
            float targetEyeY = isCrouching ? crouchEyeY : standingEyeY;
            mouseLook.SetCameraTargetY(targetEyeY);
            mouseLook.SetSliding(isSliding);
        }
    }

    public void SetMoveInput(Vector2 dir)
    {
        moveInput = Vector2.ClampMagnitude(dir, 1f);
    }

    public void SetSprintInput(bool sprint)
    {
        isSprinting = sprint;
    }

    public void SetCrouchInput(bool crouch)
    {
        if (crouch && !wantsCrouch)
        {
            // Fresh crouch press: generous buffer window for landing slide or instant chain
            slideBufferTimer = 0.5f;
        }
        wantsCrouch = crouch;
    }

    public void SetJumpHoldInput(bool held)
    {
        isJumpHeld = held;
    }

    public void Jump()
    {
        lastJumpPressedTime = Time.time;
    }

    public void SetCheckpoint(Vector3 pos)
    {
        currentCheckpointPos = pos;
    }

    public void RespawnAtCheckpoint()
    {
        // Safety: If checkpoint is invalid or below kill plane, recalibrate to safe ground
        if (currentCheckpointPos.y < voidKillY + 1.0f)
        {
            EnsureSafeSpawnPosition();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = currentCheckpointPos;
        isSliding = false;
        isCrouching = false;
        slideBufferTimer = 0f;
        slideTimer = 0f;
        airborneSlideBufferTimer = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazards") || other.CompareTag("Respawn"))
        {
            RespawnAtCheckpoint();
        }
    }

    private void UpdateCrouchAndSlideState()
    {
        bool justLanded = isGrounded && !wasGroundedLastFrame;

        // 1. Redliner / Apex Air-to-Ground Landing Slide Queue
        if (justLanded)
        {
            float horizontalSpeed = CurrentSpeed;
            bool shouldSlideOnLand = wantsCrouch || (slideBufferTimer > 0f);

            // Only trigger landing slide if landing with actual sprint/run velocity!
            if (shouldSlideOnLand && (horizontalSpeed >= (walkSpeed - 0.5f) || airborneRunPreserved))
            {
                // Landed while holding or buffering crouch with momentum: instant explosive landing slide!
                float entrySpeed = Mathf.Max(horizontalSpeed + slideBoost, minSlideEntrySpeed);
                StartSlide(entrySpeed);
                slideBufferTimer = 0f;
                airborneRunPreserved = false;
            }
            else
            {
                airborneRunPreserved = false;
            }
        }

        if (slideTimer > 0f)
        {
            slideTimer -= Time.fixedDeltaTime;
        }

        // 2. Ground Crouch & Slide State Updates
        if (isGrounded)
        {
            if (wantsCrouch)
            {
                isCrouching = true;

                if (!isSliding)
                {
                    float horizontalSpeed = CurrentSpeed;
                    bool isMovingForward = moveInput.y > 0.1f;
                    
                    // Strict Redliner Slide Rule:
                    // MUST be actively sprinting forward with high speed, OR carry preserved high momentum!
                    // Normal walking + crouch simply crouch-walks without sliding.
                    bool hasSlideVelocity = (isSprinting && isMovingForward && horizontalSpeed >= (walkSpeed - 0.5f)) ||
                                            (airborneRunPreserved && horizontalSpeed >= (walkSpeed - 0.5f));

                    if (hasSlideVelocity)
                    {
                        // SPRINT / RUN -> SLIDE CHAIN!
                        float entrySpeed = Mathf.Max(horizontalSpeed + slideBoost, minSlideEntrySpeed);
                        StartSlide(entrySpeed);
                    }
                }
            }
            else
            {
                // Crouch key not pressed
                if (isSliding)
                {
                    // Allow slide to finish its minimum tactile duration before standing
                    if (slideTimer <= 0f)
                    {
                        if (HasOverheadClearance())
                        {
                            isSliding = false;
                            isCrouching = false;
                        }
                    }
                }
                else if (isCrouching)
                {
                    if (HasOverheadClearance())
                    {
                        isCrouching = false;
                    }
                }
            }
        }
        else
        {
            // Airborne: tuck into crouch if holding crouch
            if (wantsCrouch)
            {
                isCrouching = true;
            }
            else if (isCrouching && !isSliding)
            {
                if (HasOverheadClearance())
                {
                    isCrouching = false;
                }
            }
        }

        // 3. Active Slide Physics Simulation
        if (isSliding)
        {
            if (!isGrounded)
            {
                // Airborne buffer: If sliding off a ramp lip or hill into the air,
                // do NOT abruptly cancel the slide state for small bumps or air time!
                airborneSlideBufferTimer += Time.fixedDeltaTime;
                if (airborneSlideBufferTimer > 0.4f)
                {
                    isSliding = false;
                }
                return;
            }
            else
            {
                airborneSlideBufferTimer = 0f;
            }

            // Slope Physics Calculation
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float slopeDot = Vector3.Dot(slideDirection, downhill);

            if (slopeAngle > 4.0f)
            {
                // Surfing downhill: adds massive gravity acceleration!
                // Surfing uphill: naturally decelerates faster
                float angleRad = slopeAngle * Mathf.Deg2Rad;
                float gravityForce = slopeDot * slopeAcceleration * Mathf.Sin(angleRad);
                currentSlideSpeed += gravityForce * Time.fixedDeltaTime;
            }
            else
            {
                // Flat ground realistic quadratic drag (smooth deceleration curve)
                float totalDecel = slideFriction + (currentSlideSpeed * slideDrag);
                currentSlideSpeed -= totalDecel * Time.fixedDeltaTime;
            }

            // Player can steer and carve slide direction with mouse aim and full WASD redirection (Redliner Slide Carving)
            Vector3 aimDir = moveInput.sqrMagnitude > 0.05f 
                ? (transform.forward * moveInput.y + transform.right * moveInput.x).normalized 
                : transform.forward;
            if (aimDir.sqrMagnitude > 0.05f)
            {
                Vector3 projectedAim = Vector3.ProjectOnPlane(aimDir, groundNormal).normalized;
                slideDirection = Vector3.RotateTowards(slideDirection, projectedAim, slideSteerRate * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
            }

            currentSlideSpeed = Mathf.Clamp(currentSlideSpeed, 0f, maxSpeed);

            // Natural transition: When slide slows down to walking speed, smoothly drop into crouch walk
            if (currentSlideSpeed < minSlideSpeed)
            {
                isSliding = false;
            }
        }
    }

    private void StartSlide(float targetSpeed)
    {
        isSliding = true;
        isCrouching = true;
        slideTimer = minSlideDuration;
        airborneSlideBufferTimer = 0f;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVel.sqrMagnitude > 0.5f)
        {
            slideDirection = horizontalVel.normalized;
        }
        else if (moveInput.sqrMagnitude > 0.1f)
        {
            slideDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        }
        else
        {
            slideDirection = transform.forward;
        }

        currentSlideSpeed = Mathf.Clamp(Mathf.Max(targetSpeed, minSlideEntrySpeed), minSlideEntrySpeed, maxSpeed);

        if (audioEffects != null) audioEffects.PlaySlideStart();
        if (mouseLook != null) mouseLook.AddScreenShake(0.12f, -transform.forward);
    }

    public void BoostSlide(float boostAmount)
    {
        if (isSliding)
        {
            currentSlideSpeed = Mathf.Min(currentSlideSpeed + boostAmount, maxSpeed);
        }
        else
        {
            StartSlide(CurrentSpeed + boostAmount);
        }

        if (audioEffects != null) audioEffects.PlaySlideBoost();
    }

    private bool HasOverheadClearance()
    {
        float currentTopY = feetLocalY + capsule.height;
        Vector3 origin = transform.position + Vector3.up * currentTopY;
        float radius = capsule.radius * 0.8f;
        float checkDistance = standingHeight - capsule.height + 0.1f;

        bool blocked = Physics.SphereCast(origin, radius, Vector3.up, out RaycastHit hit, checkDistance, overheadClearanceMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    private void ProcessBufferedJump()
    {
        bool canJump = (Time.time - lastGroundedTime <= coyoteTime);
        // Supports both frame-tap jump buffering and Redliner-style hold-Space continuous bunny-hop chaining!
        bool wantsJump = (Time.time - lastJumpPressedTime <= jumpBufferTime) || isJumpHeld;

        // Elastic Slingshot Jump: Pressing jump while grappling flings the player with extra aerial impulse
        if (wantsJump && pullAbility != null && pullAbility.IsGrappling)
        {
            lastJumpPressedTime = -10f;
            pullAbility.FlingGrappleJump();
            return;
        }

        if (canJump && wantsJump)
        {
            lastJumpPressedTime = -10f;
            lastGroundedTime = -10f;
            isGrounded = false;

            Vector3 currentVel = rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);
            float hSpeed = horizontalVel.magnitude;
            bool wasSlidingBeforeJump = isSliding;

            // Preserve running momentum state in air so landing slide can chain cleanly
            if (isSprinting || hSpeed >= runThreshold || wasSlidingBeforeJump)
            {
                airborneRunPreserved = true;
            }

            if (wasSlidingBeforeJump || isSprinting || hSpeed >= runThreshold)
            {
                // SLIDE -> JUMP CHAIN & SPRINT JUMP (REDLINER BUNNY-HOP SPEED STACKING)
                // Preserves 100% of horizontal speed PLUS injects bHopBoost (+2.5 m/s)!
                float baseSpeed = Mathf.Max(hSpeed, currentSlideSpeed);
                float boostedSpeed = Mathf.Min(baseSpeed + bHopBoost, maxSpeed);

                Vector3 hopDir = horizontalVel.sqrMagnitude > 0.1f ? horizontalVel.normalized : transform.forward;

                // Subtle steering influence toward movement input
                Vector3 wishDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
                if (wishDir.sqrMagnitude > 0.1f)
                {
                    hopDir = Vector3.RotateTowards(hopDir, wishDir, 35f * Mathf.Deg2Rad, 0f);
                }

                Vector3 newHVel = hopDir * boostedSpeed;
                rb.linearVelocity = new Vector3(newHVel.x, jumpImpulse, newHVel.z);

                isSliding = false;
                airborneSlideBufferTimer = 0f;

                if (audioEffects != null) audioEffects.PlaySlideBoost();
                if (mouseLook != null) mouseLook.AddScreenShake(0.1f, Vector3.up);
            }
            else
            {
                currentVel.y = jumpImpulse;
                rb.linearVelocity = currentVel;

                if (audioEffects != null) audioEffects.PlayFootstep();
            }
        }
    }

    /// <summary>
    /// Smooth ledge mantling: when falling near a wall or platform edge while moving forward,
    /// automatically detects the top surface and hoists the player up cleanly (02_REBOUND_CURRENT_BASELINE.md:144).
    /// </summary>
    private void ProcessLedgeMantle()
    {
        if (isGrounded || isSliding || isGrappling || rb.linearVelocity.y > 1.2f || moveInput.y < 0.2f) return;

        Vector3 chestOrigin = transform.position + Vector3.up * 0.2f;
        float forwardCheckDist = 0.85f;

        if (Physics.Raycast(chestOrigin, transform.forward, out RaycastHit wallHit, forwardCheckDist, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (wallHit.normal.y < 0.35f)
            {
                Vector3 ledgeProbeOrigin = wallHit.point + transform.forward * 0.35f + Vector3.up * 1.2f;
                if (Physics.Raycast(ledgeProbeOrigin, Vector3.down, out RaycastHit ledgeHit, 1.4f, groundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (ledgeHit.normal.y > 0.65f && ledgeHit.point.y > transform.position.y)
                    {
                        Vector3 targetStandPos = new Vector3(ledgeHit.point.x, ledgeHit.point.y + Mathf.Abs(feetLocalY) + 0.05f, ledgeHit.point.z);
                        if (!Physics.CheckCapsule(targetStandPos + Vector3.up * 0.5f, targetStandPos + Vector3.up * 1.5f, 0.4f, overheadClearanceMask, QueryTriggerInteraction.Ignore))
                        {
                            Vector3 mantleVel = new Vector3(transform.forward.x * 3.5f, jumpImpulse * 0.65f, transform.forward.z * 3.5f);
                            rb.linearVelocity = mantleVel;
                            if (audioEffects != null) audioEffects.PlayFootstep();
                            if (mouseLook != null) mouseLook.AddScreenShake(0.08f, Vector3.up);
                        }
                    }
                }
            }
        }
    }

    public void ApplyRecoil(Vector3 recoilVelocity)
    {
        if (isSliding)
        {
            // SLIDE -> RECOIL-CONVERT BOOST!
            float recoilMagnitude = new Vector3(recoilVelocity.x, 0f, recoilVelocity.z).magnitude;
            BoostSlide(recoilMagnitude + slideRecoilBoost);

            if (recoilVelocity.y > 0f)
            {
                Vector3 currentVel = rb.linearVelocity;
                currentVel.y = Mathf.Max(0f, currentVel.y) + recoilVelocity.y;
                rb.linearVelocity = currentVel;
            }
            return;
        }

        Vector3 newVel = rb.linearVelocity;

        // Airborne Vertical Rocket Jump Limiter: Max 1 upward rocket jump while airborne
        if (!isGrounded && recoilVelocity.y > 2.0f)
        {
            if (hasAirRocketJumped)
            {
                // Already rocket-jumped in air: suppress infinite vertical climbing, keep horizontal wall redirection!
                recoilVelocity.y = 0f;
            }
            else
            {
                hasAirRocketJumped = true;
            }
        }

        if (recoilVelocity.y > 0f)
        {
            newVel.y = Mathf.Max(0f, newVel.y) + recoilVelocity.y;
        }
        else
        {
            newVel.y += recoilVelocity.y;
        }

        newVel.x += recoilVelocity.x;
        newVel.z += recoilVelocity.z;

        rb.linearVelocity = newVel;
    }

    public void SetSlowMotionActive(bool active)
    {
        isSlowMotionActive = active;
    }

    private void CheckGroundStatus()
    {
        Vector3 checkPos = transform.position + Vector3.up * (feetLocalY + 0.15f);
        if (groundCheck != null) checkPos = groundCheck.position;

        Collider[] hits = Physics.OverlapSphere(checkPos, groundCheckRadius, groundLayerMask, QueryTriggerInteraction.Ignore);
        bool foundGround = false;
        groundedObject = null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform != transform && !hits[i].transform.IsChildOf(transform))
            {
                foundGround = true;
                groundedObject = hits[i].transform;
                break;
            }
        }

        if (!foundGround)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Abs(feetLocalY) + 0.35f, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                {
                    foundGround = true;
                    groundedObject = hit.transform;
                }
            }
        }

        isGrounded = foundGround;

        // Ground normal for slope surfing
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit normalHit, Mathf.Abs(feetLocalY) + 0.6f, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            groundNormal = normalHit.normal;
        }
        else
        {
            groundNormal = Vector3.up;
        }

        if (isGrounded && !wasGroundedLastFrame)
        {
            lastGroundedTime = Time.time;
            hasAirRocketJumped = false;
            if (mouseLook != null && previousVerticalVelocity < -2.0f)
            {
                mouseLook.TriggerLandingDip(previousVerticalVelocity);
                if (audioEffects != null) audioEffects.PlayFootstep();
            }
        }
        else if (isGrounded)
        {
            lastGroundedTime = Time.time;
            hasAirRocketJumped = false;
        }
    }

    private void ApplyMovementVelocity()
    {
        if (isSliding && isGrounded)
        {
            // Project slide velocity onto the terrain slope surface
            Vector3 slideVel = Vector3.ProjectOnPlane(slideDirection, groundNormal).normalized * currentSlideSpeed;
            // Preserves downward slope speed, but allows natural launch trajectory
            rb.linearVelocity = new Vector3(slideVel.x, slideVel.y, slideVel.z);
            return;
        }

        Vector3 targetDirection = (transform.forward * moveInput.y + transform.right * moveInput.x);
        Vector3 currentVel = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);
        float currentHorizontalSpeed = horizontalVel.magnitude;

        if (isGrounded)
        {
            float targetSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed; // 3.5 m/s
            }
            else if (isSprinting && moveInput.y > 0.1f)
            {
                targetSpeed = sprintSpeed; // 11.5 m/s sprint
            }
            else
            {
                targetSpeed = walkSpeed; // 6.5 m/s walk
            }

            Vector3 targetVelocity = targetDirection * targetSpeed;

            // MOMENTUM-AWARE GROUND CONTROL (Section D1/D2 in 01_MASTER_AGENT_PROMPT.md):
            // Never abruptly snap or zero out inherited velocity from aerial hops, grapple flings, or slide exits.
            Vector3 newHorizontalVel;
            if (currentHorizontalSpeed > targetSpeed + 0.5f)
            {
                // Carrying high momentum:
                if (moveInput.sqrMagnitude > 0.05f)
                {
                    Vector3 wishDir = targetDirection.normalized;
                    float dot = Vector3.Dot(horizontalVel.normalized, wishDir);
                    if (dot > 0.1f)
                    {
                        // Moving along wish direction: gentle aerodynamic drag preserves speed runway
                        newHorizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, groundMomentumDrag * Time.fixedDeltaTime);
                        newHorizontalVel = Vector3.RotateTowards(newHorizontalVel, wishDir, (airAcceleration * 1.2f / Mathf.Max(currentHorizontalSpeed, 6f)) * Time.fixedDeltaTime, 0f);
                    }
                    else
                    {
                        // Opposing input: active responsive braking
                        newHorizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, groundBraking * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    // No input: predictable deceleration to rest
                    newHorizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, groundBraking * Time.fixedDeltaTime);
                }
            }
            else
            {
                // Normal acceleration towards target speed
                newHorizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, groundAcceleration * Time.fixedDeltaTime);
            }

            rb.linearVelocity = new Vector3(newHorizontalVel.x, rb.linearVelocity.y, newHorizontalVel.z);
        }
        else
        {
            // AIRBORNE / BUNNY-HOP PRESERVATION:
            // When flying off a hill or jumping out of a slide:
            if (currentHorizontalSpeed > (walkSpeed - 0.5f))
            {
                // REDLINER / SOURCE AIR-STRAFE ACCELERATION:
                // Preserves 100% of accumulated horizontal momentum PLUS gains speed when strafing into turns!
                if (targetDirection.sqrMagnitude > 0.05f)
                {
                    Vector3 wishDir = targetDirection.normalized;
                    float currentProjection = Vector3.Dot(horizontalVel, wishDir);
                    float maxAirWishSpeed = 16.0f;
                    float addSpeed = maxAirWishSpeed - currentProjection;

                    if (addSpeed > 0f)
                    {
                        float accelSpeed = airAcceleration * Time.fixedDeltaTime;
                        float speedToApply = Mathf.Min(addSpeed, accelSpeed);
                        horizontalVel += wishDir * speedToApply;
                    }

                    // Steer velocity smoothly towards wish direction
                    horizontalVel = Vector3.RotateTowards(horizontalVel, wishDir, (airAcceleration * 1.6f / Mathf.Max(currentHorizontalSpeed, 6f)) * Time.fixedDeltaTime, 0f);

                    if (horizontalVel.magnitude > maxSpeed)
                    {
                        horizontalVel = horizontalVel.normalized * maxSpeed;
                    }

                    rb.linearVelocity = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);
                }
                // If not steering, preserve horizontal velocity 100%!
            }
            else
            {
                float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
                Vector3 targetVelocity = targetDirection * targetSpeed;
                Vector3 newHorizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, airAcceleration * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector3(newHorizontalVel.x, currentVel.y, newHorizontalVel.z);
            }
        }
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded && !isSliding) return;
        if (isGrappling) return; // Tether counter-balances downward gravity for clean swings

        float gravityMultiplier = isSlowMotionActive ? slowMoGravityScale : standardGravityScale;
        Vector3 extraGravity = Physics.gravity * (gravityMultiplier - 1f) * Time.fixedDeltaTime;
        rb.AddForce(extraGravity, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Applies elastic grapple tether acceleration pulling the player body toward the anchor point.
    /// Preserves tangential momentum for realistic Redliner swinging and slide slingshots!
    /// </summary>
    public void ApplyGrapplePull(Vector3 anchor, Vector3 pullDir, float distance)
    {
        isGrappling = true;
        grappleAnchorPoint = anchor;

        float targetSpeed = Mathf.Lerp(24.0f, 35.0f, Mathf.Clamp01(distance / 35.0f));

        // If pulling toward ground or downward ramp while moving, enter power slide!
        if (isGrounded && pullDir.y <= 0.25f && !isSliding)
        {
            StartSlide(Mathf.Max(CurrentSpeed + 4.0f, targetSpeed));
        }

        Vector3 currentVel = rb.linearVelocity;

        // Elastic pull force along tether direction (55 m/s^2)
        float pullAccel = 55.0f;
        Vector3 pullForce = pullDir * (pullAccel * Time.fixedDeltaTime);
        Vector3 newVel = currentVel + pullForce;

        // Cap inward speed towards anchor, preserving perpendicular swinging momentum
        float dot = Vector3.Dot(newVel, pullDir);
        if (dot > targetSpeed)
        {
            newVel -= pullDir * (dot - targetSpeed);
        }

        // Clamp overall velocity to maxSpeed
        if (newVel.magnitude > maxSpeed)
        {
            newVel = newVel.normalized * maxSpeed;
        }

        rb.linearVelocity = newVel;
    }

    public void EndGrapple()
    {
        isGrappling = false;
    }

    /// <summary>
    /// Releases the grapple tether with explosive spring tension forward fling.
    /// </summary>
    public void FlingGrapple(Vector3 flingDirection, float flingBoost)
    {
        isGrappling = false;
        Vector3 currentVel = rb.linearVelocity;
        float currentSpeed = currentVel.magnitude;

        // Slingshot fling: injects boost on top of existing speed, minimum 16 m/s
        float boostedSpeed = Mathf.Min(Mathf.Max(currentSpeed, 16.0f) + flingBoost, maxSpeed);

        // Blend look direction with current momentum direction for natural physics handoff
        Vector3 momentumDir = currentSpeed > 2.0f ? currentVel.normalized : transform.forward;
        Vector3 launchDir = Vector3.Slerp(momentumDir, flingDirection.normalized, 0.7f).normalized;

        if (flingDirection.y > 0.05f)
        {
            launchDir.y = Mathf.Max(launchDir.y, 0.25f);
            launchDir.Normalize();
        }

        rb.linearVelocity = launchDir * boostedSpeed;
        airborneRunPreserved = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Slide Bowling Ram: Crushes physics targets and breakable barricades at high speed
        if (isSliding && CurrentSpeed >= 7.5f)
        {
            IKineticTarget target = collision.gameObject.GetComponentInParent<IKineticTarget>();
            if (target != null)
            {
                target.OnPlayerSlideRam(CurrentSpeed);
                if (audioEffects != null) audioEffects.PlayImpactCrunch(0.65f);
                if (mouseLook != null) mouseLook.AddScreenShake(0.18f, -transform.forward);
            }
        }
    }

    private void ProcessFootstepAudio()
    {
        if (!isGrounded || isSliding || isGrappling || moveInput.sqrMagnitude < 0.05f || CurrentSpeed < 0.5f)
        {
            footstepTimer = 0f;
            return;
        }

        // Footstep cadence speeds up dynamically when sprinting vs walking vs crouching
        float stepRate = isCrouching ? 0.2f : (isSprinting ? 0.45f : 0.32f);
        footstepTimer += Time.deltaTime * (CurrentSpeed * stepRate);
        if (footstepTimer >= 1.0f)
        {
            footstepTimer = 0f;
            if (audioEffects != null) audioEffects.PlayFootstep();
        }
    }
}
