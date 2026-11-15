using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public sealed class SwarmerEnemy : MonoBehaviour, IKineticTarget, IKineticCarryTarget, IDamageReceiver
{
    [SerializeField] private Transform player;
    [SerializeField, Min(1f)] private float maxHealth = 20f;
    [SerializeField, Min(0f)] private float launchDamage = 10f;
    [SerializeField, Min(0f)] private float pushDamage = 10f;
    [SerializeField, Min(0f)] private float detectionRange = 25f;
    [SerializeField, Min(0f)] private float flightSpeed = 4.2f;
    [SerializeField, Min(0f)] private float hoverOffset = 1f;
    [SerializeField, Min(0f)] private float roamSpeed = 1.4f;
    [SerializeField, Min(0f)] private float roamRadius = 5f;
    [SerializeField, Min(0f)] private float attackRange = 5f;
    [SerializeField, Min(0f)] private float windupTime = 0.5f;
    [SerializeField, Min(0f)] private float leapSpeed = 5.8f;
    [SerializeField, Min(0f)] private float attackCooldown = 1.2f;
    [SerializeField, Min(0f)] private float damage = 15f;
    [SerializeField, Min(0f)] private float ramSpeedThreshold = 5.0f;
    [SerializeField] private bool canBePulled = true;
    [SerializeField] private bool canBePushed = true;

    private Rigidbody body;
    private SphereCollider bodyCollider;
    private Renderer visual;
    private Renderer warningCrest;
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
    private EnemyVisualPart[] shellParts;
    private EnemyVisualPart leftWing;
    private EnemyVisualPart rightWing;
    private Quaternion visualFacing = Quaternion.identity;
    private float visualYaw;
    private float wingAmplitude;
    private float hoverAmplitude;
    private Vector3 visualTravel;

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
        warningCrest = transform.Find("Crown diamond")?.GetComponent<Renderer>();
        shellParts = new[]
        {
            new EnemyVisualPart(transform, "Split carapace L"),
            new EnemyVisualPart(transform, "Split carapace R"),
            new EnemyVisualPart(transform, "Crown diamond"),
            new EnemyVisualPart(transform, "Mandible L"),
            new EnemyVisualPart(transform, "Mandible R")
        };
        leftWing = new EnemyVisualPart(transform, "Wing blade L");
        rightWing = new EnemyVisualPart(transform, "Wing blade R");
        appearance = new MaterialPropertyBlock();
        environmentMask = LayerMask.GetMask("Environment", "Default");
        health = maxHealth;
        if (GetComponent<EnemyHitFlash>() == null) gameObject.AddComponent<EnemyHitFlash>();
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

    private void LateUpdate()
    {
        float blend = 1f - Mathf.Exp(-9f * Time.deltaTime);
        bool freeFlight = !carried && !launched;
        Vector3 travel = freeFlight ? transform.InverseTransformDirection(body.linearVelocity) : Vector3.zero;
        visualTravel = Vector3.Lerp(visualTravel, travel, blend);
        Vector3 horizontal = Vector3.ProjectOnPlane(visualTravel, Vector3.up);
        float yaw = visualYaw;
        if (!freeFlight)
        {
            yaw = 0f;
        }
        else if (windingUp && player != null)
        {
            Vector3 aim = transform.InverseTransformDirection(player.position - transform.position);
            if (aim.x * aim.x + aim.z * aim.z > 0.25f) yaw = Mathf.Atan2(aim.x, aim.z) * Mathf.Rad2Deg;
        }
        else if (horizontal.sqrMagnitude > 1f)
        {
            yaw = Mathf.Atan2(horizontal.x, horizontal.z) * Mathf.Rad2Deg;
        }
        float turn = Mathf.DeltaAngle(visualYaw, yaw);
        visualYaw = Mathf.MoveTowardsAngle(visualYaw, yaw, 280f * Time.deltaTime);
        float pitch = leaping ? 20f : (windingUp ? -12f : Mathf.Clamp(-visualTravel.y * 2f, -16f, 16f));
        float bank = Mathf.Clamp(-turn * 0.22f, -15f, 15f);
        Quaternion target = freeFlight ? Quaternion.Euler(pitch, visualYaw, bank) : Quaternion.Euler(0f, visualYaw, 0f);
        visualFacing = Quaternion.Slerp(visualFacing, target, blend);

        hoverAmplitude = Mathf.Lerp(hoverAmplitude, freeFlight ? 0.035f : 0f, blend);
        float hover = Mathf.Sin(Time.time * 6f) * hoverAmplitude;
        Vector3 offset = Vector3.up * hover;
        foreach (EnemyVisualPart shell in shellParts)
        {
            shell.Orbit(visualFacing, offset, Quaternion.identity);
        }

        wingAmplitude = Mathf.Lerp(wingAmplitude, freeFlight ? (windingUp ? 19f : 13f) : 0f, blend);
        float flap = Mathf.Sin(Time.time * 19f) * wingAmplitude;
        leftWing.Orbit(visualFacing, offset, Quaternion.Euler(0f, 0f, flap));
        rightWing.Orbit(visualFacing, offset, Quaternion.Euler(0f, 0f, -flap));
    }

    private void FixedUpdate()
    {
        if (dead || carried) return;
        if (launched)
        {
            if (Time.time >= projectileExpiresAt) EndLaunch();
            return;
        }

        if (leaping && Time.time >= leapEndsAt) EndLunge();
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
                // Stop near the aim point instead of flying past it at full speed.
                leapEndsAt = Time.time + Mathf.Min(0.9f, (distance + 0.65f) / Mathf.Max(leapSpeed, 0.1f));
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
        Color color = warning ? new Color(1f, 0.055f, 0.035f) : new Color(0.6f, 0.4f, 0.22f);
        appearance.SetColor("_BaseColor", color);
        appearance.SetColor("_Color", color);
        if (visual != null) visual.SetPropertyBlock(appearance);
        if (warningCrest != null) warningCrest.SetPropertyBlock(appearance);
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
            // 65% chance to detonate into a high-yield plasma explosion!
            bool triggersExplosion = UnityEngine.Random.value < 0.65f;
            Vector3 impactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;

            if (triggersExplosion)
            {
                DetonatePlasmaExplosion(impactPoint);
            }
            else
            {
                IKineticTarget target = collision.collider.GetComponentInParent<IKineticTarget>();
                if (target != null && collision.relativeVelocity.magnitude >= 4f)
                {
                    target.OnPhysicsCrush(collision.relativeVelocity.magnitude, gameObject);
                }
            }
            EndLaunch();
            TakeDamage(maxHealth * 2f, "LaunchDetonation", 0);
            return;
        }

        PlayerMovement movement = collision.collider.GetComponentInParent<PlayerMovement>();
        if (movement != null && movement.IsSliding &&
            Vector3.Dot(collision.relativeVelocity, movement.SlideDirection) >= ramSpeedThreshold)
        {
            OnPlayerSlideRam(ramSpeedThreshold);
            return;
        }

        if (!leaping || attackHit) return;
        if (movement == null)
        {
            if ((environmentMask & (1 << collision.gameObject.layer)) != 0) EndLunge();
            return;
        }
        attackHit = true;
        EndLunge();
        movement.GetComponent<IDamageReceiver>()?.TakeDamage(damage, "Swarmer", GetInstanceID());
        movement.GetComponent<SwarmerTestHealth>()?.TakeDamage(damage);
    }

    private void EndLunge()
    {
        body.linearVelocity = Vector3.zero;
        Interrupt();
    }

    private void DetonatePlasmaExplosion(Vector3 origin)
    {
        float explosionRadius = 6.5f;
        float explosionDamage = 75.0f;

        Collider[] hits = Physics.OverlapSphere(origin, explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject == gameObject) continue;

            IDamageReceiver receiver = hits[i].GetComponentInParent<IDamageReceiver>();
            if (receiver != null)
            {
                receiver.TakeDamage(explosionDamage, "SwarmerExplosion", GetInstanceID());
            }

            Rigidbody hitRb = hits[i].attachedRigidbody;
            if (hitRb != null && !hitRb.isKinematic)
            {
                hitRb.AddExplosionForce(750f, origin, explosionRadius, 1.2f, ForceMode.Impulse);
            }

            IKineticTarget kinetic = hits[i].GetComponentInParent<IKineticTarget>();
            if (kinetic != null)
            {
                kinetic.OnPhysicsCrush(16f, gameObject);
            }
        }

        FloatingDamageNumber.Spawn(origin + Vector3.up * 1.5f, explosionDamage, true);

        // Screen shake if player is within vicinity
        PlayerMovement pMove = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
        if (pMove != null)
        {
            float dist = Vector3.Distance(origin, pMove.transform.position);
            if (dist < 18.0f)
            {
                var look = pMove.GetComponentInChildren<MouseLook>();
                if (look != null) look.AddScreenShake(Mathf.Lerp(0.35f, 0.05f, dist / 18.0f), Vector3.up);
            }
        }
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

        // Physical knockback fling: send the swarmer flying in the direction of impact!
        Vector3 knockbackDir = prop != null ? (transform.position - prop.transform.position).normalized : -transform.forward;
        knockbackDir.y = Mathf.Max(knockbackDir.y, 0.35f);
        knockbackDir.Normalize();

        if (body != null)
        {
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = knockbackDir * (speed * 1.6f) + Vector3.up * 4.2f;
        }

        if (!TakeDamage(speed * 2.2f)) Interrupt();
    }

    public void TakeDamage(float amount, string source = "Unknown", int attackId = 0)
    {
        TakeDamage(amount);
    }

    private bool TakeDamage(float amount)
    {
        health -= amount;
        FloatingDamageNumber.Spawn(transform.position + Vector3.up * 0.4f, amount, amount >= 30f);
        GetComponent<EnemyHitFlash>()?.Flash();
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
        FindAnyObjectByType<PlayerHealth>()?.HealOnKill("Swarmer", transform.position);
        Defeated?.Invoke(this);
        Destroy(gameObject);
    }
}
