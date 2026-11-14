using System;
using UnityEngine;

/// <summary>
/// Implements the Wind Cannon (Push) ability.
/// Blasts objects with visible compressed air shockwaves and delivers 3D rocket recoil.
/// - Surface proximity check gives 1.5x explosive boost when blasting within 3m of floor/walls.
/// - Procedural hand kickback and directional screen shake.
/// </summary>
public class PushAbility : MonoBehaviour
{
    [Header("Blast Tuning")]
    [Tooltip("Maximum raycast distance to push an object.")]
    [SerializeField] private float maxRange = 12.0f;

    [Tooltip("Impulse force delivered to target crates and enemies.")]
    [SerializeField] private float targetPushImpulse = 18.0f;

    [Header("Surface Proximity Kick (Newton's Third Law)")]
    [Tooltip("Maximum distance to a solid surface (floor or wall) required to kick/recoil the player.")]
    [SerializeField] private float surfaceProximityDistance = 3.8f;

    [Tooltip("Impulse kick velocity delivered when blasting within proximity of a solid surface.")]
    [SerializeField] private float surfaceKickVelocity = 13.5f;

    [Tooltip("Cooldown in seconds between push blasts.")]
    [SerializeField] private float cooldownTime = 0.45f;

    [Header("Visual Shockwave")]
    [SerializeField] private Transform abilityRayOrigin;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private ParticleSystem blastParticles;
    [SerializeField] private LineRenderer shockwaveRing;

    [Header("Procedural Hand Kickback")]
    [SerializeField] private float handRecoilDistance = 0.18f;
    [SerializeField] private float handRecoverySpeed = 10.0f;

    [Header("System References")]
    [SerializeField] private PullAbility pullAbility;
    [SerializeField] private SlowMotionAbility slowMotionAbility;

    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private MouseLook mouseLook;
    private PlayerAudioEffects audioEffects;

    private float lastPushTime = -10f;
    private Vector3 initialHandLocalPos;
    private float currentHandRecoil = 0f;

    // Shockwave Ring Animation
    private float ringTimer = 1f;
    private Vector3 ringOrigin;
    private Vector3 ringForward;

    // Public API
    public event Action OnPushed;
    public float CooldownPercent => Mathf.Clamp01((Time.time - lastPushTime) / cooldownTime);
    public bool IsOnCooldown => (Time.time - lastPushTime) < cooldownTime;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCamera = GetComponentInParent<Camera>();
        mouseLook = GetComponentInParent<MouseLook>();
        audioEffects = GetComponentInParent<PlayerAudioEffects>();

        if (playerCamera == null) playerCamera = Camera.main;
        if (pullAbility == null) pullAbility = GetComponentInParent<PullAbility>() ?? FindAnyObjectByType<PullAbility>();
        if (slowMotionAbility == null) slowMotionAbility = GetComponentInParent<SlowMotionAbility>() ?? FindAnyObjectByType<SlowMotionAbility>();

        initialHandLocalPos = transform.localPosition;

        if (hitLayers.value == 0)
        {
            hitLayers = ~LayerMask.GetMask("Player", "Ignore Raycast");
        }

