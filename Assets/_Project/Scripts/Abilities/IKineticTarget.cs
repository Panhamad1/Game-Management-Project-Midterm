using UnityEngine;

/// <summary>
/// Standard interface for any enemy, puzzle block, or physics object that reacts to
/// the player's Wind Blast (Push), Vacuum Vortex (Pull), or thrown crates (Crush).
/// Specified in docs/TEAM_DEVELOPMENT_HANDBOOK.md
/// </summary>
public interface IKineticTarget
{
    // Can this target be lifted or reeled in toward the player?
    bool CanBePulled { get; }

    // Can this target be blasted backward by wind cannon recoil?
    bool CanBePushed { get; }

    // Called when the player blasts this target with Wind Cannon (Push)
    void OnPushed(Vector3 forceDirection, float forceMagnitude, Vector3 hitPoint);

    // Called continuously while the player pulls this target with Vacuum Vortex
    void OnPulled(Transform pullOrigin, float pullForce);

    // Called when a high-speed flying crate or prop smashes into this target
    void OnPhysicsCrush(float impactSpeed, GameObject flyingProp);

    // Called when the player physically rams into this target at high slide speed (TEAM_DEVELOPMENT_HANDBOOK.md:182)
    void OnPlayerSlideRam(float slideSpeed);
}
