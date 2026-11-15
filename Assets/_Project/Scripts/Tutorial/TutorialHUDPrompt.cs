using UnityEngine;

/// <summary>
/// Displays persistent contextual tutorial prompts and chamber objectives in an Egyptian aesthetic.
/// Built with zero-GC immediate GUI for maximum reliability with zero external prefab dependencies.
/// </summary>
public class TutorialHUDPrompt : MonoBehaviour
{
    public static TutorialHUDPrompt Instance { get; private set; }

    [Header("Current Chamber State")]
    [SerializeField] private int chamberIndex = 1;
    [SerializeField] private string chamberTitle = "CHAMBER 1: LOCOMOTION & DASH-SLIDE";
    [SerializeField] private string chamberObjective = "Sprint forward [W+SHIFT] and press [CTRL / C] to slide under the low gate.";
    [SerializeField] private string chamberControls = "[W + SHIFT] Sprint  •  [SHIFT / Q] Dash  •  [CTRL / C] Slide";
    [SerializeField] private bool isCompleted = false;

    private float completionBannerTimer = 0f;
    private GUIStyle cardStyle;
    private GUIStyle titleStyle;
    private GUIStyle objectiveStyle;
    private GUIStyle badgeStyle;
    private GUIStyle checkStyle;
    private Texture2D cardBgTex;
    private Texture2D borderTex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CreateTextures();
    }

    private void CreateTextures()
    {
        if (cardBgTex == null)
        {
            cardBgTex = new Texture2D(1, 1);
            cardBgTex.SetPixel(0, 0, new Color(0.04f, 0.05f, 0.08f, 0.90f));
            cardBgTex.Apply();
        }

        if (borderTex == null)
        {
            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(0.88f, 0.72f, 0.25f, 0.95f)); // Egyptian Gold
            borderTex.Apply();
        }
    }

    public void SetChamber(int index, string title, string objective, string controls)
    {
        chamberIndex = index;
        chamberTitle = title;
        chamberObjective = objective;
        chamberControls = controls;
        isCompleted = false;
        completionBannerTimer = 0f;
    }

    public void SetChamberCompleted(bool completed)
    {
        if (completed && !isCompleted)
        {
            completionBannerTimer = 4.0f;
        }
        isCompleted = completed;
    }

    private void Update()
    {
        if (completionBannerTimer > 0f)
        {
            completionBannerTimer -= Time.deltaTime;
        }
    }

    private void InitStyles()
    {
        if (cardStyle != null) return;
        CreateTextures();

        cardStyle = new GUIStyle(GUI.skin.box);
        cardStyle.normal.background = cardBgTex;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 13;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.95f, 0.82f, 0.35f); // Egyptian Gold
        titleStyle.alignment = TextAnchor.MiddleLeft;

        objectiveStyle = new GUIStyle(GUI.skin.label);
        objectiveStyle.fontSize = 12;
        objectiveStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f);
        objectiveStyle.alignment = TextAnchor.MiddleLeft;
        objectiveStyle.wordWrap = true;

        badgeStyle = new GUIStyle(GUI.skin.label);
        badgeStyle.fontSize = 11;
        badgeStyle.fontStyle = FontStyle.Bold;
        badgeStyle.normal.textColor = new Color(0.35f, 0.85f, 1.0f); // Chrono Cyan
        badgeStyle.alignment = TextAnchor.MiddleRight;

        checkStyle = new GUIStyle(GUI.skin.label);
        checkStyle.fontSize = 12;
        checkStyle.fontStyle = FontStyle.Bold;
        checkStyle.normal.textColor = new Color(0.2f, 1.0f, 0.45f); // Jade Green
        checkStyle.alignment = TextAnchor.MiddleRight;
    }

    private void OnGUI()
    {
        InitStyles();

        float cardWidth = Mathf.Min(620f, Screen.width - 32f);
        float cardHeight = 72f;
        float cardX = (Screen.width - cardWidth) * 0.5f;
        float cardY = 16f;

        // Gold Border Frame (1px)
        Color oldColor = GUI.color;
        GUI.color = isCompleted ? new Color(0.2f, 1.0f, 0.45f, 0.9f) : new Color(0.88f, 0.72f, 0.25f, 0.9f);
        GUI.DrawTexture(new Rect(cardX - 1, cardY - 1, cardWidth + 2, cardHeight + 2), borderTex);

        // Dark Background Card
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(cardX, cardY, cardWidth, cardHeight), cardBgTex);

        // Header Line: Chamber Title (Left) + Status / Controls (Right)
        float padding = 10f;
        Rect titleRect = new Rect(cardX + padding, cardY + 6f, cardWidth * 0.6f, 20f);
        GUI.Label(titleRect, chamberTitle, titleStyle);

        Rect statusRect = new Rect(cardX + cardWidth * 0.55f, cardY + 6f, cardWidth * 0.45f - padding, 20f);
        if (isCompleted || completionBannerTimer > 0f)
        {
            GUI.Label(statusRect, "[ GATE UNLOCKED - MASTERED ]", checkStyle);
        }
        else
        {
            GUI.Label(statusRect, "IN PROGRESS", badgeStyle);
        }

        // Objective Description (Center)
        Rect objRect = new Rect(cardX + padding, cardY + 26f, cardWidth - padding * 2f, 22f);
        GUI.Label(objRect, chamberObjective, objectiveStyle);

        // Keybind Badges (Bottom)
        Rect controlsRect = new Rect(cardX + padding, cardY + 46f, cardWidth - padding * 2f, 20f);
        GUI.Label(controlsRect, chamberControls, badgeStyle);

        GUI.color = oldColor;
    }
}
