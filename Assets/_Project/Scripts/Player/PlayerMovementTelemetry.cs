using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Real-time diagnostic telemetry and movement validation instrumentation for REBOUND.
/// Implements Section 2 of 06_VALIDATION_AND_TEST_SUITE.md:
/// - Real-time on-screen telemetry overlay (Toggle with F3)
/// - Velocity vector decomposition (Horizontal, Vertical, Radial, Tangential)
/// - Ground normal, slope angle, and contact status
/// - Coyote time, jump buffer, and slide timers
/// - Grapple tether physics, distance, and tension metrics
/// - Speed cap monitoring and impulse logging
/// </summary>
public class PlayerMovementTelemetry : MonoBehaviour
{
    [Header("Telemetry Settings")]
    [Tooltip("If true, shows the diagnostic HUD overlay on screen.")]
    [SerializeField] private bool showOverlay = true;

    [Tooltip("Key to toggle the telemetry overlay.")]
    [SerializeField] private Key toggleKey = Key.F3;

    private PlayerMovement movement;
    private PullAbility pullAbility;
    private PushAbility pushAbility;
    private SlowMotionAbility slowMo;
    private MouseLook mouseLook;
    private Rigidbody rb;

    private float peakSpeed = 0f;
    private float peakAirborneSpeed = 0f;
    private float peakSlideSpeed = 0f;
    private string lastImpulseSource = "None";
    private float lastImpulseDeltaV = 0f;
    private float lastImpulseTime = -10f;
    private bool speedCapTriggered = false;

    // GUI Style
    private GUIStyle boxStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle warningStyle;
    private bool stylesInitialized = false;

    // Public API for test tracking
    public float PeakSpeed => peakSpeed;
    public float PeakAirborneSpeed => peakAirborneSpeed;
    public float PeakSlideSpeed => peakSlideSpeed;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        pullAbility = GetComponentInChildren<PullAbility>();
        pushAbility = GetComponentInChildren<PushAbility>();
        slowMo = GetComponentInChildren<SlowMotionAbility>();
        mouseLook = GetComponentInChildren<MouseLook>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
        {
            showOverlay = !showOverlay;
        }

        if (movement != null && rb != null)
        {
            float currentSpeed = movement.CurrentSpeed;
            if (currentSpeed > peakSpeed) peakSpeed = currentSpeed;

            if (!movement.IsGrounded && currentSpeed > peakAirborneSpeed)
            {
                peakAirborneSpeed = currentSpeed;
            }

            if (movement.IsSliding && currentSpeed > peakSlideSpeed)
            {
                peakSlideSpeed = currentSpeed;
            }

            speedCapTriggered = currentSpeed >= 34.8f;
        }
    }

    public void RecordImpulse(string source, float deltaV)
    {
        lastImpulseSource = source;
        lastImpulseDeltaV = deltaV;
        lastImpulseTime = Time.time;
    }

    public void ResetPeakMetrics()
    {
        peakSpeed = 0f;
        peakAirborneSpeed = 0f;
        peakSlideSpeed = 0f;
        lastImpulseSource = "None";
        lastImpulseDeltaV = 0f;
    }

    private void InitializeStyles()
    {
        if (stylesInitialized) return;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.08f, 0.12f, 0.88f));

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 13;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.3f, 0.85f, 1.0f); // Electric Cyan

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = Color.white;

        warningStyle = new GUIStyle(GUI.skin.label);
        warningStyle.fontSize = 11;
        warningStyle.fontStyle = FontStyle.Bold;
        warningStyle.normal.textColor = new Color(1.0f, 0.35f, 0.35f); // Soft Red

        stylesInitialized = true;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        if (!showOverlay || movement == null || rb == null) return;

        InitializeStyles();

        float panelWidth = 360f;
        float panelHeight = 440f;
        float margin = 12f;

        Rect panelRect = new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight);
        GUI.Box(panelRect, GUIContent.none, boxStyle);

        GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 8f, panelWidth - 20f, panelHeight - 16f));

        GUILayout.Label("REBOUND MOVEMENT TELEMETRY (F3)", headerStyle);
        GUILayout.Space(4f);

        Vector3 vel = rb.linearVelocity;
        Vector3 hVel = new Vector3(vel.x, 0f, vel.z);
        float hSpeed = hVel.magnitude;
        float vSpeed = vel.y;
        float totalSpeed = vel.magnitude;

        // 1. Velocity Decomposition
        GUILayout.Label($"Velocity: ({vel.x:F1}, {vel.y:F1}, {vel.z:F1}) m/s", labelStyle);
        GUILayout.Label($"Horizontal Speed: {hSpeed:F2} m/s | Vertical: {vSpeed:F2} m/s", labelStyle);
        GUILayout.Label($"Total Speed: {totalSpeed:F2} m/s (Cap: 35.0 m/s)", speedCapTriggered ? warningStyle : labelStyle);

        GUILayout.Space(4f);
        GUILayout.Label("LOCOMOTION STATE:", headerStyle);
        string locState = movement.IsSliding ? "SLIDING" : (movement.IsCrouching ? "CROUCHING" : (movement.IsSprinting ? "SPRINTING" : "WALKING"));
        if (!movement.IsGrounded) locState = vSpeed >= 0f ? "AIRBORNE_RISING" : "AIRBORNE_FALLING";
        if (pullAbility != null && pullAbility.IsGrappling) locState += " + GRAPPLING";

        GUILayout.Label($"State: {locState}", labelStyle);
        GUILayout.Label($"Grounded: {movement.IsGrounded} | Normal: ({movement.SlideDirection.x:F2}, {movement.SlideDirection.z:F2})", labelStyle);

        GUILayout.Space(4f);
        GUILayout.Label("PEAK KINETIC METRICS:", headerStyle);
        GUILayout.Label($"Peak Speed: {peakSpeed:F2} m/s", labelStyle);
        GUILayout.Label($"Peak Airborne: {peakAirborneSpeed:F2} m/s | Peak Slide: {peakSlideSpeed:F2} m/s", labelStyle);

        if (pullAbility != null && pullAbility.IsGrappling)
        {
            GUILayout.Space(4f);
            GUILayout.Label("GRAPPLE WINCH PHYSICS:", headerStyle);
            Vector3 anchor = pullAbility.GrapplePoint;
            float dist = Vector3.Distance(transform.position, anchor);
            Vector3 tetherDir = (anchor - transform.position).normalized;
            float inwardSpeed = Vector3.Dot(vel, tetherDir);
            Vector3 tangVel = vel - tetherDir * inwardSpeed;
            GUILayout.Label($"Anchor Dist: {dist:F1} m | Inward Speed: {inwardSpeed:F2} m/s", labelStyle);
            GUILayout.Label($"Tangential Swing Speed: {tangVel.magnitude:F2} m/s", labelStyle);
        }

        if (Time.time - lastImpulseTime < 3.0f)
        {
            GUILayout.Space(4f);
            GUILayout.Label($"Last Impulse: {lastImpulseSource} (+{lastImpulseDeltaV:F1} m/s)", warningStyle);
        }

        GUILayout.Space(4f);
        GUILayout.Label("PHYSICS RUNTIME:", headerStyle);
        GUILayout.Label($"TimeScale: {Time.timeScale:F2} | FixedDeltaTime: {Time.fixedDeltaTime:F4}s", labelStyle);
        GUILayout.Label($"Mass: {rb.mass}kg | ContinuousDynamic: {rb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic}", labelStyle);

        GUILayout.EndArea();
    }
}
