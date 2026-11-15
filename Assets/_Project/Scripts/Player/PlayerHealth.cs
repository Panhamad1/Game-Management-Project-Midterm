using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Universal interface for damage receivers supporting origin tracking and attack deduplication.
/// </summary>
public interface IDamageReceiver
{
    void TakeDamage(float amount, string source = "Unknown", int attackId = 0);
}

/// <summary>
/// Authoritative player health and combat damage pipeline.
/// Provides attack ID deduplication, combat feedback events, procedural screen-space red flash,
/// health HUD rendering, and clean state restoration on death/respawn.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageReceiver
{
    [Header("Health Settings")]
    [SerializeField, Min(10f)] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityWindow = 0.05f; // Short window to prevent physics engine double-ticks

    [Header("Debug Status")]
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;

    // Deduplication tracking: attackId -> last hit timestamp
    private readonly Dictionary<int, float> recentAttackIds = new Dictionary<int, float>();
    private float lastAnyDamageTime = -10f;
    private float damageFlashAlpha = 0f;

    // References
    private PlayerMovement playerMovement;
    private PullAbility pullAbility;
    private PushAbility pushAbility;
    private SlowMotionAbility slowMoAbility;
    private MouseLook mouseLook;
    private Rigidbody rb;

    // Public Events
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<float, string, int> OnDamaged; // (amount, source, attackId)
    public event Action OnPlayerDied;
    public event Action OnPlayerRespawned;

    // Public Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public float HealthPercent => Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth));

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        pullAbility = GetComponentInChildren<PullAbility>();
        pushAbility = GetComponentInChildren<PushAbility>();
        slowMoAbility = GetComponentInChildren<SlowMotionAbility>();
        mouseLook = GetComponentInChildren<MouseLook>() ?? FindAnyObjectByType<MouseLook>();
    }

    private void Update()
    {
        if (damageFlashAlpha > 0f)
        {
            damageFlashAlpha = Mathf.MoveTowards(damageFlashAlpha, 0f, 2.5f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Backward-compatible overload for legacy / enemy test health callers.
    /// </summary>
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, "Generic_Damage", 0);
    }

    /// <summary>
    /// Primary authoritative damage entrypoint.
    /// </summary>
    public void TakeDamage(float amount, string source = "Unknown", int attackId = 0)
    {
        if (isDead || amount <= 0f) return;

        // 1. Attack ID Deduplication (prevents multi-hit frame bugs without a broad invulnerability shield)
        if (attackId != 0)
        {
            if (recentAttackIds.TryGetValue(attackId, out float lastHitTime))
            {
                if (Time.time - lastHitTime < 0.35f)
                {
                    // Deduplicated: this attack/projectile already damaged the player within 350ms
                    return;
                }
            }
            recentAttackIds[attackId] = Time.time;
        }
        else
        {
            // For attackId == 0, apply micro-invulnerability to debounce identical frame collider sweeps
            if (Time.time - lastAnyDamageTime < invulnerabilityWindow)
            {
                return;
            }
        }

        lastAnyDamageTime = Time.time;
        PruneOldAttackIds();

        // 2. Apply Health Reduction
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        damageFlashAlpha = Mathf.Clamp(damageFlashAlpha + (amount / maxHealth) * 0.85f, 0.25f, 0.75f);

        Debug.Log($"[PlayerHealth] Damage: -{amount:F1} from [{source}] (AttackID: {attackId}). HP: {currentHealth:F0}/{maxHealth:F0}", this);

        OnDamaged?.Invoke(amount, source, attackId);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Camera impact punch shake
        if (mouseLook != null)
        {
            float shakeMagnitude = Mathf.Clamp(amount / 35.0f, 0.15f, 0.45f);
            mouseLook.AddScreenShake(shakeMagnitude, UnityEngine.Random.insideUnitSphere.normalized);
        }

        // 3. Check Lethal Damage
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Restores health up to maxHealth.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Vampiric kill reward: heals the player based on enemy archetype.
    /// </summary>
    public void HealOnKill(string enemyType, Vector3 enemyPosition)
    {
        if (isDead) return;

        float healAmount;
        switch (enemyType.ToLowerInvariant())
        {
            case "swarmer":
                healAmount = 10f;
                break;
            case "sentry":
                healAmount = 18f;
                break;
            case "sniper":
                healAmount = 30f;
                break;
            case "hulkleaper":
            case "hulk":
                healAmount = 45f;
                break;
            default:
                healAmount = 15f;
                break;
        }

        Heal(healAmount);
        FloatingDamageNumber.SpawnHeal(enemyPosition + Vector3.up * 1.2f, healAmount);
    }

    /// <summary>
    /// Handles lethal damage: fires events, resets all ability states, and repositions the player.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.LogWarning("[PlayerHealth] Player killed! Executing clean ability and position reset...", this);
        OnPlayerDied?.Invoke();

        // 1. Reset Traversal / Grapple / Prop Carry
        if (pullAbility != null)
        {
            pullAbility.ResetState();
        }

        // 2. Deactivate Focus / Time Dilation
        if (slowMoAbility != null)
        {
            slowMoAbility.DeactivateSlowMotion();
        }

        // 3. Reset Physics Velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 4. Respawn Player at safe checkpoint
        if (playerMovement != null)
        {
            playerMovement.RespawnAtCheckpoint();
        }

        // 5. Restore Health to full
        currentHealth = maxHealth;
        isDead = false;
        recentAttackIds.Clear();
        damageFlashAlpha = 0f;

        OnPlayerRespawned?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Cleans up stale attack IDs from the tracking dictionary to prevent memory buildup.
    /// </summary>
    private void PruneOldAttackIds()
    {
        if (recentAttackIds.Count < 32) return;

        float cutoff = Time.time - 2.0f;
        List<int> staleKeys = null;

        foreach (var kvp in recentAttackIds)
        {
            if (kvp.Value < cutoff)
            {
                if (staleKeys == null) staleKeys = new List<int>();
                staleKeys.Add(kvp.Key);
            }
        }

        if (staleKeys != null)
        {
            for (int i = 0; i < staleKeys.Count; i++)
            {
                recentAttackIds.Remove(staleKeys[i]);
            }
        }
    }

    private void OnGUI()
    {
        // 1. Full-screen Red Flash Vignette on Damage
        if (damageFlashAlpha > 0.01f)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0.9f, 0.05f, 0.05f, damageFlashAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        // 2. Sleek Tactical Health HUD (Bottom Left)
        float barWidth = 220f;
        float barHeight = 18f;
        float startX = 30f;
        float startY = Screen.height - 50f;

        // Background box
        GUI.Box(new Rect(startX - 5, startY - 24, barWidth + 10, barHeight + 32), "");

        // Label
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.fontSize = 12;
        textStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(startX, startY - 22, barWidth, 20), $"HP: {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}", textStyle);

        // Health bar background
        Color oldGuiColor = GUI.color;
        GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        GUI.DrawTexture(new Rect(startX, startY, barWidth, barHeight), Texture2D.whiteTexture);

        // Health fill
        float fillWidth = barWidth * HealthPercent;
        Color healthColor = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.85f, 0.4f), HealthPercent);
        GUI.color = healthColor;
        GUI.DrawTexture(new Rect(startX, startY, fillWidth, barHeight), Texture2D.whiteTexture);
        GUI.color = oldGuiColor;
    }
}