        SetupShockwaveRing();
    }

    private void SetupShockwaveRing()
    {
        if (shockwaveRing == null)
        {
            shockwaveRing = gameObject.AddComponent<LineRenderer>();
            shockwaveRing.useWorldSpace = true;
            shockwaveRing.loop = true;
            shockwaveRing.positionCount = 24;
            shockwaveRing.startWidth = 0.08f;
            shockwaveRing.endWidth = 0.08f;

            // Translucent cyan shockwave material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            mat.color = new Color(0.3f, 0.85f, 1f, 0.75f);
            shockwaveRing.material = mat;
            shockwaveRing.enabled = false;
        }
    }

    private void Update()
    {
        UpdateHandRecoil();
        UpdateShockwaveRing();
    }

    /// <summary>
    /// Executes a Wind Blast. Returns true if successfully fired.
    /// </summary>
    public bool TryPush()
    {
        if (IsOnCooldown) return false;

        // Suppress Push if currently carrying a launchable object (Crate Cannon fires instead)
        if (pullAbility != null && pullAbility.IsCarrying && pullAbility.HeldProp != null && pullAbility.HeldProp.CanBeLaunched)
        {
            return false;
        }

        lastPushTime = Time.time;

        Vector3 rayOrigin = abilityRayOrigin != null ? abilityRayOrigin.position : playerCamera.transform.position;
        Vector3 rayDirection = playerCamera != null ? playerCamera.transform.forward : transform.forward;

        // 0. Projectile Deflection: Check for incoming deflectable projectiles in a forward parry cone
        bool deflectedAny = false;
        Collider[] deflectCandidates = Physics.OverlapSphere(rayOrigin + rayDirection * 2.0f, 2.6f, hitLayers, QueryTriggerInteraction.Collide);
        foreach (var col in deflectCandidates)
        {
            IDeflectableProjectile deflectable = col.GetComponentInParent<IDeflectableProjectile>();
            if (deflectable != null && deflectable.CanBeDeflected)
            {
                if (deflectable.TryDeflect(rayDirection, playerMovement != null ? playerMovement.gameObject : gameObject))
                {
                    deflectedAny = true;
                    if (slowMotionAbility != null)
                    {
                        slowMotionAbility.SiphonEnergy(15.0f); // Deflection action reward (+15 energy)
                    }
                }
            }
        }

        bool hitSurface = false;
        float hitDistance = maxRange;

        // 1. Raycast for kinetic targets or walls
        Ray ray = new Ray(rayOrigin, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers, QueryTriggerInteraction.Ignore))
        {
            hitSurface = true;
            hitDistance = hit.distance;

            IKineticTarget target = hit.collider.GetComponentInParent<IKineticTarget>();
            if (target != null && target.CanBePushed)
            {
                // Deliver rich kinetic impact context
                KineticImpactContext ctx = new KineticImpactContext(
                    gameObject,
                    hit.point,
                    hit.normal,
                    rayDirection,
                    targetPushImpulse,
                    0.5f * 5.0f * targetPushImpulse * targetPushImpulse,
                    false
                );
                target.OnKineticImpact(ctx);
                target.OnPushed(rayDirection, targetPushImpulse, hit.point);

                // Action reward: Target switch triggers grant Focus boost
                if (slowMotionAbility != null && hit.collider.GetComponentInParent<TargetButton>() != null)
                {
                    slowMotionAbility.SiphonEnergy(15.0f);
                }
            }
            else
            {
                Rigidbody targetRb = hit.collider.attachedRigidbody;
                if (targetRb != null && !targetRb.isKinematic)
                {
                    targetRb.AddForceAtPosition(rayDirection * targetPushImpulse, hit.point, ForceMode.Impulse);
                }
            }
        }

        // 2. Proximity Kick Calculation
        // Newton's Third Law: Wind blast only kicks the player if blasting off a solid surface within 3.8m,
        // OR if the player is currently sliding along the ground.
        // Blasting into open empty air delivers ZERO player recoil (prevents infinite flying / hovering).
        Vector3 finalRecoilVelocity = Vector3.zero;
        bool hasSurfaceKick = false;
        bool isSliding = playerMovement != null && playerMovement.IsSliding;

        if (isSliding)
        {
            // Ground is the surface: Blast boosts slide velocity forward
            hasSurfaceKick = true;
            finalRecoilVelocity = transform.forward * surfaceKickVelocity;
        }
        else if (hitSurface && hitDistance <= surfaceProximityDistance)
        {
            hasSurfaceKick = true;
            // Closer proximity delivers a punchier kick
            float distanceRatio = 1.0f - Mathf.Clamp01(hitDistance / surfaceProximityDistance);
            float kickMagnitude = surfaceKickVelocity * (0.8f + distanceRatio * 0.4f);

            Vector3 kickDir = -rayDirection;

            // If blasting a vertical wall, reflect direction to ensure clean wall redirection
            if (hit.normal.y < 0.6f && hit.normal != Vector3.zero)
            {
                kickDir = Vector3.Lerp(kickDir, hit.normal, 0.5f).normalized;
            }

            finalRecoilVelocity = kickDir * kickMagnitude;
        }

        if (playerMovement != null && hasSurfaceKick)
        {
            playerMovement.ApplyRecoil(finalRecoilVelocity);
        }

        // 3. Visual Shockwave Ring & Particles
        TriggerShockwaveRing(rayOrigin + rayDirection * 0.4f, rayDirection);
        if (blastParticles != null) blastParticles.Play();

        // 4. Procedural Hand Recoil & Camera Shake
        currentHandRecoil = handRecoilDistance;
        Vector3 shakeDir = isSliding ? transform.forward : (-rayDirection);
        float shakeIntensity = deflectedAny ? 0.45f : (hasSurfaceKick ? 0.35f : 0.15f);
        if (mouseLook != null)
        {
            mouseLook.AddScreenShake(shakeIntensity, shakeDir);
        }

        // 5. Sound
        if (audioEffects != null)
        {
            audioEffects.PlayWindBlast();
        }

        OnPushed?.Invoke();
        return true;
    }

    private void TriggerShockwaveRing(Vector3 origin, Vector3 forward)
    {
        ringOrigin = origin;
        ringForward = forward;
        ringTimer = 0f;
        if (shockwaveRing != null) shockwaveRing.enabled = true;
    }

    private void UpdateShockwaveRing()
    {
        if (shockwaveRing == null || ringTimer >= 1f) return;

        ringTimer += Time.deltaTime * 5.0f;
        if (ringTimer >= 1f)
        {
            shockwaveRing.enabled = false;
            return;
        }

        float radius = Mathf.Lerp(0.2f, 2.8f, ringTimer);
        shockwaveRing.startWidth = Mathf.Lerp(0.12f, 0.01f, ringTimer);
        shockwaveRing.endWidth = shockwaveRing.startWidth;

        Vector3 right = Vector3.Cross(ringForward, Vector3.up);
        if (right.sqrMagnitude < 0.001f) right = Vector3.Cross(ringForward, Vector3.right);
        right.Normalize();
        Vector3 up = Vector3.Cross(right, ringForward).normalized;

        int count = shockwaveRing.positionCount;
        for (int i = 0; i < count; i++)
        {
            float angle = (float)i / count * Mathf.PI * 2f;
            Vector3 pos = ringOrigin + (ringForward * (ringTimer * 1.5f)) + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
            shockwaveRing.SetPosition(i, pos);
        }
    }

    private void UpdateHandRecoil()
    {
        if (currentHandRecoil > 0f)
        {
            currentHandRecoil = Mathf.MoveTowards(currentHandRecoil, 0f, handRecoverySpeed * Time.deltaTime);
        }
        transform.localPosition = initialHandLocalPos - Vector3.forward * currentHandRecoil;
    }

    /// <summary>
    /// Checks if an incoming deflectable projectile is currently within the player's parry cone.
    /// </summary>
    public bool HasDeflectableTargetInSight(out IDeflectableProjectile projectile)
    {
        projectile = null;
        Vector3 origin = abilityRayOrigin != null ? abilityRayOrigin.position : (playerCamera != null ? playerCamera.transform.position : transform.position);
        Vector3 forward = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        Collider[] hits = Physics.OverlapSphere(origin + forward * 2.2f, 2.6f, hitLayers, QueryTriggerInteraction.Collide);
        foreach (var col in hits)
        {
            IDeflectableProjectile proj = col.GetComponentInParent<IDeflectableProjectile>();
            if (proj != null && proj.CanBeDeflected)
            {
                projectile = proj;
                return true;
            }
        }
        return false;
    }
}
