using UnityEngine;

/// <summary>
/// Tracks peak movement speeds for diagnostics.
/// </summary>
public class PlayerMovementTelemetry : MonoBehaviour
{
    private PlayerMovement movement;

    public float PeakSpeed { get; private set; }
    public float PeakAirborneSpeed { get; private set; }
    public float PeakSlideSpeed { get; private set; }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (movement == null) return;

        float speed = movement.CurrentSpeed;
        PeakSpeed = Mathf.Max(PeakSpeed, speed);
        if (!movement.IsGrounded) PeakAirborneSpeed = Mathf.Max(PeakAirborneSpeed, speed);
        if (movement.IsSliding) PeakSlideSpeed = Mathf.Max(PeakSlideSpeed, speed);
    }

    public void ResetPeakMetrics()
    {
        PeakSpeed = 0f;
        PeakAirborneSpeed = 0f;
        PeakSlideSpeed = 0f;
    }
}
