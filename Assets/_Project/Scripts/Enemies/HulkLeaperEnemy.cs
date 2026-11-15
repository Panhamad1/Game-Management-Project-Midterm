using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public sealed class HulkLeaperEnemy : MonoBehaviour, IKineticTarget
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Renderer armor;
    [SerializeField] private GameObject weakPoint;
    [SerializeField] private HulkShockwave shockwave;
    [SerializeField, Min(1f)] private float maxHealth = 90f;
    [SerializeField, Min(0f)] private float walkSpeed = 3f;
    [SerializeField, Min(0f)] private float detectionRange = 24f;
    [SerializeField, Min(0f)] private float slamRange = 12f;
    [SerializeField, Min(0f)] private float slamCooldown = 6f;
    [SerializeField, Min(0f)] private float windupTime = 0.8f;
    [SerializeField, Min(0.1f)] private float jumpHeight = 4f;
    [SerializeField, Min(0f)] private float stunDuration = 2.5f;
    [SerializeField, Min(0f)] private float meleeRange = 2.3f;
    [SerializeField, Min(0f)] private float meleeDamage = 20f;
    [SerializeField, Min(0f)] private float meleeKnockbackSpeed = 7f;
    [SerializeField, Min(0f)] private float meleeLiftSpeed = 3f;
    [SerializeField, Min(0f)] private float meleeCooldown = 1.35f;
    [SerializeField, Min(0f)] private float weakPointMultiplier = 3f;

    private enum State { Chase, Windup, Airborne, Stunned }
    private State state;
    private Rigidbody body;
    private MaterialPropertyBlock appearance;
    private int floorMask;
    private float health;
    private float stateUntil;
    private float landingDeadline;
    private float nextSlam;
    private float nextMelee;
    private Vector3 landingTarget;
    private EnemyVisualPart[] torso;
    private EnemyVisualPart leftArm;
    private EnemyVisualPart rightArm;
    private EnemyVisualPart leftLeg;
    private EnemyVisualPart rightLeg;
    private float gait;
    private float gaitWeight;
    private float brace;
    private float lift;
    private float meleeSwingAge = 1f;
    private Renderer coreRenderer;
    private MaterialPropertyBlock coreAppearance;

    public bool CanBePulled => false;
    public bool CanBePushed => false;
    public event Action<HulkLeaperEnemy> Defeated;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        health = maxHealth;
        floorMask = LayerMask.GetMask("Environment", "Default");
        appearance = new MaterialPropertyBlock();
        if (weakPoint != null)
        {
            coreRenderer = weakPoint.GetComponent<Renderer>();
            coreAppearance = new MaterialPropertyBlock();
        }
        torso = new[]
        {
            new EnemyVisualPart(transform, "Armor"), new EnemyVisualPart(transform, "Sandstone collar"),
            new EnemyVisualPart(transform, "Collar inlay"), new EnemyVisualPart(transform, "Royal crown"),
            new EnemyVisualPart(transform, "Crown band"), new EnemyVisualPart(transform, "Visor"),
            new EnemyVisualPart(transform, "Chest seal"), new EnemyVisualPart(transform, "Belt"),
            new EnemyVisualPart(transform, "Shoulder L"), new EnemyVisualPart(transform, "Shoulder R")
        };
        leftArm = new EnemyVisualPart(transform, "Gauntlet L");
        rightArm = new EnemyVisualPart(transform, "Gauntlet R");
        leftLeg = new EnemyVisualPart(transform, "Greave L");
        rightLeg = new EnemyVisualPart(transform, "Greave R");
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (weakPoint != null) weakPoint.SetActive(false);
        SetArmorColor(new Color(0.24f, 0.28f, 0.33f));
    }

    private void LateUpdate()
    {
        if (state == State.Stunned && coreRenderer != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 9f);
            coreAppearance.SetColor("_BaseColor", Color.Lerp(new Color(1f, 0.24f, 0.035f),
                new Color(2f, 1.15f, 0.34f), pulse));
            coreRenderer.SetPropertyBlock(coreAppearance);
        }

        float speed = state == State.Chase ? Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude : 0f;
        float blend = 1f - Mathf.Exp(-12f * Time.deltaTime);
        gait += speed * 4f * Time.deltaTime;
        gaitWeight = Mathf.Lerp(gaitWeight, Mathf.Clamp01(speed / Mathf.Max(walkSpeed, 0.01f)), blend);
        float braceTarget = state == State.Windup ? 1f : (state == State.Stunned ? 0.6f : 0f);
        brace = Mathf.Lerp(brace, braceTarget, blend);
        lift = Mathf.Lerp(lift, state == State.Airborne ? 1f : 0f, blend);
        float stride = Mathf.Sin(gait) * gaitWeight;
        Vector3 upperOffset = Vector3.up * (Mathf.Abs(stride) * 0.075f - brace * 0.24f + lift * 0.08f);
        foreach (EnemyVisualPart part in torso) part.Pose(upperOffset, Quaternion.identity);

        meleeSwingAge += Time.deltaTime;
        float swing = meleeSwingAge < 0.09f
            ? Mathf.SmoothStep(0f, 1f, meleeSwingAge / 0.09f)
            : 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.53f, meleeSwingAge));
        float armRaise = Mathf.Lerp(-55f * lift - 24f * brace, -145f, swing);
        float armStride = stride * 25f * (1f - swing);
        leftArm.Swing(upperOffset, new Vector3(-0.76f, 2.08f, 0.04f), armRaise + armStride);
        rightArm.Swing(upperOffset, new Vector3(0.76f, 2.08f, 0.04f), armRaise - armStride);
        leftLeg.Pose(new Vector3(0f, Mathf.Max(0f, stride) * 0.15f + lift * 0.17f, stride * 0.23f), Quaternion.Euler(-stride * 20f, 0f, 0f));
        rightLeg.Pose(new Vector3(0f, Mathf.Max(0f, -stride) * 0.15f + lift * 0.17f, -stride * 0.23f), Quaternion.Euler(stride * 20f, 0f, 0f));
    }

    private void FixedUpdate()
    {
        if (health <= 0f) return;
        switch (state)
        {
            case State.Windup:
                if (Time.time >= stateUntil) LaunchSlam();
                break;
            case State.Airborne:
                if (Time.time >= stateUntil && body.linearVelocity.y <= 0.1f &&
                    Physics.Raycast(body.position + Vector3.up * 0.35f, Vector3.down,
                        out RaycastHit floor, 0.53f, floorMask, QueryTriggerInteraction.Ignore))
                {
                    BeginRecovery(floor.point, true);
                }
                else if (Time.time >= landingDeadline)
                {
                    BeginRecovery(body.position, false);
                }
                break;
            case State.Stunned:
                if (Time.time >= stateUntil)
                {
                    state = State.Chase;
                    if (weakPoint != null) weakPoint.SetActive(false);
                    SetArmorColor(new Color(0.24f, 0.28f, 0.33f));
                }
                break;
            default:
                Chase();
                break;
        }
    }

    private void Chase()
    {
        Vector3 offset = player != null ? player.transform.position - body.position : Vector3.zero;
        bool visible = player != null && offset.sqrMagnitude <= detectionRange * detectionRange &&
            !Physics.Linecast(body.position + Vector3.up, player.transform.position,
                floorMask, QueryTriggerInteraction.Ignore);
        Vector3 flat = Vector3.ProjectOnPlane(offset, Vector3.up);
        if (visible && flat.sqrMagnitude > 0.01f)
        {
            body.MoveRotation(Quaternion.LookRotation(flat));
        }

        if (visible && offset.magnitude <= slamRange && Time.time >= nextSlam)
        {
            Vector3 prediction = offset + player.Rb.linearVelocity * 0.3f;
            landingTarget = body.position + Vector3.ClampMagnitude(Vector3.ProjectOnPlane(prediction, Vector3.up), slamRange);
            state = State.Windup;
            stateUntil = Time.time + windupTime;
            StopHorizontal();
            SetArmorColor(new Color(1f, 0.55f, 0.06f));
            return;
        }

        Vector3 velocity = visible && flat.magnitude > meleeRange ? flat.normalized * walkSpeed : Vector3.zero;
        body.linearVelocity = velocity + Vector3.up * body.linearVelocity.y;
        if (visible && offset.magnitude <= meleeRange && Time.time >= nextMelee)
        {
            nextMelee = Time.time + meleeCooldown;
            meleeSwingAge = 0f;
            Vector3 outward = flat.sqrMagnitude > 0.001f ? flat.normalized : transform.forward;
            player.ApplyEnemyKnockback(outward * meleeKnockbackSpeed + Vector3.up * meleeLiftSpeed);
            player.GetComponent<SwarmerTestHealth>()?.TakeDamage(meleeDamage);
        }
    }

    private void LaunchSlam()
    {
        float gravity = Mathf.Max(0.01f, -Physics.gravity.y);
        float verticalSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);
        float flightTime = 2f * verticalSpeed / gravity;
        Vector3 horizontal = Vector3.ProjectOnPlane(landingTarget - body.position, Vector3.up);
        body.linearVelocity = horizontal / flightTime + Vector3.up * verticalSpeed;
        state = State.Airborne;
        stateUntil = Time.time + 0.15f;
        landingDeadline = Time.time + flightTime + 1f;
    }

    private void OnCollisionEnter(Collision collision) => CheckContact(collision);

    private void OnCollisionStay(Collision collision)
    {
        if (state == State.Airborne) CheckContact(collision);
    }

    private void CheckContact(Collision collision)
    {
        if (state == State.Airborne && Time.time >= stateUntil)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < 0.5f) continue;
                BeginRecovery(contact.point, true);
                return;
            }
        }
        else if (state == State.Stunned && collision.rigidbody != null &&
            !collision.rigidbody.isKinematic &&
            !collision.rigidbody.TryGetComponent<PlayerMovement>(out _) &&
            !collision.rigidbody.TryGetComponent<IKineticTarget>(out _))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal)) >= 4f)
                {
                    DamageDuringRecovery(impactSpeed, 1f);
                    break;
                }
            }
        }
    }

    private void BeginRecovery(Vector3 point, bool landed)
    {
        StopHorizontal();
        state = State.Stunned;
        stateUntil = Time.time + stunDuration;
        nextSlam = Time.time + slamCooldown;
        if (weakPoint != null) weakPoint.SetActive(true);
        SetArmorColor(new Color(0.16f, 0.19f, 0.23f));
        if (landed && shockwave != null) shockwave.Emit(point, player);
    }

    private void StopHorizontal()
    {
        body.linearVelocity = Vector3.up * body.linearVelocity.y;
    }

    private void SetArmorColor(Color color)
    {
        if (armor == null) return;
        appearance.SetColor("_BaseColor", color);
        appearance.SetColor("_Color", color);
        armor.SetPropertyBlock(appearance);
    }

    public void OnPushed(Vector3 direction, float force, Vector3 point) { }
    public void OnPulled(Transform origin, float force) { }
    public void OnPhysicsCrush(float speed, GameObject prop) => TakeItemHit(speed, prop, 1f);
    public void OnPlayerSlideRam(float speed) { }

    public void HitExposedCore(float speed, GameObject prop) => TakeItemHit(speed, prop, weakPointMultiplier);

    private void TakeItemHit(float speed, GameObject prop, float multiplier)
    {
        if (prop == null || (prop.TryGetComponent<InteractableProp>(out var crate) && crate.IsCarried)) return;
        DamageDuringRecovery(speed, multiplier);
    }

    private void DamageDuringRecovery(float speed, float multiplier)
    {
        if (health <= 0f || state != State.Stunned || speed < 4f) return;
        health -= speed * multiplier;
        if (health > 0f) return;
        Defeated?.Invoke(this);
        Destroy(gameObject);
    }
}
