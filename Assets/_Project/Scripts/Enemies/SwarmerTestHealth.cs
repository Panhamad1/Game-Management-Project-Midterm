using UnityEngine;

// Temporary health receiver for the enemy test encounter.
public sealed class SwarmerTestHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    public float Health { get; private set; }

    private void Awake()
    {
        Health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        Health = Mathf.Max(0f, Health - amount);
        Debug.Log($"Enemy hit: {Health:0}/{maxHealth:0} test HP", this);
        if (Health > 0f) return;

        GetComponent<PlayerMovement>()?.RespawnAtCheckpoint();
        Health = maxHealth;
    }
}
