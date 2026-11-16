using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(100)]
public sealed class PyramidWaveEncounter : MonoBehaviour
{
    private const int StartingLives = 2;
    [SerializeField] private PlayerHealth player;
    [SerializeField] private GameObject exitDoor;
    [SerializeField] private BoxCollider exitThreshold;
    [SerializeField] private GameObject swarmer;
    [SerializeField] private GameObject sentry;
    [SerializeField] private GameObject hulk;
    [SerializeField] private GameObject sniper;
    [SerializeField] private Material crateMaterial;

    // Columns follow the prefab order in StartWave.
    private static readonly int[,] Waves =
    {
        { 0, 0, 0, 3 },
        { 0, 1, 0, 4 },
        { 0, 1, 1, 5 },
        { 1, 2, 1, 5 },
        { 2, 2, 2, 6 }
    };

    private static readonly Vector2[] EnemySlots =
    {
        new Vector2(32f, -3f), new Vector2(33f, -23f), new Vector2(29f, -8f),
        new Vector2(28f, -18f), new Vector2(39f, -5f), new Vector2(39f, -21f),
        new Vector2(22f, -18f), new Vector2(22f, -9f), new Vector2(34f, -13f),
        new Vector2(18f, -5f), new Vector2(19f, -22f), new Vector2(26f, -13f),
        new Vector2(14f, -9f), new Vector2(14f, -19f), new Vector2(37f, -17f),
        new Vector2(37f, -9f), new Vector2(26f, -4f), new Vector2(26f, -23f)
    };

    private readonly List<(GameObject body, IEnemyHealthStatus health, float height)> activeEnemies =
        new List<(GameObject body, IEnemyHealthStatus health, float height)>();
    private Camera playerCamera;
    private int lives = StartingLives;
    private int wave;
    private int floorMask;
    private float nextWaveAt;
    private bool fighting;
    private bool complete;
    private bool escaped;
    private bool defeated;
    private bool spawnBlocked;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private PlayerAbilityInput playerInput;
    private bool started;
    private bool ownsPause;
    private bool paused;
    private int lastPauseToggleFrame = -1;
    private bool escapeEventHeld;
    private static bool restartOnLoad;
    private float previousTimeScale;
    private double startedAt;
    private double pausedAt;
    private float bestTime;
    private float finishTime;
    private bool recordBeaten;
    private const string BestTimeKey = "REBOUND.Pyramid.BestSeconds";

    public void TryFinish(PlayerHealth entrant)
    {
        if (!complete || escaped || defeated || entrant == null || entrant != player ||
            !exitThreshold.bounds.Contains(entrant.transform.position)) return;

        escaped = true;
        finishTime = (float)(Time.realtimeSinceStartupAsDouble - startedAt);
        if (bestTime <= 0f || finishTime < bestTime)
        {
            recordBeaten = true;
            bestTime = finishTime;
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            PlayerPrefs.Save();
        }

        player.GetComponentInChildren<SlowMotionAbility>()?.DeactivateSlowMotion();
        PauseForMenu();
    }

    private void Start()
    {
        if (player == null || exitDoor == null || exitThreshold == null ||
            swarmer == null || sentry == null || hulk == null || sniper == null)
        {
            Debug.LogError("Pyramid waves require the player, exit door, exit threshold, and all four enemy prefabs.", this);
            enabled = false;
            return;
        }

        floorMask = LayerMask.GetMask("Default", "Environment");
        exitDoor.SetActive(true);
        player.OnPlayerDied += HandleLifeLost;
        playerInput = player.GetComponent<PlayerAbilityInput>();
        playerCamera = player.GetComponentInChildren<Camera>();
        bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        if (restartOnLoad)
        {
            restartOnLoad = false;
            BeginRun();
            return;
        }
        PauseForMenu();
    }

    private void BeginRun()
    {
        started = true;
        startedAt = Time.realtimeSinceStartupAsDouble;
        ResumeControls();
        nextWaveAt = Time.time + 3f;
        SpawnSupply(new Vector2(9f, -10f));
        SpawnSupply(new Vector2(9f, -18f));
        SpawnSupply(new Vector2(16f, -14f));
        SpawnSupply(new Vector2(24f, -14f));
    }

    private void OnDestroy()
    {
        if (player != null) player.OnPlayerDied -= HandleLifeLost;
        RestoreTime();
    }

