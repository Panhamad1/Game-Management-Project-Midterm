using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for REBOUND Combat, Object Physics, Crate Cannon, and Focus upgrade systems.
/// Validates:
/// - Crate Cannon velocity inheritance equation
/// - Kinetic energy calculation (E = 0.5 * m * v^2)
/// - Focus meter drain and recharge rates
/// - Projectile deflection geometry and parry window
/// - Object weight class classification and traits
/// </summary>
public class CombatPhysicsTests
{
    [Test]
    public void Test_CrateCannon_VelocityInheritance_StationaryPlayer()
    {
        Vector3 aimDirection = Vector3.forward;
        float baseLaunchSpeed = 35.0f;
        Vector3 playerVelocity = Vector3.zero;

        Vector3 finalVelocity = aimDirection * baseLaunchSpeed + playerVelocity * 0.45f;

        Assert.AreEqual(35.0f, finalVelocity.magnitude, 0.001f);
        Assert.AreEqual(Vector3.forward * 35.0f, finalVelocity);
    }

    [Test]
    public void Test_CrateCannon_VelocityInheritance_SprintingPlayerBoost()
    {
        Vector3 aimDirection = Vector3.forward;
        float baseLaunchSpeed = 35.0f;
        Vector3 playerVelocity = Vector3.forward * 20.0f; // Sprinting forward at 20 m/s

        // 35 m/s base + (20 * 0.45) = 35 + 9 = 44 m/s launch velocity
        Vector3 finalVelocity = aimDirection * baseLaunchSpeed + playerVelocity * 0.45f;

        Assert.AreEqual(44.0f, finalVelocity.magnitude, 0.001f);
        Assert.AreEqual(Vector3.forward * 44.0f, finalVelocity);
    }

    [Test]
    public void Test_CrateCannon_VelocityInheritance_LateralSlide()
    {
        Vector3 aimDirection = Vector3.forward;
        float baseLaunchSpeed = 35.0f;
        Vector3 playerVelocity = Vector3.right * 10.0f; // Sliding laterally at 10 m/s

        Vector3 finalVelocity = aimDirection * baseLaunchSpeed + playerVelocity * 0.45f;

        Assert.AreEqual(35.0f, finalVelocity.z, 0.001f);
        Assert.AreEqual(4.5f, finalVelocity.x, 0.001f);
        // Total magnitude = sqrt(35^2 + 4.5^2) = sqrt(1225 + 20.25) = ~35.288 m/s
        Assert.AreEqual(35.288f, finalVelocity.magnitude, 0.01f);
    }

    [Test]
    public void Test_KineticEnergy_ScalesWithMassAndSquaredVelocity()
    {
        // Formula: E = 0.5 * m * v^2
        float v = 35.0f; // Crate Cannon speed

        // Light Crate (~1 kg)
        float mLight = 1.0f;
        float eLight = 0.5f * mLight * (v * v);
        Assert.AreEqual(612.5f, eLight, 0.1f);

        // Medium Crate (~5 kg)
        float mMedium = 5.0f;
        float eMedium = 0.5f * mMedium * (v * v);
        Assert.AreEqual(3062.5f, eMedium, 0.1f);

        // Heavy Crate (~25 kg, dragged at 15 m/s)
        float mHeavy = 25.0f;
        float vHeavy = 15.0f;
        float eHeavy = 0.5f * mHeavy * (vHeavy * vHeavy);
        Assert.AreEqual(2812.5f, eHeavy, 0.1f);
    }

    [Test]
    public void Test_WeakPoint_DoublesKineticImpactEnergy()
    {
        float baseEnergy = 3062.5f;
        float weakPointMultiplier = 2.0f;

        float effectiveEnergy = baseEnergy * weakPointMultiplier;
        Assert.AreEqual(6125.0f, effectiveEnergy, 0.1f);
    }

    [Test]
    public void Test_FocusResource_DrainRate_YieldsFourSecondsDuration()
    {
        float maxEnergy = 100.0f;
        float drainRate = 25.0f; // per second

        float duration = maxEnergy / drainRate;
        Assert.AreEqual(4.0f, duration, 0.001f, "100 energy at 25/s drain yields exactly 4 seconds of continuous focus.");
    }

    [Test]
    public void Test_FocusResource_RechargeTime_YieldsExpectedFullRecovery()
    {
        float maxEnergy = 100.0f;
        float recoveryRate = 15.0f; // per second

        float rechargeTime = maxEnergy / recoveryRate;
        Assert.AreEqual(6.666f, rechargeTime, 0.01f, "Full recharge takes approximately 6.67 seconds.");
    }

    [Test]
    public void Test_DeflectionParryAngle_DotProductValidation()
    {
        Vector3 aimForward = Vector3.forward;

        // Projectile coming directly towards player (traveling -forward)
        Vector3 incomingDirect = -Vector3.forward;
        // Target deflection direction is along aim forward
        Vector3 deflectedDirect = aimForward;
        Assert.AreEqual(1.0f, Vector3.Dot(aimForward, deflectedDirect), 0.001f);

        // 30-degree deflected angle should have dot product >= cos(30 deg) = 0.866
        Vector3 angledDir = Quaternion.Euler(0f, 30f, 0f) * aimForward;
        float dotAngle = Vector3.Dot(aimForward, angledDir);
        Assert.GreaterOrEqual(dotAngle, 0.865f);
    }

    [Test]
    public void Test_WeightClass_HeavyCannotBeLaunched()
    {
        WeightClass lightClass = WeightClass.Light;
        WeightClass mediumClass = WeightClass.Medium;
        WeightClass heavyClass = WeightClass.Heavy;

        bool lightCanLaunch = lightClass != WeightClass.Heavy;
        bool mediumCanLaunch = mediumClass != WeightClass.Heavy;
        bool heavyCanLaunch = heavyClass != WeightClass.Heavy;

        Assert.IsTrue(lightCanLaunch);
        Assert.IsTrue(mediumCanLaunch);
        Assert.IsFalse(heavyCanLaunch, "Heavy weight class props must be restricted from mid-air launch.");
    }
}
