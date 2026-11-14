using UnityEngine;

/// <summary>
/// Contract for any projectile (Sniper bolt, Rapid-Sentry laser, energy projectile)
/// that can be parried and reflected by the player's Wind Cannon (Push blast).
/// Specified in 03_PUSH_WIND_CANNON_SPEC.md and 08_ENEMY_INTEGRATION_AND_CONTRACTS.md.
/// </summary>
public interface IDeflectableProjectile
{
    /// <summary>
    /// Whether this projectile is currently in a state that allows deflection.
    /// Returns false once already reflected to prevent infinite reflection loops.
    /// </summary>
    bool CanBeDeflected { get; }

    /// <summary>
    /// Attempts to deflect the projectile along a new trajectory with updated ownership.
    /// </summary>
    /// <param name="newDirection">Target flight direction for the deflected shot.</param>
    /// <param name="newOwner">New owner GameObject (e.g. the player) for faction damage attribution.</param>
    /// <returns>True if successfully deflected.</returns>
    bool TryDeflect(Vector3 newDirection, GameObject newOwner);
}
