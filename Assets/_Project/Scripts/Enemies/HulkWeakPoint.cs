using UnityEngine;

public sealed class HulkWeakPoint : MonoBehaviour, IKineticTarget
{
    private HulkLeaperEnemy hulk;

    private void Awake() => hulk = GetComponentInParent<HulkLeaperEnemy>();

    public bool CanBePulled => false;
    public bool CanBePushed => false;
    public void OnPushed(Vector3 direction, float force, Vector3 point) { }
    public void OnPulled(Transform origin, float force) { }
    public void OnPlayerSlideRam(float speed) { }
    public void OnPhysicsCrush(float speed, GameObject prop) => hulk.HitExposedCore(speed, prop);
}
