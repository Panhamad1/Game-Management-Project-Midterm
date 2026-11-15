using UnityEngine;

/// <summary>
/// Immutable context struct describing a kinetic impact event on an IKineticTarget.
/// Encapsulates relative speed, kinetic impact energy, contact normal, and weak-point detection.
/// Specified in 08_ENEMY_INTEGRATION_AND_CONTRACTS.md.
/// </summary>
public struct KineticImpactContext
{
    public GameObject SourceObject;
    public Vector3 Point;
    public Vector3 Normal;
    public Vector3 Direction;
    public float RelativeImpactSpeed;
    public float ImpactEnergy;
    public bool HitWeakPoint;

    public KineticImpactContext(
        GameObject sourceObject,
        Vector3 point,
        Vector3 normal,
        Vector3 direction,
        float relativeImpactSpeed,
        float impactEnergy,
        bool hitWeakPoint)
    {
        SourceObject = sourceObject;
        Point = point;
        Normal = normal;
        Direction = direction;
        RelativeImpactSpeed = relativeImpactSpeed;
        ImpactEnergy = impactEnergy;
        HitWeakPoint = hitWeakPoint;
    }
}
