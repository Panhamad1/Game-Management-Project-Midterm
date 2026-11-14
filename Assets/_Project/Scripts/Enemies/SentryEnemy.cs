using UnityEngine;

public sealed class SentryEnemy : MonoBehaviour, IKineticTarget
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform head;
    [SerializeField] private Transform muzzle;
    [SerializeField] private EnemyProjectile boltPrefab;
    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField, Min(0f)] private float detectionRange = 30f;
    [SerializeField, Min(0f)] private float turnSpeed = 180f;
    [SerializeField, Min(1)] private int shotsPerBurst = 3;
    [SerializeField, Min(0.1f)] private float shotsPerSecond = 4f;
    [SerializeField, Min(0f)] private float burstPause = 1.5f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 25f;
    [SerializeField, Min(0f)] private float projectileDamage = 5f;
    [SerializeField, Min(0f)] private float aimSpread = 3f;
    [SerializeField, Min(0f)] private float pushDamage = 10f;
    [SerializeField, Min(0f)] private float crushDamage = 10f;
    [SerializeField] private LayerMask sightMask;
    [SerializeField] private LayerMask projectileMask;

    private float health;
    private int shotsRemaining;
    private float nextShot;
    private float readyAt;

    public bool CanBePulled => false;
    public bool CanBePushed => health > 0f;

    private void Awake()
    {
        health = maxHealth;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (sightMask.value == 0) sightMask = LayerMask.GetMask("Environment", "Default");
        if (projectileMask.value == 0)
        {
            projectileMask = LayerMask.GetMask("Environment", "Default", "Player", "Interactables", "Enemies");
        }
    }

    private void Update()
    {
        if (health <= 0f || player == null || head == null || muzzle == null || boltPrefab == null) return;

        Vector3 aim = player.position + Vector3.up * 0.55f;
        Vector3 offset = aim - muzzle.position;
        if (offset.sqrMagnitude < 0.0001f) return;
        bool visible = offset.sqrMagnitude <= detectionRange * detectionRange &&
            !Physics.Linecast(muzzle.position, aim, sightMask, QueryTriggerInteraction.Ignore);

        if (!visible)
        {
            if (shotsRemaining > 0) EndBurst();
            return;
        }

        head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.LookRotation(aim - head.position),
            turnSpeed * Time.deltaTime);
        if (shotsRemaining == 0)
        {
            if (Time.time < readyAt || Vector3.Angle(head.forward, offset) > 8f) return;
            shotsRemaining = shotsPerBurst;
            nextShot = Time.time;
        }
        if (Time.time < nextShot) return;

        nextShot = Time.time + 1f / shotsPerSecond;
        float sweep = shotsPerBurst > 1
            ? Mathf.Lerp(-aimSpread, aimSpread, (float)(shotsPerBurst - shotsRemaining) / (shotsPerBurst - 1))
            : 0f;
        Quaternion shotRotation = head.rotation * Quaternion.Euler(0f, sweep, 0f);
        EnemyProjectile bolt = Instantiate(boltPrefab, muzzle.position, shotRotation);
        bolt.Launch(transform, projectileSpeed, projectileDamage, projectileMask);
        shotsRemaining--;
        if (shotsRemaining == 0) EndBurst();
    }

    private void EndBurst()
    {
        shotsRemaining = 0;
        readyAt = Time.time + burstPause;
    }

    public void OnPushed(Vector3 direction, float force, Vector3 point) => TakeDamage(pushDamage);
    public void OnPulled(Transform origin, float force) { }
    public void OnPhysicsCrush(float speed, GameObject prop) => TakeDamage(crushDamage);
    public void OnPlayerSlideRam(float speed) { }

    private void TakeDamage(float amount)
    {
        if (health <= 0f) return;
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }
}
