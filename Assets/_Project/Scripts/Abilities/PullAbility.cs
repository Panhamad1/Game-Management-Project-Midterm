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
            if (playerMovement == null || playerMovement.GroundedObject != propHit.transform)
            {
                IKineticTarget target = propHit.collider.GetComponentInParent<IKineticTarget>();
                if (target == null || target.CanBePulled)
                {
                    Rigidbody targetRb = propHit.collider.attachedRigidbody;
                    if (targetRb != null && !targetRb.isKinematic)
                    {
                        heldRigidbody = targetRb;
                        heldProp = propHit.collider.GetComponentInParent<InteractableProp>();
                        if (heldProp != null) heldProp.SetCarriedState(true);
                        heldRigidbody.angularDamping = 6.0f;
                        isGrappling = false;
                        if (grappleCable != null) grappleCable.enabled = false;
                        return;
                    }
                }
            }
        }

        // Priority 2: Elastic Grapple Slingshot to static geometry or anchors (Redliner style)
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

        Rigidbody launchedRb = heldRigidbody;
        InteractableProp prop = heldProp;

        ReleaseHeldObject();

        Vector3 launchDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        launchedRb.linearVelocity = launchDir * launchSpeed;

        // Apply recoil to player
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
        Vector3 hoverTarget = camPos + camForward * hoverDistance;

        // Heavy crate ground drag rule
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

        // 2. Weight-Class Responsive Spring Reel Dynamics:
        // Light (~1kg): rapid 32 m/s snap (instant mid-air ammo snatch)
        // Medium (~5kg): agile 24 m/s reel
        // Heavy (~25kg): 12 m/s ground drag (heavier resistance)
        Vector3 toTarget = hoverTarget - heldRigidbody.position;
        float distance = toTarget.magnitude;

        float classReelSpeed = maxReelSpeed;
        float pullAccel = 50.0f;

        if (heldProp != null)
        {
            switch (heldProp.Class)
            {
                case WeightClass.Light:
                    classReelSpeed = 32.0f;
                    pullAccel = 65.0f;
                    break;
                case WeightClass.Medium:
                    classReelSpeed = 24.0f;
                    pullAccel = 50.0f;
                    break;
                case WeightClass.Heavy:
                    classReelSpeed = 12.0f;
                    pullAccel = 25.0f;
                    break;
            }
        }

        if (distance > 0.1f)
        {
            float speed = Mathf.Min(distance * 12.0f, classReelSpeed);
            heldRigidbody.linearVelocity = Vector3.MoveTowards(heldRigidbody.linearVelocity, toTarget.normalized * speed, pullAccel * Time.fixedDeltaTime);
        }
        else
        {
            heldRigidbody.linearVelocity = Vector3.zero;
        }

        // 3. Auto-align rotation to face forward with player camera (Riot Shielding)
        Quaternion targetRotation = playerCamera != null ? playerCamera.transform.rotation : transform.rotation;
        heldRigidbody.MoveRotation(Quaternion.Slerp(heldRigidbody.rotation, targetRotation, 14f * Time.fixedDeltaTime));
    }

    private void ReleaseHeldObject()
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.angularDamping = 0.05f;
            if (heldProp != null) heldProp.SetCarriedState(false);
            heldRigidbody = null;
            heldProp = null;
        }

        if (vortexRings != null) vortexRings.enabled = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;
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

        if (heldRigidbody != null)
        {
            trajectoryLine.enabled = true;
            Vector3 startPos = heldRigidbody.position;
            Vector3 launchDir = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            Vector3 currentVel = launchDir * launchSpeed;
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
