using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tactical HUD:
/// - Dynamic cyan crosshair brackets that lock on targets
/// - Target Readout Badge: displays object name, weight class, and distance
/// - Electric Cyan Focus Vision pulse (replaces dark horror vignette)
/// - Bottom energy gauge
/// </summary>
public class AbilityHUD : MonoBehaviour
{
    [Header("Crosshair Elements")]
    [SerializeField] private Image crosshairDot;
    [SerializeField] private Image[] crosshairBrackets;
    [SerializeField] private Color defaultCrosshairColor = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField] private Color targetAcquiredColor = new Color(0.1f, 0.95f, 1.0f, 1.0f);
    [SerializeField] private Color grabObjectColor = new Color(1.0f, 0.78f, 0.12f, 1.0f); // Amber / Yellow for Crates
    [SerializeField] private Color grappleAnchorColor = new Color(0.05f, 0.95f, 1.0f, 1.0f); // Electric Cyan for Walls/Towers
    [SerializeField] private Color crateCannonColor = new Color(0.15f, 1.0f, 0.4f, 1.0f); // Neon Green for Armed Crate Launch

    [Header("Target Readout Badge")]
    [SerializeField] private GameObject targetBadgeRoot;
    [SerializeField] private Text targetBadgeText;

    [Header("Focus Vision (Clear Sight)")]
    [SerializeField] private Image focusOverlay;
    [SerializeField] [Range(0f, 1f)] private float maxFocusAlpha = 0.0f; // Disabled to ensure 100% crisp, unobstructed visibility

    [Header("Gauges")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private Image pushCooldownRing;
    [SerializeField] private Text speedometerText;
    [SerializeField] private Text controlsGuideText;

    [Header("References")]
    [SerializeField] private PushAbility pushAbility;
    [SerializeField] private PullAbility pullAbility;
    [SerializeField] private SlowMotionAbility slowMotionAbility;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Camera playerCamera;

    private float currentFocusAlpha = 0f;
    private InteractableProp currentTargetProp = null;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerMovement == null) playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (pushAbility == null) pushAbility = FindAnyObjectByType<PushAbility>();
        if (pullAbility == null) pullAbility = FindAnyObjectByType<PullAbility>();
        if (slowMotionAbility == null) slowMotionAbility = FindAnyObjectByType<SlowMotionAbility>();

        if (controlsGuideText != null)
        {
            controlsGuideText.text = "[SHIFT] Sprint   [C] Slide   [SPACE] B-Hop/Fling   [LMB] Push/Boost   [RMB] Grapple/Grab   [Q/F] Focus";
        }

        SetupCyanFocusTexture();
    }

    private void SetupCyanFocusTexture()
    {
        if (focusOverlay == null || focusOverlay.sprite != null) return;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxRadius = size * 0.5f;

        Color cyanColor = new Color(0.05f, 0.75f, 1.0f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                // Soft edge glow: transparent in center, glowing cyan towards corners
                float alpha = Mathf.SmoothStep(0.45f, 1.0f, dist);
                tex.SetPixel(x, y, new Color(cyanColor.r, cyanColor.g, cyanColor.b, alpha));
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        focusOverlay.sprite = sprite;
        focusOverlay.type = Image.Type.Simple;

        Color c = cyanColor;
        c.a = 0f;
        focusOverlay.color = c;
    }

    private void Update()
    {
        UpdateCrosshairTargeting();
        UpdateFocusVision();
        UpdateGauges();
    }

    private void UpdateCrosshairTargeting()
    {
        bool showBadge = false;
        InteractableProp propUnderCrosshair = null;
        string badgeInfo = "";
        Color activeColor = defaultCrosshairColor;
        float bracketDistance = 14f;

        // Priority 0: Deflectable Projectile in parry range
        if (pushAbility != null && pushAbility.HasDeflectableTargetInSight(out _))
        {
            showBadge = true;
            activeColor = targetAcquiredColor; // Electric Cyan
            bracketDistance = 24f;
            badgeInfo = "[LMB] DEFLECT PROJECTILE";
        }
        // Priority 1: Carrying an object (Armed Crate Cannon)
        else if (pullAbility != null && pullAbility.IsCarrying)
        {
            showBadge = true;
            activeColor = crateCannonColor;
            bracketDistance = 22f;
            string objClass = pullAbility.HeldProp != null ? pullAbility.HeldProp.Class.ToString().ToUpper() : "OBJECT";
            badgeInfo = pullAbility.HeldProp != null && pullAbility.HeldProp.CanBeLaunched
                ? $"[LMB] FIRE CRATE CANNON ({objClass})"
                : $"CARRYING {objClass}";
        }
        else if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            // Priority 2: Check for Grab Targets (Props, Crates, TargetButtons) within 45m
            LayerMask targetMask = LayerMask.GetMask("Interactables", "Enemies", "Projectiles");
            if (targetMask.value == 0) targetMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

            bool hasPropTarget = Physics.Raycast(ray, out RaycastHit propHit, 45f, targetMask, QueryTriggerInteraction.Ignore);
            if (!hasPropTarget)
            {
                Ray assistRay = new Ray(ray.origin + ray.direction * 1.5f, ray.direction);
                hasPropTarget = Physics.SphereCast(assistRay, 0.35f, out propHit, 43.5f, targetMask, QueryTriggerInteraction.Ignore);
            }

            if (hasPropTarget && propHit.collider != null)
            {
                IKineticTarget target = propHit.collider.GetComponentInParent<IKineticTarget>();
                if (target != null)
                {
                    propUnderCrosshair = propHit.collider.GetComponentInParent<InteractableProp>();
                    TargetButton button = propHit.collider.GetComponentInParent<TargetButton>();

                    float dist = Vector3.Distance(ray.origin, propHit.point);
                    showBadge = true;
                    activeColor = grabObjectColor; // Vibrant Amber / Yellow
                    bracketDistance = 20f; // Expand into open box

                    if (propUnderCrosshair != null)
                    {
                        string verb = propUnderCrosshair.Class == WeightClass.Heavy ? "DRAG" : "GRAB";
                        badgeInfo = $"[RMB] {verb} · {propUnderCrosshair.name.ToUpper()} ({propUnderCrosshair.Class.ToString().ToUpper()}) · {dist:F1}m";
                    }
                    else if (button != null)
                    {
                        badgeInfo = $"TARGET SWITCH · {button.ButtonName.ToUpper()} · {dist:F1}m";
                    }
                    else
                    {
                        badgeInfo = $"[RMB] GRAB · {propHit.collider.gameObject.name.ToUpper()} · {dist:F1}m";
                    }
                }
            }

            // Priority 3: If no grab object, check for Grapple Anchors (Walls, Floor, Towers, Ceiling) within 75m
            if (!showBadge)
            {
                LayerMask grappleMask = LayerMask.GetMask("Environment", "Default", "GrappleAnchors");
                if (grappleMask.value == 0) grappleMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

                bool hasGrappleHit = Physics.Raycast(ray, out RaycastHit grappleHit, 75f, grappleMask, QueryTriggerInteraction.Ignore);
                if (!hasGrappleHit)
                {
                    Ray assistGrappleRay = new Ray(ray.origin + ray.direction * 2.0f, ray.direction);
                    hasGrappleHit = Physics.SphereCast(assistGrappleRay, 0.25f, out grappleHit, 73f, grappleMask, QueryTriggerInteraction.Ignore);
                }

                if (hasGrappleHit && grappleHit.collider != null)
                {
                    float grappleDist = Vector3.Distance(ray.origin, grappleHit.point);
                    if (grappleDist >= 4.0f)
                    {
                        showBadge = true;
                        activeColor = grappleAnchorColor; // Electric Cyan
                        bracketDistance = 9.5f; // Contract into sharp diamond / cross
                        badgeInfo = $"[RMB] GRAPPLE · {grappleDist:F1}m";
                    }
                }
            }
        }

        // Highlight swap for interactable props
        if (propUnderCrosshair != currentTargetProp)
        {
            if (currentTargetProp != null) currentTargetProp.SetHighlighted(false);
            currentTargetProp = propUnderCrosshair;
            if (currentTargetProp != null) currentTargetProp.SetHighlighted(true);
        }

        // Crosshair Center Dot
        if (crosshairDot != null) crosshairDot.color = activeColor;

        // Crosshair Brackets: Dynamic shape morphing (Top, Bottom, Left, Right)
        if (crosshairBrackets != null && crosshairBrackets.Length >= 4)
        {
            for (int i = 0; i < crosshairBrackets.Length; i++)
            {
                if (crosshairBrackets[i] != null) crosshairBrackets[i].color = activeColor;
            }

            if (crosshairBrackets[0] != null) crosshairBrackets[0].rectTransform.anchoredPosition = new Vector2(0f, bracketDistance);
            if (crosshairBrackets[1] != null) crosshairBrackets[1].rectTransform.anchoredPosition = new Vector2(0f, -bracketDistance);
            if (crosshairBrackets[2] != null) crosshairBrackets[2].rectTransform.anchoredPosition = new Vector2(-bracketDistance, 0f);
            if (crosshairBrackets[3] != null) crosshairBrackets[3].rectTransform.anchoredPosition = new Vector2(bracketDistance, 0f);
        }

        // Target Readout Badge
        if (targetBadgeRoot != null)
        {
            targetBadgeRoot.SetActive(showBadge);
            if (targetBadgeText != null && showBadge)
            {
                targetBadgeText.text = badgeInfo;
                targetBadgeText.color = activeColor;
            }
        }
    }

    private void UpdateFocusVision()
    {
        if (focusOverlay == null) return;

        // Preserve crystal clear vision with zero screen haze
        if (maxFocusAlpha <= 0.001f)
        {
            if (focusOverlay.gameObject.activeSelf) focusOverlay.gameObject.SetActive(false);
            return;
        }

        bool isSlowMo = slowMotionAbility != null && slowMotionAbility.IsActive;
        float targetAlpha = isSlowMo ? maxFocusAlpha : 0f;

        currentFocusAlpha = Mathf.MoveTowards(currentFocusAlpha, targetAlpha, 2.5f * Time.unscaledDeltaTime);
        Color c = focusOverlay.color;
        c.a = currentFocusAlpha;
        focusOverlay.color = c;
    }

    private void UpdateGauges()
    {
        if (energyBar != null && slowMotionAbility != null)
        {
            energyBar.value = slowMotionAbility.EnergyPercent;
        }

        if (pushCooldownRing != null && pushAbility != null)
        {
            pushCooldownRing.fillAmount = pushAbility.CooldownPercent;
        }

        if (speedometerText != null && playerMovement != null)
        {
            float spd = playerMovement.CurrentSpeed;
            if (pullAbility != null && pullAbility.IsGrappling)
            {
                speedometerText.text = $"GRAPPLE · {spd:F1} m/s";
                speedometerText.color = targetAcquiredColor;
            }
            else if (playerMovement.IsSliding)
            {
                speedometerText.text = $"SLIDE · {spd:F1} m/s";
                speedometerText.color = new Color(0.2f, 1.0f, 0.6f);
            }
            else if (!playerMovement.IsGrounded && spd > 10.0f)
            {
                speedometerText.text = $"B-HOP · {spd:F1} m/s";
                speedometerText.color = targetAcquiredColor;
            }
            else if (playerMovement.IsSprinting)
            {
                speedometerText.text = $"SPRINT · {spd:F1} m/s";
                speedometerText.color = new Color(1.0f, 0.85f, 0.2f);
            }
            else
            {
                speedometerText.text = $"{spd:F1} m/s";
                speedometerText.color = spd > 12f ? targetAcquiredColor : Color.white;
            }
        }
    }
}