    private void PauseForMenu()
    {
        if (playerInput != null) playerInput.enabled = false;
        previousTimeScale = Time.timeScale;
        ownsPause = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreTime()
    {
        if (!ownsPause) return;
        ownsPause = false;
        Time.timeScale = previousTimeScale;
    }

    private void ResumeControls()
    {
        RestoreTime();
        if (playerInput != null) playerInput.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ResumeRun()
    {
        startedAt += Time.realtimeSinceStartupAsDouble - pausedAt;
        paused = false;
        ResumeControls();
    }

    private void TogglePause()
    {
        if (!started || defeated || escaped || lastPauseToggleFrame == Time.frameCount) return;
        lastPauseToggleFrame = Time.frameCount;
        if (paused)
        {
            ResumeRun();
            return;
        }

        paused = true;
        pausedAt = Time.realtimeSinceStartupAsDouble;
        PauseForMenu();
    }

    private void Update()
    {
        if (!started || defeated || escaped) return;
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            TogglePause();
            return;
        }
        if (paused) return;
        if (complete)
        {
            // Use the player's position as a fallback if a fast dash skips the trigger callback.
            TryFinish(player);
            return;
        }
        if (!fighting)
        {
            if (Time.time >= nextWaveAt) StartWave();
            return;
        }

        activeEnemies.RemoveAll(enemy => enemy.body == null);
        if (activeEnemies.Count > 0) return;

        fighting = false;
        wave++;
        if (wave == Waves.GetLength(0))
        {
            complete = true;
            exitDoor.SetActive(false);
        }
        else
        {
            nextWaveAt = Time.time + 3f;
        }
    }

    private void StartWave()
    {
        GameObject[] prefabs = { sniper, sentry, hulk, swarmer };
        var placements = new List<(GameObject prefab, Vector3 position)>();
        int slot = 0;
        for (int kind = 0; kind < prefabs.Length; kind++)
        {
            for (int count = 0; count < Waves[wave, kind]; count++)
            {
                bool flying = prefabs[kind] == swarmer;
                Vector3 position = default;
                bool found = false;
                while (slot < EnemySlots.Length && !found)
                {
                    found = TryFindFloor(EnemySlots[slot++], out position) &&
                        Vector3.Distance(position, player.transform.position) >= 4f &&
                        !Physics.CheckSphere(position + Vector3.up * 1.4f, flying ? 0.45f : 0.8f,
                            floorMask, QueryTriggerInteraction.Ignore);
                }
                if (!found)
                {
                    if (!spawnBlocked) Debug.LogWarning("The pyramid wave is waiting for clear spawn positions.", this);
                    spawnBlocked = true;
                    nextWaveAt = Time.time + 2f;
                    return;
                }
                placements.Add((prefabs[kind], position + Vector3.up * (flying ? 1f : 0.06f)));
            }
        }

        foreach (var placement in placements)
        {
            Vector3 facing = Vector3.ProjectOnPlane(player.transform.position - placement.position, Vector3.up);
            GameObject enemy = Instantiate(placement.prefab, placement.position, Quaternion.LookRotation(facing), transform);
            float height = placement.prefab == swarmer ? 0.65f
                : placement.prefab == hulk ? 3.25f
                : placement.prefab == sniper ? 2.2f : 1.95f;
            activeEnemies.Add((enemy, enemy.GetComponent<IEnemyHealthStatus>(), height));
        }
        spawnBlocked = false;
        fighting = true;
    }

    private void HandleLifeLost()
    {
        if (!started || escaped || defeated) return;

        lives--;
        if (lives == 0)
        {
            defeated = true;
            player.GetComponentInChildren<SlowMotionAbility>()?.DeactivateSlowMotion();
            PauseForMenu();
            return;
        }
        player.RefillAfterLifeLost();
    }

    private bool TryFindFloor(Vector2 point, out Vector3 position)
    {
        bool hit = Physics.Raycast(new Vector3(point.x, 3.8f, point.y), Vector3.down, out RaycastHit floor,
            5f, floorMask, QueryTriggerInteraction.Ignore);
        position = floor.point;
        return hit && floor.normal.y >= 0.7f;
    }

    private void SpawnSupply(Vector2 point)
    {
        int layer = LayerMask.NameToLayer("Interactables");
        if (layer < 0 || !TryFindFloor(point, out Vector3 floor)) return;
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Pyramid Combat Crate";
        crate.layer = layer;
        crate.transform.SetParent(transform);
        crate.transform.position = floor + Vector3.up * 0.68f;
        crate.transform.localScale = Vector3.one * 1.25f;
        if (crateMaterial != null) crate.GetComponent<Renderer>().sharedMaterial = crateMaterial;
        crate.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        crate.AddComponent<InteractableProp>();
    }

    private void OnGUI()
    {
        if (Event.current.keyCode == KeyCode.Escape)
        {
            if (Event.current.type == EventType.KeyUp) escapeEventHeld = false;
            else if (Event.current.type == EventType.KeyDown && started && !defeated && !escaped)
            {
                if (!escapeEventHeld) TogglePause();
                escapeEventHeld = true;
                Event.current.Use();
            }
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
            titleStyle = new GUIStyle(labelStyle) { fontSize = 32, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = new Color(1f, 0.78f, 0.40f);
        }
        if (!started || escaped || defeated || paused)
        {
            DrawMenu();
            return;
        }

        DrawEnemyHealth();
        string message;
        if (complete) message = "Wave 5 cleared  •  Go through the exit";
        else if (fighting) message = $"Wave {wave + 1} / {Waves.GetLength(0)}  •  {activeEnemies.Count} remaining";
        else if (spawnBlocked) message = $"Wave {wave + 1}  •  Waiting for a clear spawn";
        else
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, nextWaveAt - Time.time));
            message = $"Wave {wave + 1} / {Waves.GetLength(0)}  •  Starts in {seconds}";
        }
        Rect badge = new Rect((Screen.width - 360f) * 0.5f, 20f, 360f, 40f);
        GUI.Box(badge, GUIContent.none);
        GUI.Label(badge, message, labelStyle);
        GUI.Label(new Rect(badge.x, badge.yMax + 4f, badge.width, 28f),
            $"Lives {lives} / {StartingLives}   •   {FormatTime((float)(Time.realtimeSinceStartupAsDouble - startedAt))}", labelStyle);
    }

    private void DrawEnemyHealth()
    {
        if (Event.current.type != EventType.Repaint || playerCamera == null) return;

        foreach (var enemy in activeEnemies)
        {
            if (enemy.body == null || enemy.health == null || enemy.health.HealthPercent <= 0f) continue;
            Vector3 point = enemy.body.transform.position + Vector3.up * enemy.height;
            Vector3 screen = playerCamera.WorldToScreenPoint(point);
            if (screen.z <= 0f || screen.x < 0f || screen.x > Screen.width ||
                screen.y < 0f || screen.y > Screen.height) continue;
            if (Physics.Linecast(playerCamera.transform.position, point, floorMask, QueryTriggerInteraction.Ignore)) continue;

            float width = Mathf.Clamp(90f / Mathf.Sqrt(screen.z), 30f, 62f);
            Rect bar = new Rect(screen.x - width * 0.5f, Screen.height - screen.y, width, 7f);
            Color original = GUI.color;
            GUI.color = new Color(0.09f, 0.04f, 0.025f, 0.9f);
            GUI.DrawTexture(bar, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(new Color(0.88f, 0.15f, 0.07f), new Color(0.96f, 0.68f, 0.20f),
                enemy.health.HealthPercent);
            GUI.DrawTexture(new Rect(bar.x + 1f, bar.y + 1f, (bar.width - 2f) * enemy.health.HealthPercent, 5f),
                Texture2D.whiteTexture);
            GUI.color = original;
        }
    }

    private void DrawMenu()
    {
        Color originalColor = GUI.color;
        GUI.color = new Color(0.065f, 0.041f, 0.024f, 0.87f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = originalColor;

        float width = Mathf.Min(420f, Screen.width - 32f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - 270f) * 0.5f, width, 270f);
        GUI.Box(panel, GUIContent.none);
        string title = paused ? "PAUSED" : escaped ? "VICTORY" : defeated ? "GAME OVER" : "REBOUND PYRAMID";
        GUI.Label(new Rect(panel.x, panel.y + 23f, width, 46f), title, titleStyle);
        if (escaped)
        {
            GUI.Label(new Rect(panel.x, panel.y + 72f, width, 32f),
                (recordBeaten ? "New record  " : "Finish time  ") + FormatTime(finishTime), labelStyle);
        }
        else if (defeated)
        {
            GUI.Label(new Rect(panel.x, panel.y + 72f, width, 32f), "You ran out of lives.", labelStyle);
        }
        GUI.Label(new Rect(panel.x, panel.y + 108f, width, 30f),
            bestTime > 0f ? "Best time  " + FormatTime(bestTime) : "Best time  —", labelStyle);

        float buttonWidth = Mathf.Min(200f, width - 32f);
        float buttonX = panel.center.x - buttonWidth * 0.5f;
        if (paused)
        {
            if (GUI.Button(new Rect(buttonX, panel.y + 130f, buttonWidth, 35f), "Resume")) ResumeRun();
            if (GUI.Button(new Rect(buttonX, panel.y + 172f, buttonWidth, 35f), "Restart"))
            {
                restartOnLoad = true;
                ReloadScene();
            }
            if (GUI.Button(new Rect(buttonX, panel.y + 214f, buttonWidth, 35f), "Exit")) ExitGame();
            return;
        }
        bool runEnded = escaped || defeated;
        if (GUI.Button(new Rect(buttonX, panel.y + 155f, buttonWidth, 40f), runEnded ? "Play Again" : "Play"))
        {
            if (runEnded) ReloadScene();
            else BeginRun();
        }

        if (GUI.Button(new Rect(buttonX, panel.y + 207f, buttonWidth, 40f), "Exit")) ExitGame();
    }

    private void ReloadScene()
    {
        RestoreTime();
        SceneManager.LoadScene(gameObject.scene.path);
    }

    private void ExitGame()
    {
        RestoreTime();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static string FormatTime(float seconds)
    {
        int hundredths = Mathf.FloorToInt(seconds * 100f);
        return $"{hundredths / 6000:00}:{hundredths / 100 % 60:00}.{hundredths % 100:00}";
    }
}
