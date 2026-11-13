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

    // IKineticTarget implementation
    public bool CanBePulled => canPull;
    public bool CanBePushed => canPush;
    public Rigidbody Rb => rb;
    public bool IsCarried => isCarried;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        propRenderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Enforce physical mass defaults by weight class
        ApplyMassByClass();
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
        // Subtle emissive glow or outline when aimed at
        for (int i = 0; i < propRenderers.Length; i++)
        {
            if (propRenderers[i] == null) continue;
            propRenderers[i].GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", on ? new Color(0.2f, 0.4f, 0.9f) : Color.black);
            propRenderers[i].SetPropertyBlock(propBlock);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If this crate was thrown as a missile and hits an enemy
        float relativeSpeed = collision.relativeVelocity.magnitude;
        if (relativeSpeed >= crushSpeedThreshold)
        {
            IKineticTarget target = collision.gameObject.GetComponent<IKineticTarget>();
            if (target != null && collision.gameObject != gameObject)
            {
                target.OnPhysicsCrush(relativeSpeed, gameObject);
            }
        }
    }
}
