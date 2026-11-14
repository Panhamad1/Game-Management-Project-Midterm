using UnityEngine;

/// <summary>
/// Implements the Slow Motion (Focus) ability.
/// - World physics slows to 25% speed.
/// - Dynamic FOV expansion (+8 degrees) for high-speed perspective rush.
/// - Synthesized sub-bass audio drops on enter and exit.
/// - Floaty air-time for player mid-air maneuvering.
/// </summary>
public class SlowMotionAbility : MonoBehaviour
{
    [Header("Time Scale Tuning")]
    [SerializeField] private float slowMoTimeScale = 0.25f;

    [Header("Perspective & Sensory Tuning")]
    [SerializeField] private float fovExpansion = 8.0f;

    [Header("Energy Meter (Focus Resource)")]
    [Tooltip("If true, Slow Motion has zero cooldown and can be used indefinitely.")]
    [SerializeField] private bool infiniteSlowMo = false;
    [SerializeField] private float maxEnergy = 100.0f;
    [SerializeField] private float drainRate = 25.0f; // 100 / 25 = 4 seconds duration
    [SerializeField] private float recoveryDelay = 0.75f; // Wait before recharging
    [SerializeField] private float recoveryRate = 15.0f; // Recharges full in ~6.6s
    [SerializeField] private float minActivationEnergy = 5.0f;

    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private PlayerAudioEffects audioEffects;

    private float currentEnergy;
    private float recoveryTimer = 0f;
    private bool isActive = false;
    private float defaultFixedDeltaTime;

    // Public API Contracts
    public bool IsActive => isActive;
    public float FovExpansion => fovExpansion;
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercent => infiniteSlowMo ? 1.0f : Mathf.Clamp01(currentEnergy / maxEnergy);

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCamera = GetComponentInChildren<Camera>();
        audioEffects = GetComponentInParent<PlayerAudioEffects>();

        if (playerCamera == null) playerCamera = Camera.main;

        currentEnergy = maxEnergy;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (infiniteSlowMo)
        {
            currentEnergy = maxEnergy;
        }
        else if (isActive)
        {
            currentEnergy -= drainRate * dt;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                DeactivateSlowMotion();
            }
        }
        else
        {
            if (recoveryTimer > 0f)
            {
                recoveryTimer -= dt;
            }
            else if (currentEnergy < maxEnergy)
            {
                // Kinetic Siphon Recharge (TEAM_DEVELOPMENT_HANDBOOK.md:71)
                // Focus recharges faster when moving fast
                float rate = recoveryRate;
                float playerSpeed = playerMovement != null ? playerMovement.CurrentSpeed : 0f;
                if (playerSpeed > 6.0f)
                {
                    float speedRatio = Mathf.Clamp01((playerSpeed - 6.0f) / 14.0f);
                    rate += 12.0f * speedRatio;
                }

                currentEnergy = Mathf.Min(currentEnergy + rate * dt, maxEnergy);
            }
        }

        // Camera FOV during Slow Motion is authoritatively handled by MouseLook.cs to avoid frame-by-frame FOV fighting.
    }

    /// <summary>
    /// Siphons immediate kinetic energy reward when smashing obstacles, ramming enemies, or landing high-speed stunts.
    /// </summary>
    public void SiphonEnergy(float bonus)
    {
        currentEnergy = Mathf.Min(currentEnergy + bonus, maxEnergy);
    }

    public void ActivateSlowMotion()
    {
        if (isActive || (!infiniteSlowMo && currentEnergy < minActivationEnergy)) return;

        isActive = true;
        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowMoTimeScale;

        if (playerMovement != null)
        {
            playerMovement.SetSlowMotionActive(true);
        }

        if (audioEffects != null)
        {
            audioEffects.PlaySlowMoEnter();
        }
    }

    public void DeactivateSlowMotion()
    {
        if (!isActive) return;

        isActive = false;
        recoveryTimer = recoveryDelay;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;

        if (playerMovement != null)
        {
            playerMovement.SetSlowMotionActive(false);
        }

        if (audioEffects != null)
        {
            audioEffects.PlaySlowMoExit();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
