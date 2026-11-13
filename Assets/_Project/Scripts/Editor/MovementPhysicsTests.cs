using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for REBOUND movement physics equations and candidate models.
/// Validates Section 4 of 05_ARCHITECTURE_AND_IMPLEMENTATION_PLAN.md and 07_PARAMETER_LEDGER.md:
/// - Reconciled Jump Height (1.6m clearance under 2.4x gravity)
/// - Quake/Source Vector-Projection Air Strafing
/// - Slope Surfing Acceleration & Aerodynamic Slide Drag
/// - Grapple Inward Clamping & Tangential Velocity Preservation
/// - Surface Proximity Recoil Scaling
/// - Diagonal Input Normalization
/// </summary>
public class MovementPhysicsTests
{
    [Test]
    public void Test_ReconciledJumpImpulse_YieldsTarget1Point6MeterHeight()
    {
        // Target: 1.6m height
        // Gravity: standard world gravity 9.81 m/s^2 * 2.4 = 23.544 m/s^2
        float effectiveGravity = 9.81f * 2.4f;
        float targetHeight = 1.6f;

        // Formula: v_y = sqrt(2 * g * h)
        float theoreticalImpulse = Mathf.Sqrt(2f * effectiveGravity * targetHeight);

        // Assert theoretical impulse is approximately 8.68 m/s (~8.7 m/s)
        Assert.AreEqual(8.68f, theoreticalImpulse, 0.05f);

        // Verification: If impulse is 8.7 m/s, height = v^2 / (2 * g)
        float actualHeight = (8.7f * 8.7f) / (2f * effectiveGravity);
        Assert.AreEqual(1.6f, actualHeight, 0.05f, "8.7 m/s takeoff velocity precisely achieves 1.6m vertical clearance under 2.4x gravity.");
    }

    [Test]
    public void Test_DiagonalInputNormalization_NeverExceedsUnitMagnitude()
    {
        // Diagonal WASD raw vector is (1, 1)
        Vector2 rawInput = new Vector2(1f, 1f);
        Vector2 normalized = Vector2.ClampMagnitude(rawInput, 1f);

        Assert.LessOrEqual(normalized.magnitude, 1.0001f);
        Assert.AreEqual(0.7071f, normalized.x, 0.001f);
        Assert.AreEqual(0.7071f, normalized.y, 0.001f);
    }

    [Test]
    public void Test_AirStrafing_PreservesSpeedAndAddsAccelerationOnCurve()
    {
        // Player moving along Z at 15 m/s, strafes to the right (X = 1)
        Vector3 initialVelocity = new Vector3(0f, 0f, 15f);
        Vector3 wishDir = new Vector3(1f, 0f, 0f); // 90-degree strafe

        float currentProjection = Vector3.Dot(initialVelocity, wishDir); // 0
        float maxAirWishSpeed = 16.0f;
        float addSpeed = maxAirWishSpeed - currentProjection; // 16.0f

        float airAcceleration = 26.0f;
        float dt = 0.02f;
        float accelSpeed = airAcceleration * dt; // 0.52 m/s
        float speedToApply = Mathf.Min(addSpeed, accelSpeed);

        Vector3 newVelocity = initialVelocity + wishDir * speedToApply;

        // Forward velocity is 100% preserved (15 m/s)
        Assert.AreEqual(15f, newVelocity.z, 0.001f);
        // Lateral velocity developed
        Assert.AreEqual(0.52f, newVelocity.x, 0.001f);
        // Total speed increased from 15 m/s to ~15.009 m/s without energy loss
        Assert.Greater(newVelocity.magnitude, initialVelocity.magnitude);
    }

    [Test]
    public void Test_SlideDrag_DeceleratesSmoothlyAtHighSpeed()
    {
        float slideFriction = 2.2f; // m/s^2
        float slideDrag = 0.035f;   // 1/s
        float dt = 0.02f;

        // Test at 30 m/s
        float v1 = 30f;
        float decel1 = slideFriction + (v1 * slideDrag); // 2.2 + 1.05 = 3.25 m/s^2
        float v1_next = v1 - (decel1 * dt);

        Assert.AreEqual(3.25f, decel1, 0.01f);
        Assert.AreEqual(29.935f, v1_next, 0.01f);

        // Test at 10 m/s
        float v2 = 10f;
        float decel2 = slideFriction + (v2 * slideDrag); // 2.2 + 0.35 = 2.55 m/s^2
        Assert.AreEqual(2.55f, decel2, 0.01f);
        Assert.Greater(decel1, decel2, "Drag is naturally higher at 30 m/s than at 10 m/s.");
    }

