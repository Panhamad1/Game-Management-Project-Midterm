using UnityEngine;

public sealed class EnemyProjectile : MonoBehaviour, IKineticTarget, IDeflectableProjectile
{
    private Transform shooter;
    private float speed;
    private float damage;
    private float expiresAt;
    private int hitMask;
    private SniperEnemy sniper;
    private SlowMotionAbility focus;
    private bool reflected;

    private void Awake()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    public bool CanBePulled => false;
    public bool CanBePushed => CanBeDeflected;

    // IDeflectableProjectile Implementation
    public bool CanBeDeflected => enabled && !reflected;

    public bool TryDeflect(Vector3 newDirection, GameObject newOwner)
    {
        if (!CanBeDeflected) return false;

        reflected = true;

        if (sniper != null)
        {
            // If fired by Sniper, reflect directly back at the sniper's head
            transform.rotation = Quaternion.LookRotation(sniper.ReturnTarget - transform.position);
        }
        else
        {
            // Otherwise (e.g. Sentry bolt), deflect along the player's aim direction
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        speed = Mathf.Max(speed * 1.6f, 32.0f);
        damage = Mathf.Max(damage * 2.0f, 45.0f);

        hitMask &= ~LayerMask.GetMask("Player");
        // Ensure enemies can be hit by reflected projectile
        hitMask |= LayerMask.GetMask("Enemy", "Default", "Interactable");
        expiresAt = Time.time + 4f;

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.startColor = new Color(0.15f, 0.95f, 1.0f, 0.95f);
            trail.endColor = new Color(0.1f, 0.5f, 1.0f, 0f);
        }

        return true;
    }

    public void EnableReflection(SniperEnemy source, SlowMotionAbility playerFocus)
    {
        sniper = source;
        focus = playerFocus;
    }

    public void OnPushed(Vector3 direction, float force, Vector3 point)
    {
        TryDeflect(direction, null);
    }

    public void OnPulled(Transform origin, float force) { }
    public void OnPhysicsCrush(float impactSpeed, GameObject prop) { }
    public void OnPlayerSlideRam(float slideSpeed) { }

    public void Launch(Transform owner, float boltSpeed, float boltDamage, int collisionMask)
    {
        shooter = owner;
        speed = Mathf.Min(boltSpeed, 22.0f); // Calibrated to match player parkour speed
        damage = boltDamage;
        hitMask = collisionMask;
        expiresAt = Time.time + 4f;

        // Ensure dynamic bright plasma trail for visual clarity
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.16f;
            trail.endWidth = 0.02f;
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (unlit != null) trail.material = new Material(unlit);
            trail.startColor = new Color(1.0f, 0.45f, 0.1f, 0.95f);
            trail.endColor = new Color(1.0f, 0.1f, 0.1f, 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector3 direction = transform.forward;
        float distance = speed * Time.fixedDeltaTime;
        // Sweep the full step so fast bolts cannot tunnel through a crate or the player.
        RaycastHit closest = default;
        float closestDistance = float.PositiveInfinity;
        foreach (RaycastHit hit in Physics.RaycastAll(transform.position, direction, distance,
                     hitMask, QueryTriggerInteraction.Ignore))
        {
            if (!reflected && shooter != null && hit.collider.transform.IsChildOf(shooter)) continue;
            if (hit.distance >= closestDistance) continue;
            closest = hit;
            closestDistance = hit.distance;
        }

        if (closest.collider != null)
        {
            enabled = false;
            if (reflected)
            {
                if (sniper != null && closest.collider.transform.IsChildOf(sniper.transform))
                {
                    sniper.OnReturnedShot();
                }
                else
                {
                    // Reflected bolt damages enemies or health receivers
                    closest.collider.GetComponentInParent<IKineticTarget>()?.OnPhysicsCrush(speed, gameObject);
                    closest.collider.GetComponentInParent<IDamageReceiver>()?.TakeDamage(damage, "Deflected_Projectile", GetInstanceID());
                    closest.collider.GetComponentInParent<SwarmerTestHealth>()?.TakeDamage(damage);
                }
            }
            else
            {
                // Damages player or targets
                closest.collider.GetComponentInParent<IDamageReceiver>()?.TakeDamage(damage, "Enemy_Projectile", GetInstanceID());
                closest.collider.GetComponentInParent<SwarmerTestHealth>()?.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (Time.time >= expiresAt)
        {
            enabled = false;
            Destroy(gameObject);
        }
        else
        {
            transform.position += direction * distance;
        }
    }
}
