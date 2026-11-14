using UnityEngine;

public sealed class EnemyProjectile : MonoBehaviour, IKineticTarget
{
    private Transform shooter;
    private float speed;
    private float damage;
    private float expiresAt;
    private int hitMask;
    private SniperEnemy sniper;
    private SlowMotionAbility focus;
    private bool reflected;

    public bool CanBePulled => false;
    public bool CanBePushed => enabled && !reflected && sniper != null && focus != null && focus.IsActive;

    public void EnableReflection(SniperEnemy source, SlowMotionAbility playerFocus)
    {
        sniper = source;
        focus = playerFocus;
    }

    public void OnPushed(Vector3 direction, float force, Vector3 point)
    {
        if (!CanBePushed) return;
        reflected = true;
        transform.rotation = Quaternion.LookRotation(sniper.ReturnTarget - transform.position);
        hitMask &= ~LayerMask.GetMask("Player");
        expiresAt = Time.time + 4f;
    }

    public void OnPulled(Transform origin, float force) { }
    public void OnPhysicsCrush(float impactSpeed, GameObject prop) { }
    public void OnPlayerSlideRam(float slideSpeed) { }

    public void Launch(Transform owner, float boltSpeed, float boltDamage, int collisionMask)
    {
        shooter = owner;
        speed = boltSpeed;
        damage = boltDamage;
        hitMask = collisionMask;
        expiresAt = Time.time + 4f;
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
            }
            else
            {
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
