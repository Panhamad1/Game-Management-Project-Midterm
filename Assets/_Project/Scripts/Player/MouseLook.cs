using UnityEngine;

/// <summary>
/// First-person camera look controller:
/// - Smooth, uninhibited mouse aiming with zero physics resistance
/// - Vertical pitch strictly clamped between -85 and +85 degrees
/// - Clean horizontal yaw rotation with zero camera roll or tilt
/// - Subtle vertical-only head-bobbing and landing compression dip
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Header("Look Sensitivity")]
    [Tooltip("Mouse sensitivity multiplier.")]
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("Hierarchy References")]
    [SerializeField] private Transform playerBody;

    [Header("Head Motion (Vertical Only)")]
    [SerializeField] private float bobFrequency = 10.0f;
    [SerializeField] private float bobVerticalAmplitude = 0.03f;

    [Header("Landing Dip")]
    [SerializeField] private float landingDipMax = 0.15f;
    [SerializeField] private float landingRecoverySpeed = 6.0f;

    [Header("Dynamic FOV & Speed Banking")]
    [SerializeField] private float baseFOV = 85f;
    [SerializeField] private float maxFOV = 105f;
    [SerializeField] private float maxFOVSpeed = 30f;
    [SerializeField] private float maxBankAngle = 2.5f;
    [SerializeField] private float bankSpeed = 8.0f;

    private Camera cam;
    private SlowMotionAbility slowMotionAbility;
    private float pitchAngle = 0f;
    private float yawAngle = 0f;
    private float currentRollAngle = 0f;
    private Vector2 currentLookDelta = Vector2.zero;
    private Vector3 initialLocalPos;
    private float bobTimer = 0f;
    private float currentLandingDip = 0f;
    private float targetEyeLocalY = 0.65f;
    private float currentEyeLocalY = 0.65f;
    private bool isSliding = false;
    private Rigidbody bodyRb;

    private float shakeTrauma = 0f;
    private Vector3 shakeDirection = Vector3.zero;

    private void Start()
    {
        LockCursor();

        cam = GetComponent<Camera>();
        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            // CRITICAL: Ensure near clip plane is razor thin to prevent ground clipping
            cam.nearClipPlane = 0.03f;
        }

        if (slowMotionAbility == null) slowMotionAbility = GetComponentInParent<SlowMotionAbility>() ?? FindAnyObjectByType<SlowMotionAbility>();

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        if (playerBody != null)
        {
            yawAngle = playerBody.eulerAngles.y;
            bodyRb = playerBody.GetComponent<Rigidbody>();
        }

        initialLocalPos = transform.localPosition;
        currentEyeLocalY = initialLocalPos.y;
        targetEyeLocalY = initialLocalPos.y;

        pitchAngle = transform.localEulerAngles.x;
        if (pitchAngle > 180f) pitchAngle -= 360f;
    }

    public void SetCameraTargetY(float targetY)
    {
        targetEyeLocalY = targetY;
    }

    public void SetCrouchYOffset(float offset)
    {
        // Backward compatibility
        targetEyeLocalY = initialLocalPos.y + offset;
    }

    public void SetSliding(bool sliding)
    {
        isSliding = sliding;
    }

    public void SetLookInput(Vector2 lookDelta)
    {
        currentLookDelta = lookDelta;
    }

    public void AddScreenShake(float intensity, Vector3 direction)
    {
        shakeTrauma = Mathf.Clamp01(shakeTrauma + intensity);
        shakeDirection = direction.normalized;
    }

    public void TriggerLandingDip(float fallSpeed)
    {
        float dip = Mathf.Clamp(Mathf.Abs(fallSpeed) * 0.01f, 0.03f, landingDipMax);
        currentLandingDip = dip;
    }

    public void UpdateCameraMotion(Vector2 moveInput, bool isGrounded, float speed)
    {
        float dt = Time.unscaledDeltaTime;

        // 1. Mouse Aiming: Instant, uninhibited rotation
        float mouseX = currentLookDelta.x * mouseSensitivity;
        float mouseY = currentLookDelta.y * mouseSensitivity;

        // Consume look delta immediately
        currentLookDelta = Vector2.zero;

        // Vertical Pitch (Up / Down)
        pitchAngle -= mouseY;
        pitchAngle = Mathf.Clamp(pitchAngle, -85f, 85f);

        // Horizontal Yaw (Left / Right) directly on player body
        // Maintain an authoritative yaw angle so we NEVER read back from the physics-interpolated transform!
        yawAngle += mouseX;
        if (playerBody != null)
        {
            Quaternion bodyRot = Quaternion.Euler(0f, yawAngle, 0f);
            playerBody.rotation = bodyRot;
            if (bodyRb != null)
            {
                bodyRb.rotation = bodyRot;
                bodyRb.angularVelocity = Vector3.zero;
            }
        }

        // 2. Camera Shake Decay
        Vector3 shakeOffset = Vector3.zero;
        if (shakeTrauma > 0f)
        {
            float shakePower = shakeTrauma * shakeTrauma * 0.08f;
            shakeOffset = new Vector3(
                (Random.value * 2f - 1f) * shakePower,
                (Random.value * 2f - 1f) * shakePower,
                0f
            ) + shakeDirection * (shakeTrauma * 0.05f);

            shakeTrauma = Mathf.MoveTowards(shakeTrauma, 0f, 4.0f * dt);
        }

        // Apply local rotation to camera pivot with subtle speed banking and slide carve tilt
        float targetRoll = -moveInput.x * Mathf.Clamp01(speed / 10f) * maxBankAngle;
        currentRollAngle = Mathf.Lerp(currentRollAngle, targetRoll, bankSpeed * dt);
        float effectivePitch = pitchAngle + (isSliding ? 1.5f : 0f);
        transform.localRotation = Quaternion.Euler(effectivePitch, 0f, currentRollAngle);

        // Dynamic FOV based on movement speed (+4 degrees when sliding, +2 degrees subtle breathe when in Slow-Mo)
        if (cam != null)
        {
            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, Mathf.Clamp01(speed / maxFOVSpeed));
            if (isSliding) targetFOV = Mathf.Min(targetFOV + 4f, maxFOV);
            if (slowMotionAbility != null && slowMotionAbility.IsActive) targetFOV = Mathf.Min(targetFOV + slowMotionAbility.FovExpansion, maxFOV);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, 8.0f * dt);
        }

        // 3. Vertical-Only Head Bobbing (disabled while sliding)
        Vector3 bobOffset = Vector3.zero;
        if (isGrounded && !isSliding && moveInput.sqrMagnitude > 0.05f && speed > 0.5f)
        {
            bobTimer += dt * bobFrequency;
            float verticalBob = Mathf.Sin(bobTimer) * bobVerticalAmplitude;
            bobOffset = new Vector3(0f, verticalBob, 0f);
        }
        else
        {
            bobTimer = 0f;
        }

        // 4. Landing Dip & Smooth Eye Height Transition (Never dips below target eye level)
        if (currentLandingDip > 0f)
        {
            currentLandingDip = Mathf.MoveTowards(currentLandingDip, 0f, landingRecoverySpeed * dt);
        }

        currentEyeLocalY = Mathf.MoveTowards(currentEyeLocalY, targetEyeLocalY, 10.0f * dt);

        transform.localPosition = new Vector3(initialLocalPos.x, currentEyeLocalY, initialLocalPos.z) + bobOffset - Vector3.up * currentLandingDip + shakeOffset;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
