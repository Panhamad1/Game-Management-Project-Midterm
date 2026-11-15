using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interactive Kinetic Target Button placed across the arena.
/// Activates when hit by:
/// 1. Crate Cannon (firing a block/crate at high speed)
/// 2. Slide Bowling Ram (physically sliding into it at 6+ m/s)
/// 3. Direct Wind Cannon (Push) blast
/// 
/// Visuals & Feedback:
/// - Glowing core switches from Bright Red to Neon Green
/// - Physically depresses inward on impact
/// - Plays procedural high-energy confirmation chime
/// - Siphons immediate Focus energy to player as a reward
/// </summary>
public class TargetButton : MonoBehaviour, IKineticTarget
{
    [Header("Button Identity & State")]
    [SerializeField] private string buttonName = "Target Switch";
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool canBeReset = false;
    [SerializeField] private float resetDelay = 6.0f;

    [Header("Visual Feedback")]
    [SerializeField] private MeshRenderer buttonCoreRenderer;
    [SerializeField] private Color inactiveColor = new Color(1.0f, 0.18f, 0.18f, 1.0f); // Bright Red
    [SerializeField] private Color activeColor = new Color(0.1f, 1.0f, 0.35f, 1.0f);    // Neon Green
    [SerializeField] private Transform movingCore;
    [SerializeField] private float depressDistance = 0.18f;

    [Header("Audio & Events")]
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;

    private Vector3 initialCoreLocalPos;
    private AudioSource audioSource;
    private float resetTimer = 0f;

    public bool IsActivated => isActivated;
    public string ButtonName => buttonName;

    // IKineticTarget Contracts
    public bool CanBePulled => false; // Anchored to wall/tower, cannot be carried away
    public bool CanBePushed => true;

    private void Awake()
    {
        if (movingCore != null)
        {
            initialCoreLocalPos = movingCore.localPosition;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.85f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 60.0f;
        }

        UpdateVisualState();
    }

    private void Update()
    {
        if (isActivated && canBeReset)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                Deactivate();
            }
        }
    }

    public void Activate()
    {
        if (isActivated && !canBeReset) return;

        isActivated = true;
        resetTimer = resetDelay;
        UpdateVisualState();
        PlayActivationChime();

        // Screen shake & Focus reward if close to player
        GameObject player = GameObject.Find("Player_Rig");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < 45f)
            {
                MouseLook ml = player.GetComponentInChildren<MouseLook>();
                if (ml != null) ml.AddScreenShake(0.2f * Mathf.Clamp01(1f - (dist / 45f)), Vector3.up);

                // Give immediate Focus / Slow-Mo reward!
                SlowMotionAbility slowMo = player.GetComponentInChildren<SlowMotionAbility>();
                if (slowMo != null) slowMo.SiphonEnergy(2.0f);
            }
        }

        onActivated?.Invoke();
        Debug.Log($"REBOUND: Target Button [{buttonName}] ACTIVATED!");
    }

    public void Deactivate()
    {
        isActivated = false;
        UpdateVisualState();
        onDeactivated?.Invoke();
    }

    private void UpdateVisualState()
    {
        if (buttonCoreRenderer != null)
        {
            Material mat = buttonCoreRenderer.material;
            mat.color = isActivated ? activeColor : inactiveColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", isActivated ? activeColor * 1.5f : inactiveColor * 1.2f);
            }
        }

        if (movingCore != null)
        {
            Vector3 targetPos = initialCoreLocalPos;
            if (isActivated)
            {
                targetPos -= Vector3.forward * depressDistance;
            }
            movingCore.localPosition = targetPos;
        }
    }

    public void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint)
    {
        Activate();
    }

    public void OnPulled(Transform pullOrigin, float pullForce)
    {
        // Anchored target
    }

    public void OnPhysicsCrush(float impactSpeed, GameObject flyingProp)
    {
        // Hit by high-speed flying crate launched via Crate Cannon!
        if (impactSpeed >= 4.0f)
        {
            Activate();
        }
    }

    public void OnKineticImpact(in KineticImpactContext context)
    {
        if (context.RelativeImpactSpeed >= 3.5f || context.ImpactEnergy >= 15.0f)
        {
            Activate();
        }
    }

    public void OnPlayerSlideRam(float slideSpeed)
    {
        // Player slid into button at high speed
        if (slideSpeed >= 5.5f)
        {
            Activate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detect direct crate collision
        if (collision.relativeVelocity.magnitude >= 4.0f)
        {
            InteractableProp prop = collision.gameObject.GetComponentInParent<InteractableProp>();
            if (prop != null)
            {
                Activate();
            }
        }
    }

    private void PlayActivationChime()
    {
        if (audioSource == null) return;

        int sampleRate = 44100;
        int numSamples = (int)(sampleRate * 0.45f);
        float[] samples = new float[numSamples];

        float freq1 = 880.0f; // A5 note
        float freq2 = 1760.0f; // A6 note

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 9.0f);
            samples[i] = (Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.5f +
                          Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.4f) * env;
        }

        AudioClip clip = AudioClip.Create("ButtonChime", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip, 0.9f);
    }
}
