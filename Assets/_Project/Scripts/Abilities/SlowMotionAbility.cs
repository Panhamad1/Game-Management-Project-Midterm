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

    [Header("Energy Meter (Seconds)")]
    [Tooltip("If true, Slow Motion has zero cooldown and can be used indefinitely.")]
    [SerializeField] private bool infiniteSlowMo = true;
    [SerializeField] private float maxDuration = 4.0f;
    [SerializeField] private float rechargeDuration = 6.0f;

    private PlayerMovement playerMovement;
    private Camera playerCamera;
    private PlayerAudioEffects audioEffects;

    private float currentEnergy;
    private bool isActive = false;
    private float defaultFixedDeltaTime;
    private float baseFOV = 60f;
    private float targetFOV;

    // Public API Contracts
    public bool IsActive => isActive;
    public float EnergyPercent => infiniteSlowMo ? 1.0f : Mathf.Clamp01(currentEnergy / maxDuration);

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerCamera = GetComponentInChildren<Camera>();
        audioEffects = GetComponentInParent<PlayerAudioEffects>();

        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCamera != null)
        {
            baseFOV = playerCamera.fieldOfView;
        }

        targetFOV = baseFOV;
        currentEnergy = maxDuration;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (infiniteSlowMo)
        {
            currentEnergy = maxDuration;
        }
        else if (isActive)
        {
            currentEnergy -= dt;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                DeactivateSlowMotion();
            }
        }
        else
        {
            // Kinetic Siphon Recharge (TEAM_DEVELOPMENT_HANDBOOK.md:71)
            // Focus primarily recharges while moving fast; slow baseline trickle while walking
            float playerSpeed = playerMovement != null ? playerMovement.CurrentSpeed : 0f;
            if (playerSpeed > 4.5f && currentEnergy < maxDuration)
            {
                float speedRatio = Mathf.Clamp01((playerSpeed - 4.5f) / 15.0f);
                float rechargeRate = Mathf.Lerp(0.4f, 2.2f, speedRatio);
                currentEnergy = Mathf.Min(currentEnergy + rechargeRate * dt, maxDuration);
            }
            else if (rechargeDuration > 0f && currentEnergy < maxDuration)
            {
                // Slow ambient passive recovery
                currentEnergy = Mathf.Min(currentEnergy + (maxDuration / rechargeDuration) * 0.15f * dt, maxDuration);
            }
        }

        // Smooth camera FOV transition during Slow Motion
        if (playerCamera != null)
        {
            targetFOV = baseFOV + (isActive ? fovExpansion : 0f);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, dt * 10f);
        }
    }

    /// <summary>
    /// Siphons immediate kinetic energy reward when smashing obstacles, ramming enemies, or landing high-speed stunts.
    /// </summary>
    public void SiphonEnergy(float bonus)
    {
        currentEnergy = Mathf.Min(currentEnergy + bonus, maxDuration);
    }

    public void ActivateSlowMotion()
    {
        if (isActive || (!infiniteSlowMo && currentEnergy <= 0.5f)) return;

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
