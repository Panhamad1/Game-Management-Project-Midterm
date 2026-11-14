using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public sealed class SwarmerEnemy : MonoBehaviour, IKineticTarget, IKineticCarryTarget
{
    [SerializeField] private Transform player;
    [SerializeField, Min(1f)] private float maxHealth = 20f;
    [SerializeField, Min(0f)] private float launchDamage = 10f;
    [SerializeField, Min(0f)] private float pushDamage = 10f;
    [SerializeField, Min(0f)] private float detectionRange = 25f;
    [SerializeField, Min(0f)] private float flightSpeed = 10f;
    [SerializeField, Min(0f)] private float hoverOffset = 1f;
    [SerializeField, Min(0f)] private float roamSpeed = 2f;
    [SerializeField, Min(0f)] private float roamRadius = 5f;
    [SerializeField, Min(0f)] private float attackRange = 5f;
    [SerializeField, Min(0f)] private float windupTime = 0.35f;
    [SerializeField, Min(0f)] private float leapSpeed = 11f;
    [SerializeField, Min(0f)] private float attackCooldown = 1f;
    [SerializeField, Min(0f)] private float damage = 15f;
    [SerializeField, Min(0f)] private float ramSpeedThreshold = 7.5f;
    [SerializeField] private bool canBePulled = true;
    [SerializeField] private bool canBePushed = true;

    private Rigidbody body;
    private SphereCollider bodyCollider;
    private Renderer visual;
    private MaterialPropertyBlock appearance;
    private int environmentMask;
    private float health;
    private float launchAt;
    private float resumeAt;
    private float projectileExpiresAt;
    private float leapEndsAt;
    private float nextRoamChoice;
    private Vector3 home;
    private Vector3 roamGoal;
    private bool windingUp;
    private bool leaping;
    private bool attackHit;
    private bool carried;
    private bool launched;
    private bool dead;

    public bool CanBePulled => canBePulled && !dead && !launched;
    public bool CanBePushed => canBePushed && !dead;
    public event Action<SwarmerEnemy> Defeated;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        foreach (SphereCollider sphere in GetComponents<SphereCollider>())
        {
            if (!sphere.isTrigger) bodyCollider = sphere;
        }
        visual = GetComponentInChildren<Renderer>();
        appearance = new MaterialPropertyBlock();
        environmentMask = LayerMask.GetMask("Environment", "Default");
        health = maxHealth;
        float clearance = hoverOffset;
        if (Physics.SphereCast(body.position, bodyCollider.bounds.extents.x, Vector3.up,
                out RaycastHit ceiling, hoverOffset, environmentMask, QueryTriggerInteraction.Ignore))
        {
            clearance = Mathf.Max(0f, ceiling.distance - 0.05f);
        }
        home = body.position + Vector3.up * clearance;
        roamGoal = home;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetWarning(false);
    }

    private void FixedUpdate()
    {
        if (dead || carried) return;
        if (launched)
        {
            if (Time.time >= projectileExpiresAt) EndLaunch();
            return;
        }

        if (leaping && Time.time >= leapEndsAt)
        {
            body.linearVelocity = Vector3.zero;
            Interrupt();
        }
        if (leaping || Time.time < resumeAt) return;

        Vector3 offset = player != null ? player.position - body.position : Vector3.zero;
        float distance = offset.magnitude;
        bool visible = player != null && distance <= detectionRange &&
            !Physics.Linecast(body.position, player.position, environmentMask, QueryTriggerInteraction.Ignore);
        if (windingUp)
        {
            if (!visible || distance > attackRange)
            {
                SetWarning(false);
            }
            else if (Time.time >= launchAt)
            {
                SetWarning(false);
                leaping = true;
                attackHit = false;
                leapEndsAt = Time.time + 0.9f;
                body.linearVelocity = offset.normalized * leapSpeed;
            }
            return;
        }

        if (visible && distance <= attackRange)
        {
            SetWarning(true);
            launchAt = Time.time + windupTime;
            body.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 desired = visible ? offset.normalized * flightSpeed : Roam();
        body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, desired, 30f * Time.fixedDeltaTime);
    }

    private Vector3 Roam()
    {
        if (Time.time >= nextRoamChoice &&
            (nextRoamChoice > 0f || (body.position - home).sqrMagnitude < 0.09f))
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * roamRadius;
            Vector3 candidate = home + new Vector3(offset.x, UnityEngine.Random.Range(-0.25f, 0.25f), offset.y);
            nextRoamChoice = Time.time + UnityEngine.Random.Range(1.5f, 3f);
            Vector3 path = candidate - body.position;
            if (!Physics.SphereCast(body.position, bodyCollider.bounds.extents.x, path.normalized, out _,
                    path.magnitude, environmentMask, QueryTriggerInteraction.Ignore))
            {
                roamGoal = candidate;
            }
        }

        return Vector3.ClampMagnitude((roamGoal - body.position) * 3f, roamSpeed);
    }

    private void SetWarning(bool warning)
    {
        windingUp = warning;
        if (visual == null) return;
        Color color = warning ? Color.yellow : new Color(0.75f, 0.19f, 0.1f);
        appearance.SetColor("_BaseColor", color);
        appearance.SetColor("_Color", color);
        visual.SetPropertyBlock(appearance);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckSlideRam(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckSlideRam(other);
    }

    private void CheckSlideRam(Collider other)
    {
        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement != null && movement.IsSliding && movement.CurrentSpeed >= ramSpeedThreshold)
        {
            OnPlayerSlideRam(movement.CurrentSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (dead || carried) return;
        if (launched)
        {
            IKineticTarget target = collision.collider.GetComponentInParent<IKineticTarget>();
            if (target != null && collision.relativeVelocity.magnitude >= 4f)
            {
                target.OnPhysicsCrush(collision.relativeVelocity.magnitude, gameObject);
            }
            EndLaunch();
            return;
        }

        PlayerMovement movement = collision.collider.GetComponentInParent<PlayerMovement>();
        if (movement != null && movement.IsSliding &&
            Vector3.Dot(collision.relativeVelocity, movement.SlideDirection) >= ramSpeedThreshold)
        {
            OnPlayerSlideRam(ramSpeedThreshold);
            return;
        }

        if (!leaping || attackHit || movement == null) return;
        attackHit = true;
        movement.GetComponent<SwarmerTestHealth>()?.TakeDamage(damage);
    }

    public void OnPushed(Vector3 direction, float force, Vector3 point)
    {
        if (!CanBePushed) return;
        if (TakeDamage(pushDamage)) return;
        Interrupt();
        body.AddForceAtPosition(direction * force, point, ForceMode.Impulse);
    }

    public void OnPulled(Transform origin, float force)
    {
        if (!CanBePulled) return;
        Interrupt();
        body.AddForce((origin.position - body.position).normalized * force, ForceMode.Acceleration);
    }

    public bool TryBeginCarry()
    {
        if (!CanBePulled || carried) return false;
        carried = true;
        Interrupt();
        return true;
    }

    public void EndCarry()
    {
        carried = false;
        Interrupt();
    }

    public void OnLaunched()
    {
        launched = true;
        body.useGravity = true;
        projectileExpiresAt = Time.time + 5f;
    }

    private void EndLaunch()
    {
        if (TakeDamage(launchDamage)) return;
        launched = false;
        body.useGravity = false;
        body.linearVelocity = Vector3.zero;
        Interrupt();
    }

    public void OnPhysicsCrush(float speed, GameObject prop)
    {
        if (dead || speed < 4f) return;
        if (!TakeDamage(speed)) Interrupt();
    }

    private bool TakeDamage(float amount)
    {
        health -= amount;
        if (health > 0f) return false;
        Defeat();
        return true;
    }

    public void OnPlayerSlideRam(float speed)
    {
        if (!carried && !launched && speed >= ramSpeedThreshold) Defeat();
    }

    private void Interrupt()
    {
        SetWarning(false);
        leaping = false;
        resumeAt = Time.time + attackCooldown;
    }

    private void Defeat()
    {
        if (dead) return;
        dead = true;
        foreach (Collider hitbox in GetComponents<Collider>()) hitbox.enabled = false;
        Defeated?.Invoke(this);
        Destroy(gameObject);
    }
}
