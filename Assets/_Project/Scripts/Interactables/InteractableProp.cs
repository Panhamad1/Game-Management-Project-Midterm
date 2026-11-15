using UnityEngine;

public enum WeightClass
{
    Light,   // ~1.0kg: Fast projectile, simple stacking
    Medium,  // ~5.0kg: Standard puzzle block, counter-weights, combat missile
    Heavy    // ~25.0kg: Ground drag only, immovable barricades, cannot be lifted
}

/// <summary>
/// Attached to movable puzzle blocks, crates, and props.
/// Implements IKineticTarget and handles physical weight classes, high-speed crush collisions, and reticle highlights.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class InteractableProp : MonoBehaviour, IKineticTarget
{
    [Header("Prop Properties")]
    [SerializeField] private WeightClass weightClass = WeightClass.Medium;
    [SerializeField] private bool canPush = true;
    [SerializeField] private bool canPull = true;

    [Header("Crush Damage Tuning")]
    [Tooltip("Minimum impact velocity before this prop deals crush damage to an enemy.")]
    [SerializeField] private float crushSpeedThreshold = 4.0f;

    private Rigidbody rb;
    private Renderer[] propRenderers;
    private MaterialPropertyBlock propBlock;
    private bool isCarried = false;

    public WeightClass Class => weightClass;
    public bool CanPush => canPush;
    public bool CanPull => canPull;
    public bool CanBeHeld => weightClass != WeightClass.Heavy;
    public bool CanBeLaunched => weightClass != WeightClass.Heavy;
    public bool CanBlockProjectiles => weightClass != WeightClass.Light;

    // IKineticTarget implementation
    public bool CanBePulled => canPull;
    public bool CanBePushed => canPush;
    public Rigidbody Rb => rb;
    public bool IsCarried => isCarried;

    private int lastImpactTargetId = 0;
    private float lastImpactTime = -10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        propRenderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Enforce physical mass defaults by weight class
        ApplyMassByClass();
    }

    private Light auraLight;

    private void Start()
    {
        if (propRenderers != null)
        {
            for (int i = 0; i < propRenderers.Length; i++)
            {
                if (propRenderers[i] == null) continue;
                foreach (Material m in propRenderers[i].materials)
                {
                    if (m != null)
                    {
                        m.EnableKeyword("_EMISSION");
                        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    }
                }
            }
        }

        CreateSubtleGlowAura();
        ApplyGlowIndicator(false);
    }

    private void CreateSubtleGlowAura()
    {
        if (auraLight != null) return;

        GameObject lightObj = new GameObject("GlowAura");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.up * 0.15f;

        auraLight = lightObj.AddComponent<Light>();
        auraLight.type = LightType.Point;
        auraLight.range = 2.6f;
        auraLight.intensity = 0.55f; // Soft, subtle ambient radiance

        switch (weightClass)
        {
            case WeightClass.Light:
                auraLight.color = new Color(0.18f, 0.95f, 0.55f); // Scarab Jade / Mint
                break;
            case WeightClass.Medium:
                auraLight.color = new Color(1.0f, 0.82f, 0.22f); // Egyptian Gold / Amber
                break;
            case WeightClass.Heavy:
            default:
                auraLight.color = new Color(0.95f, 0.25f, 0.18f); // Molten Ruby / Crimson
                break;
        }
    }

    public void ApplyGlowIndicator(bool highlighted)
    {
        Color glowColor;
        switch (weightClass)
        {
            case WeightClass.Light:
                // Subtle Scarab Jade / Mint Cyan (Light: Urns, small rocks)
                glowColor = new Color(0.12f, 0.75f, 0.45f) * (highlighted ? 0.9f : 0.45f);
                break;
            case WeightClass.Medium:
                // Subtle Egyptian Amber / Gold (Medium: Sandstone boulders/crates)
                glowColor = new Color(0.90f, 0.65f, 0.15f) * (highlighted ? 0.9f : 0.45f);
                break;
            case WeightClass.Heavy:
            default:
                // Subtle Molten Bronze / Deep Ruby (Heavy: Obsidian monoliths)
                glowColor = new Color(0.80f, 0.20f, 0.15f) * (highlighted ? 0.9f : 0.45f);
                break;
        }

        for (int i = 0; i < propRenderers.Length; i++)
        {
            if (propRenderers[i] == null) continue;
            propRenderers[i].GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", glowColor);
            propRenderers[i].SetPropertyBlock(propBlock);
        }

        if (auraLight != null)
        {
            auraLight.intensity = highlighted ? 0.85f : 0.55f;
        }
    }

    public void SetCarriedState(bool carried)
    {
        isCarried = carried;
    }

    private void ApplyMassByClass()
    {
        switch (weightClass)
        {
            case WeightClass.Light:
                rb.mass = 1.0f;
                break;
            case WeightClass.Medium:
                rb.mass = 5.0f;
                break;
            case WeightClass.Heavy:
                rb.mass = 25.0f;
                // Heavy blocks drag on floor: prevent upward rotation instability
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                break;
        }
    }

    public void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint)
    {
        if (!canPush) return;

        rb.AddForceAtPosition(forceDirection * forceMagnitude, hitPoint, ForceMode.Impulse);
    }

    public void OnPulled(Transform pullOrigin, float pullForce)
    {
        if (!canPull) return;

        Vector3 targetPos = pullOrigin.position;

        // Heavy weight class rule: cannot be lifted into air, ground drag only!
        if (weightClass == WeightClass.Heavy)
        {
            targetPos.y = transform.position.y;
        }

        Vector3 dir = (targetPos - transform.position).normalized;
        rb.AddForce(dir * pullForce, ForceMode.Acceleration);
    }

    public void OnPhysicsCrush(float impactSpeed, GameObject flyingProp)
    {
        // Props can react to other props smashing into them (e.g. shatter or slide)
        if (impactSpeed >= crushSpeedThreshold && rb != null)
        {
            Vector3 pushDir = (transform.position - flyingProp.transform.position).normalized;
            rb.AddForce(pushDir * impactSpeed * 2.0f, ForceMode.Impulse);
        }
    }

    public void OnKineticImpact(in KineticImpactContext context)
    {
        if (context.RelativeImpactSpeed >= crushSpeedThreshold && rb != null)
        {
            Vector3 pushDir = context.Direction.sqrMagnitude > 0.01f ? context.Direction : -context.Normal;
            rb.AddForceAtPosition(pushDir * Mathf.Min(context.ImpactEnergy * 0.5f, 50f), context.Point, ForceMode.Impulse);
        }
    }

    public void OnPlayerSlideRam(float slideSpeed)
    {
        if (rb != null)
        {
            Vector3 ramDir = Camera.main != null ? Camera.main.transform.forward : transform.forward;
            ramDir.y = 0.25f;
            ramDir.Normalize();

            float massScale = weightClass == WeightClass.Light ? 3.0f : (weightClass == WeightClass.Medium ? 2.0f : 1.2f);
            rb.AddForce(ramDir * slideSpeed * massScale, ForceMode.Impulse);
        }

        // Breakable barricade planks shatter when rammed at speed
        if (gameObject.name.Contains("Plank") || gameObject.name.Contains("Barricade"))
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(slideSpeed * 30f, transform.position - Vector3.forward * 0.5f, 3.0f);
            }
        }
    }

    public void SetHighlighted(bool on)
    {
        ApplyGlowIndicator(on);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == gameObject) return;
        // Held props must NEVER trigger crush damage or kinetic destruction while carried in hands
        if (isCarried) return;

        float relativeSpeed = collision.relativeVelocity.magnitude;
        if (relativeSpeed >= crushSpeedThreshold)
        {
            int targetId = collision.collider.gameObject.GetInstanceID();
            if (targetId == lastImpactTargetId && (Time.time - lastImpactTime) < 0.2f) return;
            lastImpactTargetId = targetId;
            lastImpactTime = Time.time;

            IKineticTarget target = collision.collider.GetComponentInParent<IKineticTarget>();
            if (target != null && collision.rigidbody != rb)
            {
                ContactPoint contact = collision.GetContact(0);
                float mass = rb != null ? rb.mass : 5.0f;
                float energy = 0.5f * mass * relativeSpeed * relativeSpeed;
                bool isWeakPoint = collision.collider.gameObject.name.ToLower().Contains("weakpoint") || collision.collider.GetComponentInParent<HulkWeakPoint>() != null;

                KineticImpactContext context = new KineticImpactContext(
                    gameObject,
                    contact.point,
                    contact.normal,
                    -collision.relativeVelocity.normalized,
                    relativeSpeed,
                    energy,
                    isWeakPoint
                );

                target.OnKineticImpact(context);
                target.OnPhysicsCrush(relativeSpeed, gameObject);
            }
        }
    }
}
