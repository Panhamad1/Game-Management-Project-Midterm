using UnityEngine;

public sealed class SniperEnemy : MonoBehaviour, IKineticTarget
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

    private SlowMotionAbility focus;
    private Camera playerCamera;
    private MouseLook playerLook;
    private int coverMask;
    private int hitMask;
    private float health;
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
    private float chargePose;
    private float recoil;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public bool CanBePulled => false;
    public bool CanBePushed => health > 0f;
    public Vector3 ReturnTarget => transform.position + Vector3.up;

    private void Awake()
    {
        health = maxHealth;
        lens = new EnemyVisualPart(transform, "Lens");
        lensRim = new EnemyVisualPart(transform, "Lens rim");
        lensRecess = new EnemyVisualPart(transform, "Lens recess");
        rifle = new EnemyVisualPart(transform, "Relic rifle stock");
        barrel = new EnemyVisualPart(transform, "Relic rifle barrel");
        leftGlove = new EnemyVisualPart(transform, "Left glove");
        rightGlove = new EnemyVisualPart(transform, "Right glove");
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
        if (health <= 0f || player == null || muzzle == null || laser == null ||
            roundPrefab == null || Time.time < readyAt) return;

        Vector3 aim = playerCamera != null ? playerCamera.transform.position : player.position;
        Vector3 offset = aim - muzzle.position;
        if (offset.sqrMagnitude < 0.001f || offset.sqrMagnitude > detectionRange * detectionRange ||
            Physics.Linecast(muzzle.position, aim, coverMask, QueryTriggerInteraction.Ignore))
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
        if (Time.time < firingAt) return;

        locking = false;
        laser.enabled = false;
        readyAt = Time.time + reloadTime;
        EnemyProjectile shot = Instantiate(roundPrefab, muzzle.position, Quaternion.LookRotation(offset));
        shot.Launch(transform, roundSpeed, roundDamage, hitMask);
        shot.EnableReflection(this, focus);
        recoil = 0.17f;
    }

    private void LateUpdate()
    {
        chargePose = Mathf.MoveTowards(chargePose, locking ? 1f : 0f, Time.deltaTime * 4f);
        Vector3 weaponOffset = Vector3.down * (0.065f * (1f - chargePose)) + Vector3.back * recoil;
        lens.Pose(weaponOffset, Quaternion.identity);
        lensRim.Pose(weaponOffset, Quaternion.identity);
        lensRecess.Pose(weaponOffset, Quaternion.identity);
        rifle.Pose(weaponOffset, Quaternion.identity);
        barrel.Pose(weaponOffset, Quaternion.identity);
        leftGlove.Pose(weaponOffset, Quaternion.identity);
        rightGlove.Pose(weaponOffset, Quaternion.identity);
        recoil = Mathf.MoveTowards(recoil, 0f, Time.deltaTime * 0.9f);

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
    public void OnPhysicsCrush(float speed, GameObject prop) => TakeDamage(kineticDamage);
    public void OnPlayerSlideRam(float speed) { }

    private void TakeDamage(float damage)
    {
        if (health <= 0f) return;
        health -= damage;
        if (health <= 0f) Destroy(gameObject);
    }
}
