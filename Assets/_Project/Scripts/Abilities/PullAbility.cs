using UnityEngine;

/// <summary>
/// Dual-Mode Pull Ability:
/// 1. Gravity Gun Snatch & Crate Cannon: Aimed at crates, props, or light enemies reels them to your hands.
/// 2. Redliner Elastic Grapple Slingshot: Aimed at walls, floors, ramps, ceilings, or grapple anchors
///    shoots an elastic tether that pulls the player body at high speed (24-28 m/s).
///    Releasing RMB or tapping Space triggers an explosive slingshot fling forward into slides or leaps!
/// Specified in docs/TEAM_DEVELOPMENT_HANDBOOK.md & README.md
/// </summary>
public class PullAbility : MonoBehaviour
{
    [Header("Vacuum Suction Tuning (Props)")]
    [Tooltip("Maximum range to snatch crates and props (extended to 45m for high-speed mid-air aerial snatching).")]
    [SerializeField] private float maxPullRange = 45.0f;
    [SerializeField] private float hoverDistance = 2.5f;
    [SerializeField] private float maxReelSpeed = 24.0f;
    [SerializeField] private float leashBreakDistance = 55.0f;

    [Header("Grapple Traversal Mode")]
    [Tooltip("If false, grapple hook traversal is disabled so Pull focuses exclusively on manipulating physics props.")]
    [SerializeField] private bool enableGrappleTraversal = false;

    [Header("Grapple Slingshot Tuning (World & Anchors)")]
    [Tooltip("Maximum distance to attach a grapple hook to walls, floors, or anchors.")]
    [SerializeField] private float maxGrappleRange = 75.0f;

    [Tooltip("Forward velocity boost applied when releasing RMB or jumping during a grapple.")]
    [SerializeField] private float grappleFlingBoost = 6.5f;

    [Tooltip("Distance from anchor where grapple automatically detaches to prevent sticking.")]
    [SerializeField] private float minGrappleDetachDistance = 2.5f;

    [Header("Crate Cannon Launch")]
    [Tooltip("Projectile speed when fired with Crate Cannon combo.")]
    [SerializeField] private float launchSpeed = 35.0f;

    [Header("Hierarchy & Origin References")]
    [SerializeField] private Transform handTetherOrigin;
    [SerializeField] private Transform abilityRayOrigin;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private LineRenderer vortexRings;
    [SerializeField] private LineRenderer grappleCable;

    [Header("Layers")]
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private LayerMask grappleLayers;
    [SerializeField] private LayerMask obstructionLayers;

    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private MouseLook mouseLook;
    private PlayerAudioEffects audioEffects;

    private Rigidbody heldRigidbody;
    private InteractableProp heldProp;
    private Collider heldCollider;
    private LayerMask heldOriginalExcludedLayers;
    private IKineticCarryTarget heldCarryTarget;
    private bool isPulling = false;
    private Vector3 initialHandLocalPos;
    private float vortexAngle = 0f;

    // Grapple State
    private bool isGrappling = false;
    private Vector3 grapplePoint;
    private Vector3 grappleNormal;
    private Collider grappleCollider;
    private float grappleDuration = 0f;

    // Public API Contracts
    public bool IsCarrying => heldRigidbody != null;
    public bool IsGrappling => isGrappling;
    public bool IsPulling => isPulling;
    public bool EnableGrappleTraversal => enableGrappleTraversal;
    public Vector3 GrapplePoint => grapplePoint;
    public Rigidbody HeldRigidbody => heldRigidbody;
    public InteractableProp HeldProp => heldProp;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCamera = GetComponentInParent<Camera>();
        mouseLook = GetComponentInParent<MouseLook>();
        audioEffects = GetComponentInParent<PlayerAudioEffects>();

        if (playerCamera == null) playerCamera = Camera.main;

        initialHandLocalPos = transform.localPosition;

