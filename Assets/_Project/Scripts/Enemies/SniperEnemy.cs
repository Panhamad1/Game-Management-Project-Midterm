using UnityEngine;

public sealed class SniperEnemy : MonoBehaviour, IKineticTarget, IDamageReceiver, IEnemyHealthStatus
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer laser;
    [SerializeField] private EnemyProjectile roundPrefab;
    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField, Min(0f)] private float detectionRange = 55f;
    [SerializeField, Min(0.1f)] private float lockDuration = 1.5f;
    [SerializeField, Min(0f)] private float warningTime = 0.3f;
    [SerializeField, Min(0f)] private float reloadTime = 3f;
    [SerializeField, Min(0.1f)] private float roundSpeed = 42f;
    [SerializeField, Min(0f)] private float roundDamage = 60f;
    [SerializeField, Min(0f)] private float kineticDamage = 10f;
    [SerializeField, Min(1f)] private float aimTurnSpeed = 240f;

    private SlowMotionAbility focus;
    private Camera playerCamera;
    private MouseLook playerLook;
    private int coverMask;
    private int hitMask;
    private float health;
    public float HealthPercent => Mathf.Clamp01(health / maxHealth);
    private float firingAt;
    private float readyAt;
    private bool locking;
    private bool warned;
    private Vector3 beamAim;
    private float lockStartedAt;
    private MaterialPropertyBlock beamProperties;
    private EnemyVisualPart lens;
    private EnemyVisualPart lensRim;
    private EnemyVisualPart lensRecess;
    private EnemyVisualPart rifle;
    private EnemyVisualPart barrel;
    private EnemyVisualPart leftGlove;
    private EnemyVisualPart rightGlove;
    private EnemyVisualPart weaponMuzzle;
    private EnemyVisualPart leftSleeve;
    private EnemyVisualPart rightSleeve;
    private float chargePose;
    private float recoil;
    private float aimPitch;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public bool CanBePulled => false;
    public bool CanBePushed => health > 0f;
    public Vector3 ReturnTarget => transform.position + Vector3.up;

    private void Awake()
    {
        health = maxHealth;
        if (GetComponent<EnemyHitFlash>() == null) gameObject.AddComponent<EnemyHitFlash>();
        lens = new EnemyVisualPart(transform, "Lens");
        lensRim = new EnemyVisualPart(transform, "Lens rim");
        lensRecess = new EnemyVisualPart(transform, "Lens recess");
        rifle = new EnemyVisualPart(transform, "Relic rifle stock");
        barrel = new EnemyVisualPart(transform, "Relic rifle barrel");
        leftGlove = new EnemyVisualPart(transform, "Left glove");
        rightGlove = new EnemyVisualPart(transform, "Right glove");
        weaponMuzzle = new EnemyVisualPart(transform, "Muzzle");
        leftSleeve = new EnemyVisualPart(transform, "Left sleeve");
        rightSleeve = new EnemyVisualPart(transform, "Right sleeve");
        if (player == null) player = FindFirstObjectByType<PlayerMovement>()?.transform;
        if (player != null)
        {
            focus = player.GetComponentInChildren<SlowMotionAbility>();
            playerCamera = player.GetComponentInChildren<Camera>();
            playerLook = player.GetComponentInChildren<MouseLook>();
        }
        coverMask = LayerMask.GetMask("Default", "Environment", "Interactables");
        hitMask = coverMask | LayerMask.GetMask("Player", "Enemies");
        if (laser != null)
        {
            laser.enabled = false;
            laser.positionCount = 2;
            laser.useWorldSpace = true;
            laser.textureMode = LineTextureMode.Stretch;
            laser.alignment = LineAlignment.View;
            beamProperties = new MaterialPropertyBlock();
        }
    }

    private void Update()
    {
        if (health <= 0f || player == null || muzzle == null || laser == null || roundPrefab == null) return;

        Vector3 aim = playerCamera != null ? playerCamera.transform.position : player.position;
        Vector3 offset = aim - muzzle.position;
        if (offset.sqrMagnitude < 0.001f || offset.sqrMagnitude > detectionRange * detectionRange ||
            Physics.Linecast(muzzle.position, aim, coverMask, QueryTriggerInteraction.Ignore))
        {
            locking = false;
            laser.enabled = false;
            UpdateAimPose(aim, false);
            return;
        }

        UpdateAimPose(aim, Time.time >= readyAt);
        if (Time.time < readyAt) return;
        offset = aim - muzzle.position;
        if (offset.sqrMagnitude < 0.001f || Physics.Linecast(muzzle.position, aim, coverMask, QueryTriggerInteraction.Ignore))
        {
            locking = false;
            laser.enabled = false;
            return;
        }

        if (!locking)
        {
            locking = true;
            warned = false;
            lockStartedAt = Time.time;
            firingAt = Time.time + lockDuration;
            beamAim = player.position + Vector3.up * 0.15f;
        }

        bool imminent = firingAt - Time.time <= warningTime;
        if (imminent && !warned)
        {
            warned = true;
            playerLook?.AddScreenShake(0.15f, offset.normalized);
        }
        if (Time.time < firingAt || Vector3.Angle(muzzle.forward, offset) > 7f) return;

        locking = false;
        laser.enabled = false;
        readyAt = Time.time + reloadTime;
        EnemyProjectile shot = Instantiate(roundPrefab, muzzle.position, Quaternion.LookRotation(offset));
        shot.Launch(transform, roundSpeed, roundDamage, hitMask);
        shot.EnableReflection(this, focus);
        recoil = 0.17f;
    }

    private void UpdateAimPose(Vector3 target, bool aiming)
    {
        Vector3 heading = Vector3.ProjectOnPlane(target - transform.position, Vector3.up);
        if (aiming && heading.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(heading),
                aimTurnSpeed * Time.deltaTime);
        }

        Vector3 shoulder = new Vector3(0f, 1.55f, 0.12f);
        Vector3 localAim = transform.InverseTransformPoint(target) - shoulder;
        float targetPitch = aiming
            ? -Mathf.Atan2(localAim.y, new Vector2(localAim.x, localAim.z).magnitude) * Mathf.Rad2Deg
            : 0f;
        aimPitch = Mathf.MoveTowards(aimPitch, targetPitch, aimTurnSpeed * Time.deltaTime);
        chargePose = Mathf.MoveTowards(chargePose, aiming ? 1f : 0f, Time.deltaTime * 4f);
        Vector3 weaponOffset = Vector3.down * (0.065f * (1f - chargePose)) + Vector3.back * recoil;
        Quaternion pitch = Quaternion.Euler(aimPitch, 0f, 0f);
        Vector3 pivotOffset = shoulder + pitch * (weaponOffset - shoulder);
        lens.Orbit(pitch, pivotOffset, Quaternion.identity);
        lensRim.Orbit(pitch, pivotOffset, Quaternion.identity);
        lensRecess.Orbit(pitch, pivotOffset, Quaternion.identity);
        rifle.Orbit(pitch, pivotOffset, Quaternion.identity);
        barrel.Orbit(pitch, pivotOffset, Quaternion.identity);
        leftGlove.Orbit(pitch, pivotOffset, Quaternion.identity);
        rightGlove.Orbit(pitch, pivotOffset, Quaternion.identity);
        weaponMuzzle.Orbit(pitch, pivotOffset, Quaternion.identity);
        leftSleeve.Pose(weaponOffset * 0.3f, Quaternion.Euler(aimPitch * 0.6f, 0f, 0f));
        rightSleeve.Pose(weaponOffset * 0.3f, Quaternion.Euler(aimPitch * 0.6f, 0f, 0f));
        recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * 0.9f);
    }

    private void LateUpdate()
    {
        if (!locking || player == null || muzzle == null || laser == null || health <= 0f)
        {
            if (laser != null) laser.enabled = false;
            return;
        }

        Vector3 origin = muzzle.position;
        // Keep the warning beam below eye level so it is visible from the player's view.
        Vector3 aim = player.position + Vector3.up * 0.15f;
        beamAim = Vector3.Lerp(beamAim, aim, 1f - Mathf.Exp(-30f * Time.unscaledDeltaTime));
        Vector3 end = beamAim;
        if (Physics.Linecast(origin, end, out RaycastHit cover, coverMask, QueryTriggerInteraction.Ignore))
        {
            end = cover.point;
        }

        float charge = warningTime > 0f ? 1f - Mathf.Clamp01((firingAt - Time.time) / warningTime) : 0f;
        charge = Mathf.SmoothStep(0f, 1f, charge);
        float fade = Mathf.Clamp01((Time.time - lockStartedAt) / 0.10f);
        laser.enabled = true;
        laser.SetPosition(0, origin);
        laser.SetPosition(1, end);
        laser.widthMultiplier = Mathf.Lerp(0.045f, 0.15f, charge) * Mathf.Lerp(0.5f, 1f, fade);
        beamProperties.SetColor(BaseColor, Color.Lerp(
            new Color(1f, 0.04f, 0.02f), new Color(2f, 0.95f, 0.32f), charge));
        laser.SetPropertyBlock(beamProperties);
    }

    private void OnDisable()
    {
        locking = false;
        if (laser != null) laser.enabled = false;
    }

    public void OnReturnedShot() => TakeDamage(health);
    public void OnPushed(Vector3 direction, float force, Vector3 point) => TakeDamage(kineticDamage);
    public void OnPulled(Transform origin, float force) { }
    public void OnPhysicsCrush(float speed, GameObject prop)
    {
        Vector3 hitDir = prop != null ? (transform.position - prop.transform.position).normalized : -transform.forward;
        hitDir.y = 0.35f;
        hitDir.Normalize();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(hitDir * (speed * 10f), ForceMode.Impulse);
        }
        TakeDamage(kineticDamage);
    }
    public void OnPlayerSlideRam(float speed) { }

    public void TakeDamage(float amount, string source = "Unknown", int attackId = 0)
    {
        TakeDamage(amount);
    }

    private void TakeDamage(float damage)
    {
        if (health <= 0f) return;
        health -= damage;
        FloatingDamageNumber.Spawn(transform.position + Vector3.up * 1.5f, damage, damage >= 40f);
        GetComponent<EnemyHitFlash>()?.Flash();
        if (health <= 0f)
        {
            FindAnyObjectByType<PlayerHealth>()?.HealOnKill("Sniper", transform.position);
            Destroy(gameObject);
        }
    }
}
