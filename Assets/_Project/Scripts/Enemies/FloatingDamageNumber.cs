using UnityEngine;

/// <summary>
/// Rising, fading 3D damage number that pops in world-space on enemy hit or kinetic impact.
/// Billboarded to face the player's camera.
/// </summary>
public class FloatingDamageNumber : MonoBehaviour
{
    private TextMesh textMesh;
    private Camera playerCam;
    private float lifetime = 0.85f;
    private float timer = 0f;
    private Vector3 velocity;
    private Color baseColor;

    public static void Spawn(Vector3 worldPos, float damage, bool isCrit = false)
    {
        GameObject go = new GameObject("DamageNumber_" + Mathf.RoundToInt(damage));
        // Offset slightly upwards and random horizontal jitter so overlapping hits fan out
        Vector3 jitter = new Vector3(Random.Range(-0.25f, 0.25f), 0.35f, Random.Range(-0.25f, 0.25f));
        go.transform.position = worldPos + jitter;

        FloatingDamageNumber num = go.AddComponent<FloatingDamageNumber>();
        num.Initialize(damage, isCrit);
    }

    public static void SpawnHeal(Vector3 worldPos, float healAmount)
    {
        GameObject go = new GameObject("HealNumber_" + Mathf.RoundToInt(healAmount));
        Vector3 jitter = new Vector3(Random.Range(-0.2f, 0.2f), 0.45f, Random.Range(-0.2f, 0.2f));
        go.transform.position = worldPos + jitter;

        FloatingDamageNumber num = go.AddComponent<FloatingDamageNumber>();
        num.InitializeHeal(healAmount);
    }

    private void InitializeHeal(float healAmount)
    {
        playerCam = Camera.main;

        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.characterSize = 0.09f;
        textMesh.fontSize = 44;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        textMesh.text = $"+{healAmount:F0} HP";
        baseColor = new Color(0.15f, 1.0f, 0.4f, 1.0f); // Bright emerald green
        transform.localScale = Vector3.one * 1.3f;

        textMesh.color = baseColor;
        velocity = new Vector3(Random.Range(-0.3f, 0.3f), 2.8f, Random.Range(-0.3f, 0.3f));
    }

    private void Initialize(float damage, bool isCrit)
    {
        playerCam = Camera.main;

        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.characterSize = 0.08f;
        textMesh.fontSize = 42;
        textMesh.fontStyle = isCrit ? FontStyle.BoldAndItalic : FontStyle.Bold;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        string dmgText = damage >= 1000f ? $"{damage / 1000f:F1}k" : $"{damage:F0}";
        if (isCrit)
        {
            textMesh.text = $"{dmgText}!";
            baseColor = new Color(1.0f, 0.85f, 0.1f, 1.0f); // Bright Gold/Amber for Crits
            transform.localScale = Vector3.one * 1.35f;
        }
        else if (damage >= 40f)
        {
            textMesh.text = dmgText;
            baseColor = new Color(1.0f, 0.25f, 0.1f, 1.0f); // Bright Orange-Red for heavy hits
            transform.localScale = Vector3.one * 1.15f;
        }
        else
        {
            textMesh.text = dmgText;
            baseColor = new Color(1.0f, 1.0f, 1.0f, 0.95f); // Clean White for standard hits
            transform.localScale = Vector3.one;
        }

        textMesh.color = baseColor;
        // Float upwards with slight deceleration
        velocity = new Vector3(Random.Range(-0.4f, 0.4f), 2.4f, Random.Range(-0.4f, 0.4f));
    }

    private void Update()
    {
        if (playerCam == null) playerCam = Camera.main;
        if (playerCam != null)
        {
            // Billboard face camera
            transform.rotation = Quaternion.LookRotation(transform.position - playerCam.transform.position);
        }

        timer += Time.unscaledDeltaTime;
        float progress = timer / lifetime;

        // Upward motion
        transform.position += velocity * Time.unscaledDeltaTime;
        velocity.y = Mathf.MoveTowards(velocity.y, 0.6f, 3.0f * Time.unscaledDeltaTime);

        // Alpha fade out
        if (textMesh != null)
        {
            float alpha = Mathf.Clamp01(1f - progress * progress);
            textMesh.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