        if (interactableLayers.value == 0)
        {
            interactableLayers = LayerMask.GetMask("Interactables", "Enemies", "Projectiles");
        }
        if (grappleLayers.value == 0)
        {
            int envLayer = LayerMask.NameToLayer("Environment");
            int anchorLayer = LayerMask.NameToLayer("GrappleAnchors");
            int defLayer = LayerMask.NameToLayer("Default");

            int mask = 0;
            if (envLayer != -1) mask |= (1 << envLayer);
            if (anchorLayer != -1) mask |= (1 << anchorLayer);
            if (defLayer != -1) mask |= (1 << defLayer);
            grappleLayers = mask != 0 ? mask : ~LayerMask.GetMask("Player", "Ignore Raycast");
        }
        if (obstructionLayers.value == 0)
        {
            obstructionLayers = LayerMask.GetMask("Environment");
        }

        SetupVortexVisual();
        SetupTrajectoryVisual();
        SetupGrappleVisual();
    }

    private void SetupVortexVisual()
    {
        if (vortexRings == null)
        {
            vortexRings = gameObject.AddComponent<LineRenderer>();
            vortexRings.useWorldSpace = true;
            vortexRings.positionCount = 36;
            vortexRings.startWidth = 0.05f;
            vortexRings.endWidth = 0.01f;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            mat.color = new Color(0.2f, 0.9f, 1f, 0.6f);
            vortexRings.material = mat;
            vortexRings.enabled = false;
        }
    }

    private void SetupTrajectoryVisual()
    {
        if (trajectoryLine == null)
        {
            GameObject trajObj = new GameObject("TrajectoryLine");
            trajObj.transform.SetParent(transform);
            trajectoryLine = trajObj.AddComponent<LineRenderer>();
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.positionCount = 20;
            trajectoryLine.startWidth = 0.035f;
            trajectoryLine.endWidth = 0.015f;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            mat.color = new Color(0.1f, 0.85f, 1f, 0.5f);
            trajectoryLine.material = mat;
            trajectoryLine.enabled = false;
        }
    }

    private void SetupGrappleVisual()
    {
        if (grappleCable == null)
        {
            GameObject cableObj = new GameObject("GrappleCable");
            cableObj.transform.SetParent(transform);
            grappleCable = cableObj.AddComponent<LineRenderer>();
            grappleCable.useWorldSpace = true;
            grappleCable.positionCount = 16;
            grappleCable.startWidth = 0.045f;
            grappleCable.endWidth = 0.025f;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            mat.color = new Color(0.1f, 0.95f, 1.0f, 0.95f);
            grappleCable.material = mat;
            grappleCable.enabled = false;
        }
    }

    public void StartPull()
    {
        isPulling = true;
        AcquireTarget();
        if (audioEffects != null && heldRigidbody != null) audioEffects.SetSuctionLoop(true);
    }

    public void StopPull()
    {
        isPulling = false;

        if (heldRigidbody != null)
        {
            ReleaseHeldObject();
        }
        else if (isGrappling)
        {
            // Slingshot Fling when releasing RMB while tethered!
            StopGrapple(false);
        }

        if (audioEffects != null) audioEffects.SetSuctionLoop(false);
    }

    public void ResetState()
    {
        StopPull();
    }

    /// <summary>
    /// Dual-mode target acquisition:
    /// Priority 1: Pick up interactive props within maxPullRange.
    /// Priority 2: Hook elastic grapple to walls, floors, or anchors within maxGrappleRange.
    /// </summary>
    private void AcquireTarget()
    {
        // Aiming ray ALWAYS originates from camera center for true crosshair alignment (zero parallax)
        Vector3 origin = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 direction = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        // Priority 1: Interactive Prop / Crate Snatch (within maxPullRange = 45m)
        // Primary probe: Sharp raycast
        bool propFound = Physics.Raycast(origin, direction, out RaycastHit propHit, maxPullRange, interactableLayers, QueryTriggerInteraction.Ignore);
        // Fallback probe: Focused forward spherecast starting 1.5m ahead of camera to prevent self-collision
        if (!propFound && maxPullRange > 1.5f)
        {
            Vector3 assistOrigin = origin + direction * 1.5f;
            propFound = Physics.SphereCast(assistOrigin, 0.35f, direction, out propHit, maxPullRange - 1.5f, interactableLayers, QueryTriggerInteraction.Ignore);
        }

        if (propFound && propHit.collider != null)
        {
            // Anti-box-riding: Reject pulling/holding an object the player is standing on
            bool isStandingOnProp = playerMovement != null && playerMovement.GroundedObject != null &&
                (playerMovement.GroundedObject == propHit.transform || playerMovement.GroundedObject.IsChildOf(propHit.transform));

            if (!isStandingOnProp)
            {
                // Bullets and projectiles can NEVER be pulled
                if (propHit.collider.GetComponentInParent<EnemyProjectile>() != null ||
                    propHit.collider.GetComponentInParent<IDeflectableProjectile>() != null)
                {
                    return;
                }

                IKineticTarget target = propHit.collider.GetComponentInParent<IKineticTarget>();
                if (target == null || target.CanBePulled)
                {
                    Rigidbody targetRb = propHit.collider.attachedRigidbody;
                    if (targetRb != null && !targetRb.isKinematic)
                    {
                        IKineticCarryTarget carryTarget = target as IKineticCarryTarget;
                        if (carryTarget != null && !carryTarget.TryBeginCarry()) return;

                        heldRigidbody = targetRb;
                        heldOriginalExcludedLayers = targetRb.excludeLayers;
                        heldProp = propHit.collider.GetComponentInParent<InteractableProp>();
                        if (heldProp != null)
                        {
                            heldProp.SetCarriedState(true);
                            targetRb.excludeLayers = heldOriginalExcludedLayers.value | LayerMask.GetMask("Enemies", "Enemy");
                        }
                        heldCarryTarget = carryTarget;
                        heldRigidbody.isKinematic = true;
                        heldRigidbody.angularDamping = 6.0f;

                        // Ignore collision between held prop and player to eliminate jitter / self-propulsion
                        Collider playerCol = playerMovement != null ? playerMovement.GetComponent<Collider>() : null;
                        if (playerCol != null && propHit.collider != null)
                        {
                            heldCollider = propHit.collider;
                            Physics.IgnoreCollision(heldCollider, playerCol, true);
                        }

                        isGrappling = false;
                        if (grappleCable != null) grappleCable.enabled = false;
                        return;
                    }
                }
            }
        }

        // Priority 2: Elastic Grapple Slingshot to static geometry or anchors (if enabled)
        if (!enableGrappleTraversal) return;

        // Primary probe: Sharp raycast up to maxGrappleRange (75m)
        RaycastHit grappleHit;
        bool hitFound = Physics.Raycast(origin, direction, out grappleHit, maxGrappleRange, grappleLayers, QueryTriggerInteraction.Ignore);

        // Fallback probe: Tight spherecast (radius 0.25m) starting 2.0m ahead to catch distant pillars without hitting ground at feet
        if (!hitFound && maxGrappleRange > 2.0f)
        {
            Vector3 assistOrigin = origin + direction * 2.0f;
            hitFound = Physics.SphereCast(assistOrigin, 0.25f, direction, out grappleHit, maxGrappleRange - 2.0f, grappleLayers, QueryTriggerInteraction.Ignore);
        }

        // Must hit valid surface at least 4.0m away from the player (prevents hooking floor at feet or instant auto-detach)
        if (hitFound && grappleHit.collider != null && Vector3.Distance(origin, grappleHit.point) >= 4.0f)
        {
            StartGrapple(grappleHit.point, grappleHit.normal, grappleHit.collider);
        }
    }

    private void StartGrapple(Vector3 point, Vector3 normal, Collider hookedCollider)
    {
        Vector3 origin = playerCamera != null ? playerCamera.transform.position : transform.position;
        if (Vector3.Distance(origin, point) < 4.0f) return;

        isGrappling = true;
        grapplePoint = point;
        grappleNormal = normal;
        grappleCollider = hookedCollider;
        grappleDuration = 0f;

        if (grappleCable != null)
        {
            grappleCable.enabled = true;
            grappleCable.SetPosition(0, GetHandPosition());
            grappleCable.SetPosition(1, grapplePoint);
        }

        if (audioEffects != null) audioEffects.PlayGrappleAttach();
        if (mouseLook != null) mouseLook.AddScreenShake(0.12f, (grapplePoint - transform.position).normalized);
    }

    private void StopGrapple(bool interrupted)
    {
        if (!isGrappling) return;

        isGrappling = false;
        if (grappleCable != null) grappleCable.enabled = false;

        if (playerMovement != null)
        {
            if (!interrupted)
            {
                // Slingshot forward fling with spring tension
                Vector3 flingDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;
                playerMovement.FlingGrapple(flingDir, grappleFlingBoost);
                if (audioEffects != null) audioEffects.PlayGrappleFling();
                if (mouseLook != null) mouseLook.AddScreenShake(0.18f, flingDir);
            }
            else
            {
                playerMovement.EndGrapple();
            }
        }
    }

    /// <summary>
    /// Slingshot jump: called when Space is pressed during an active grapple tether.
    /// </summary>
    public void FlingGrappleJump()
    {
        if (!isGrappling) return;

        isGrappling = false;
        if (grappleCable != null) grappleCable.enabled = false;

        if (playerMovement != null)
        {
            Vector3 flingDir = playerCamera != null
                ? (playerCamera.transform.forward * 0.75f + Vector3.up * 0.45f).normalized
                : (transform.forward + Vector3.up * 0.5f).normalized;

            playerMovement.FlingGrapple(flingDir, grappleFlingBoost + 2.5f);
            if (audioEffects != null) audioEffects.PlayGrappleFling();
            if (mouseLook != null) mouseLook.AddScreenShake(0.25f, flingDir);
        }
    }

    /// <summary>
    /// Launches the carried crate forward as a high-speed projectile (Crate Cannon).
    /// </summary>
    public bool TryLaunchHeldObject()
    {
        if (heldRigidbody == null) return false;
        if (heldProp != null && !heldProp.CanBeLaunched) return false;

        Rigidbody launchedRb = heldRigidbody;
        IKineticCarryTarget launchedTarget = heldCarryTarget;

        ReleaseHeldObject();

        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 camFwd = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Vector3 targetPoint = camPos + camFwd * 50f;
        Vector3 launchDir = (targetPoint - launchedRb.position).normalized;
        Vector3 playerVel = playerMovement != null ? playerMovement.Velocity : Vector3.zero;

        // Crate Cannon Velocity Inheritance: 100% of launch speed + 45% of player forward velocity
        Vector3 launchVelocity = launchDir * launchSpeed + playerVel * 0.45f;
        launchedRb.isKinematic = false;
        launchedRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        launchedRb.linearVelocity = launchVelocity;
        launchedTarget?.OnLaunched();

        // Apply recoil to player through authoritative movement controller API
        if (playerMovement != null)
        {
            playerMovement.ApplyRecoil(-launchDir * 6.5f);
        }

        // Camera shake & sound
        if (mouseLook != null) mouseLook.AddScreenShake(0.35f, -launchDir);
        if (audioEffects != null) audioEffects.PlayCrateLaunch();

        return true;
    }

    private void FixedUpdate()
    {
        if (heldRigidbody != null)
        {
            UpdateHeldPhysics();
        }
        else if (isGrappling)
        {
            UpdateGrapplePhysics();
        }
    }

    private void Update()
    {
        UpdateVortexVisual();
        UpdateTrajectoryVisual();
        UpdateGrappleVisual();
        UpdateHandVibration();
    }

    private void UpdateGrapplePhysics()
    {
        if (!isGrappling || playerMovement == null) return;

        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 toAnchor = grapplePoint - camPos;
        float distance = toAnchor.magnitude;

        // Break tether only if pulled excessively far beyond max range
        if (distance > maxGrappleRange + 15.0f)
        {
            StopGrapple(true);
            return;
        }

        // True obstruction check: ignore the hooked collider itself
        if (distance > 5.0f)
        {
            Vector3 probeStart = camPos + toAnchor.normalized * 0.8f;
            Vector3 probeEnd = grapplePoint + grappleNormal * 0.35f;
            Vector3 probeDir = probeEnd - probeStart;
            float probeDist = probeDir.magnitude;

            if (Physics.Raycast(probeStart, probeDir.normalized, out RaycastHit obsHit, probeDist, obstructionLayers, QueryTriggerInteraction.Ignore))
            {
                // Only disconnect if obstructed by an alien obstacle (not the hooked surface itself)
                if (obsHit.collider != grappleCollider && (grappleCollider == null || obsHit.collider.transform != grappleCollider.transform))
                {
                    StopGrapple(true);
                    return;
                }
            }
        }

        // Auto-detach when reaching the anchor point to prevent sticking
        if (distance <= minGrappleDetachDistance)
        {
            FlingGrappleJump();
            return;
        }

        grappleDuration += Time.fixedDeltaTime;
        Vector3 pullDir = toAnchor.normalized;
        playerMovement.ApplyGrapplePull(grapplePoint, pullDir, distance);
    }

    private void UpdateHeldPhysics()
    {
        if (heldRigidbody == null) return;

        Vector3 camPos = playerCamera != null ? playerCamera.transform.position : transform.position;
        Vector3 camForward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Vector3 camRight = playerCamera != null ? playerCamera.transform.right : transform.right;
        Vector3 camUp = playerCamera != null ? playerCamera.transform.up : transform.up;

        // Obstruction retraction: clamp hold distance when walls or obstacles intervene (ignoring held prop itself!)
        float targetDistance = Mathf.Max(hoverDistance, 3.2f);
        if (Physics.SphereCast(camPos, 0.35f, camForward, out RaycastHit obsHit, targetDistance, obstructionLayers, QueryTriggerInteraction.Ignore))
        {
            if (heldCollider == null || (obsHit.collider != heldCollider && !obsHit.collider.transform.IsChildOf(heldRigidbody.transform)))
            {
                targetDistance = Mathf.Max(obsHit.distance - 0.3f, 2.4f);
            }
        }

        // Ergonomic Telekinesis Hold Position:
        // Positioned in bottom-right pocket so the object NEVER blocks the crosshair or center vision!
        Vector3 hoverTarget = camPos + (camForward * targetDistance) + (camRight * 1.15f) - (camUp * 0.55f);

        // Heavy crate ground drag rule: heavy objects cannot hover or fly; clamped to ground plane
        if (heldProp != null && heldProp.Class == WeightClass.Heavy)
        {
            hoverTarget.y = heldRigidbody.position.y;
        }

        // 1. Leash Break Check
        if (Vector3.Distance(camPos, heldRigidbody.position) > leashBreakDistance ||
            Physics.Linecast(camPos, heldRigidbody.position, obstructionLayers))
        {
            ReleaseHeldObject();
            return;
        }

        // 2. Weight-Class Responsive Glide into pocket:
        // Light (~1kg): rapid 32 m/s snap
        // Medium (~5kg): agile 24 m/s reel
        // Heavy (~25kg): 12 m/s ground drag
        Vector3 toTarget = hoverTarget - heldRigidbody.position;
        float distance = toTarget.magnitude;

        float classReelSpeed = maxReelSpeed;

        if (heldProp != null)
        {
            switch (heldProp.Class)
            {
                case WeightClass.Light:
                    classReelSpeed = 32.0f;
                    break;
                case WeightClass.Medium:
                    classReelSpeed = 24.0f;
                    break;
                case WeightClass.Heavy:
                    classReelSpeed = 12.0f;
                    break;
            }
        }

        Vector3 nextPos = heldRigidbody.position;
        int solidLayers = obstructionLayers | LayerMask.GetMask("Default");
        BoxCollider box = heldCollider as BoxCollider;
        Vector3 halfSize = heldCollider != null ? heldCollider.bounds.extents : Vector3.zero;
        if (box != null)
        {
            Vector3 scale = box.transform.lossyScale;
            halfSize = Vector3.Scale(box.size * 0.5f,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        }
        Vector3 castExtents = Vector3.Max(halfSize - Vector3.one * 0.025f, Vector3.one * 0.01f);
        if (distance > 0.02f)
        {
            nextPos = Vector3.MoveTowards(heldRigidbody.position, hoverTarget, classReelSpeed * Time.fixedDeltaTime);
            if (heldCollider != null)
            {
                Quaternion orientation = box != null ? box.transform.rotation : Quaternion.identity;
                Vector3 step = nextPos - heldRigidbody.position;
                nextPos = heldRigidbody.position + ResolveHeldTravel(
                    heldCollider.bounds.center, castExtents, orientation, step, solidLayers);
            }
            heldRigidbody.MovePosition(nextPos);
        }

        // 3. Auto-align rotation to face forward with player camera (Riot Shielding)
        Quaternion targetRotation = playerCamera != null ? playerCamera.transform.rotation : transform.rotation;
        Quaternion nextRotation = Quaternion.Slerp(heldRigidbody.rotation, targetRotation, 14f * Time.fixedDeltaTime);
        if (box != null)
        {
            Quaternion rotationDelta = nextRotation * Quaternion.Inverse(heldRigidbody.rotation);
            Vector3 center = nextPos + rotationDelta * (box.bounds.center - heldRigidbody.position);
            if (Physics.CheckBox(center, castExtents,
                    rotationDelta * box.transform.rotation, solidLayers, QueryTriggerInteraction.Ignore)) return;
        }
        heldRigidbody.MoveRotation(nextRotation);
    }

    private static Vector3 ResolveHeldTravel(Vector3 center, Vector3 extents, Quaternion orientation, Vector3 step, int solidLayers)
    {
        const float clearance = 0.04f;
        Vector3 travel = Vector3.zero;
        // A second sweep lets the crate follow the wall while still stopping at corners.
        for (int sweep = 0; sweep < 2 && step.sqrMagnitude > 0.000001f; sweep++)
        {
            float length = step.magnitude;
            Vector3 direction = step / length;
            if (!Physics.BoxCast(center + travel, extents, direction, out RaycastHit wall, orientation,
                    length + clearance, solidLayers, QueryTriggerInteraction.Ignore))
            {
                travel += step;
                break;
            }

            Vector3 advance = direction * Mathf.Clamp(wall.distance - clearance, 0f, length);
            travel += advance;
            step = Vector3.ProjectOnPlane(step - advance, wall.normal);
        }
        return travel;
    }

    private void ReleaseHeldObject()
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.excludeLayers = heldOriginalExcludedLayers;
            heldRigidbody.angularDamping = 0.05f;
            if (playerMovement != null) heldRigidbody.linearVelocity = playerMovement.Velocity;
            if (heldProp != null) heldProp.SetCarriedState(false);

            // Restore collision between player and released prop
            if (heldCollider != null && playerMovement != null)
            {
                Collider playerCol = playerMovement.GetComponent<Collider>();
                if (playerCol != null)
                {
                    Physics.IgnoreCollision(heldCollider, playerCol, false);
                }
                heldCollider = null;
            }

            heldRigidbody = null;
            heldProp = null;
        }
        if (heldCarryTarget is UnityEngine.Object carryObject && carryObject != null)
        {
            heldCarryTarget.EndCarry();
        }
        heldCarryTarget = null;

        if (vortexRings != null) vortexRings.enabled = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }

    private void OnDisable()
    {
        ReleaseHeldObject();
        StopGrapple(true);
    }

    private void UpdateGrappleVisual()
    {
        if (grappleCable == null) return;

        if (isGrappling)
        {
            grappleCable.enabled = true;
            Vector3 handPos = GetHandPosition();
            int count = grappleCable.positionCount;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector3 straight = Vector3.Lerp(handPos, grapplePoint, t);
                // Subtle electric tension vibration
                float vib = Mathf.Sin(Time.time * 45f + t * 10f) * 0.04f * Mathf.Sin(t * Mathf.PI);
                grappleCable.SetPosition(i, straight + Vector3.up * vib);
            }
        }
        else
        {
            grappleCable.enabled = false;
        }
    }

    private void UpdateVortexVisual()
    {
        if (vortexRings == null) return;

        if (isPulling && heldRigidbody != null)
        {
            vortexRings.enabled = true;
            vortexAngle += Time.deltaTime * 12f;

            Vector3 handPos = GetHandPosition();
            Vector3 targetPos = heldRigidbody.position;

            int count = vortexRings.positionCount;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                Vector3 center = Vector3.Lerp(targetPos, handPos, t);
                float radius = Mathf.Sin(t * Mathf.PI) * 0.35f;
                float angle = vortexAngle + t * Mathf.PI * 4f;

                Vector3 right = Vector3.Cross((handPos - targetPos).normalized, Vector3.up).normalized;
                Vector3 up = Vector3.Cross(right, (handPos - targetPos).normalized).normalized;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

                vortexRings.SetPosition(i, center + offset);
            }
        }
        else
        {
            vortexRings.enabled = false;
        }
    }

    private void UpdateTrajectoryVisual()
    {
        if (trajectoryLine == null) return;

        // Only draw trajectory line if holding a launchable prop (Heavy objects are ground-dragged, not launched)
        if (heldRigidbody != null && heldProp != null && heldProp.CanBeLaunched && heldProp.Class != WeightClass.Heavy)
        {
            trajectoryLine.enabled = true;
            Vector3 startPos = heldRigidbody.position;
            Vector3 launchDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            Vector3 playerVel = playerMovement != null ? playerMovement.Velocity : Vector3.zero;

            // Include velocity inheritance in prediction
            Vector3 currentVel = launchDir * launchSpeed + playerVel * 0.45f;
            Vector3 gravity = Physics.gravity;

            int count = trajectoryLine.positionCount;
            float step = 0.05f;
            for (int i = 0; i < count; i++)
            {
                float time = i * step;
                Vector3 point = startPos + currentVel * time + 0.5f * gravity * (time * time);
                trajectoryLine.SetPosition(i, point);
            }
        }
        else
        {
            trajectoryLine.enabled = false;
        }
    }

    private void UpdateHandVibration()
    {
        if (heldRigidbody != null)
        {
            Vector3 jitter = new Vector3(
                (Random.value * 2f - 1f) * 0.005f,
                (Random.value * 2f - 1f) * 0.005f,
                (Random.value * 2f - 1f) * 0.005f
            );
            transform.localPosition = initialHandLocalPos + jitter;
        }
        else if (isGrappling)
        {
            Vector3 tensionJitter = (Random.insideUnitSphere) * 0.008f;
            transform.localPosition = initialHandLocalPos + tensionJitter;
        }
        else
        {
            transform.localPosition = initialHandLocalPos;
        }
    }

    private Vector3 GetHandPosition()
    {
        return handTetherOrigin != null ? handTetherOrigin.position : transform.position;
    }
}