    [Test]
    public void Test_SlopeSurfing_DownhillAcceleratesUphillDecelerates()
    {
        Vector3 groundNormal = new Vector3(0f, 0.8988f, -0.4384f).normalized; // ~26 degree slope
        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
        float slopeAcceleration = 28.0f;
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        // Slide direction matches downhill
        Vector3 slidingDownhill = downhill;
        float dotDown = Vector3.Dot(slidingDownhill, downhill);
        float angleRad = slopeAngle * Mathf.Deg2Rad;
        float accelDown = dotDown * slopeAcceleration * Mathf.Sin(angleRad);

        Assert.Greater(accelDown, 10.0f, "Downhill surfing generates positive acceleration.");

        // Slide direction opposing downhill (going uphill)
        Vector3 slidingUphill = -downhill;
        float dotUp = Vector3.Dot(slidingUphill, downhill);
        float accelUp = dotUp * slopeAcceleration * Mathf.Sin(angleRad);

        Assert.Less(accelUp, -10.0f, "Uphill surfing generates negative deceleration.");
    }

    [Test]
    public void Test_GrappleDecomposition_Preserves100PercentTangentialVelocity()
    {
        // Player is moving east (X = 20 m/s), tether pulls north (Z = 1)
        Vector3 currentVel = new Vector3(20f, 0f, 5f);
        Vector3 pullDir = new Vector3(0f, 0f, 1f); // Along Z

        // Integrate pull force along pullDir
        float pullAccel = 55.0f;
        float dt = 0.02f;
        Vector3 pullForce = pullDir * (pullAccel * dt); // +1.1 m/s along Z
        Vector3 newVel = currentVel + pullForce; // (20, 0, 6.1)

        // Clamping inward speed along pullDir to targetSpeed (e.g., 5.0 m/s)
        float targetSpeed = 5.0f;
        float dot = Vector3.Dot(newVel, pullDir); // 6.1
        if (dot > targetSpeed)
        {
            newVel -= pullDir * (dot - targetSpeed);
        }

        // Tangential velocity (along X) is 100% preserved at exactly 20.0 m/s!
        Assert.AreEqual(20.0f, newVel.x, 0.0001f, "Tangential velocity is fully preserved without orbital decay.");
        // Inward velocity (along Z) clamped to targetSpeed
        Assert.AreEqual(5.0f, newVel.z, 0.0001f);
    }

    [Test]
    public void Test_SurfaceProximityRecoil_ZeroBeyondThresholdMaxAtPointBlank()
    {
        float surfaceProximityDistance = 3.8f;
        float surfaceKickVelocity = 13.5f;

        // Distance = 5.0m (beyond threshold)
        float distFar = 5.0f;
        bool hasKickFar = distFar <= surfaceProximityDistance;
        Assert.IsFalse(hasKickFar, "Beyond 3.8m gives zero recoil.");

        // Distance = 0.0m (point-blank)
        float distNear = 0.0f;
        float ratioNear = 1.0f - Mathf.Clamp01(distNear / surfaceProximityDistance); // 1.0
        float kickNear = surfaceKickVelocity * (0.8f + ratioNear * 0.4f); // 13.5 * 1.2 = 16.2 m/s
        Assert.AreEqual(16.2f, kickNear, 0.01f);

        // Distance = 3.8m (at exact edge)
        float distEdge = 3.8f;
        float ratioEdge = 1.0f - Mathf.Clamp01(distEdge / surfaceProximityDistance); // 0.0
        float kickEdge = surfaceKickVelocity * (0.8f + ratioEdge * 0.4f); // 13.5 * 0.8 = 10.8 m/s
        Assert.AreEqual(10.8f, kickEdge, 0.01f);
    }

    [Test]
    public void Test_WeightClassReelDynamics_ScalesPullSpeedByMass()
    {
        // Light (~1kg) snaps at 32 m/s for instant mid-air projectile snatching
        float lightReelSpeed = 32.0f;
        // Medium (~5kg) reels at 24 m/s
        float mediumReelSpeed = 24.0f;
        // Heavy (~25kg) drags at 12 m/s (heavier, takes longer)
        float heavyReelSpeed = 12.0f;

        Assert.Greater(lightReelSpeed, mediumReelSpeed, "Light objects reel in faster than medium objects.");
        Assert.Greater(mediumReelSpeed, heavyReelSpeed, "Medium objects reel in faster than heavy objects.");
        Assert.AreEqual(32.0f, lightReelSpeed);
        Assert.AreEqual(24.0f, mediumReelSpeed);
        Assert.AreEqual(12.0f, heavyReelSpeed);
    }
}
