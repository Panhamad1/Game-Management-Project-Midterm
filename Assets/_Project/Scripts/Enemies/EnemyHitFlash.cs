using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a high-impact emissive red flash across enemy renderers when taking damage or kinetic impacts.
/// Uses MaterialPropertyBlock to prevent runtime material instancing and GC allocations.
/// </summary>
public class EnemyHitFlash : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private readonly List<Renderer> targetRenderers = new List<Renderer>();
    private MaterialPropertyBlock flashBlock;
    private MaterialPropertyBlock defaultBlock;
    private Coroutine flashCoroutine;

    private static readonly Color FlashRed = new Color(2.0f, 0.2f, 0.2f, 1.0f);

    private void Awake()
    {
        flashBlock = new MaterialPropertyBlock();
        defaultBlock = new MaterialPropertyBlock();

        flashBlock.SetColor(BaseColorId, FlashRed);
        flashBlock.SetColor(ColorId, FlashRed);
        flashBlock.SetColor(EmissionColorId, FlashRed);

        RefreshRenderers();
    }

    public void RefreshRenderers()
    {
        targetRenderers.Clear();
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            if (rend is LineRenderer || rend is TrailRenderer) continue;
            targetRenderers.Add(rend);
        }
    }

    /// <summary>
    /// Flashes all enemy meshes bright emissive red for the specified duration.
    /// </summary>
    public void Flash(float duration = 0.12f)
    {
        if (!gameObject.activeInHierarchy) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(DoFlash(duration));
    }

    private IEnumerator DoFlash(float duration)
    {
        for (int i = 0; i < targetRenderers.Count; i++)
        {
            if (targetRenderers[i] != null)
            {
                targetRenderers[i].SetPropertyBlock(flashBlock);
            }
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            if (targetRenderers[i] != null)
            {
                targetRenderers[i].SetPropertyBlock(null);
            }
        }
        flashCoroutine = null;
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        for (int i = 0; i < targetRenderers.Count; i++)
        {
            if (targetRenderers[i] != null)
            {
                targetRenderers[i].SetPropertyBlock(null);
            }
        }
    }
}
