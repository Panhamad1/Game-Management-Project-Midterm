using UnityEngine;

// Stores the prefab pose so animation offsets never accumulate.
internal readonly struct EnemyVisualPart
{
    private readonly Transform part;
    private readonly Vector3 restPosition;
    private readonly Quaternion restRotation;

    public EnemyVisualPart(Transform parent, string name)
    {
        part = parent != null ? parent.Find(name) : null;
        restPosition = part != null ? part.localPosition : Vector3.zero;
        restRotation = part != null ? part.localRotation : Quaternion.identity;
    }

    public void Pose(Vector3 offset, Quaternion rotation)
    {
        if (part == null) return;
        part.SetLocalPositionAndRotation(restPosition + offset, restRotation * rotation);
    }

    public void Orbit(Quaternion facing, Vector3 offset, Quaternion articulation)
    {
        if (part == null) return;
        part.SetLocalPositionAndRotation(facing * restPosition + offset, facing * restRotation * articulation);
    }

    public void Swing(Vector3 offset, Vector3 pivot, float angle)
    {
        if (part == null) return;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.right);
        part.SetLocalPositionAndRotation(pivot + rotation * (restPosition - pivot) + offset,
            rotation * restRotation);
    }
}
