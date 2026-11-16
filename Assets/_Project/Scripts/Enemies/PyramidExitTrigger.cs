using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class PyramidExitTrigger : MonoBehaviour
{
    [SerializeField] private PyramidWaveEncounter encounter;

    private void OnTriggerEnter(Collider other)
    {
        if (encounter != null) encounter.TryFinish(other.GetComponentInParent<PlayerHealth>());
    }
}
