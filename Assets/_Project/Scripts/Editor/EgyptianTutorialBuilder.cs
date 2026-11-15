#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Procedurally builds the 6-Chamber Linear Egyptian Parkour & Combat Tutorial Academy (180m+).
/// Features staged physical mastery gates, void pit respawn triggers, and contextual HUD prompts.
/// </summary>
public static class EgyptianTutorialBuilder
{
    public const string AcademyRootName = "Egyptian_Tutorial_Academy_180m";

    [MenuItem("Tools/REBOUND/4. Clean Scene (Delete All)", false, 3)]
    public static void CleanAllArenas()
    {
        // 1. Purge known container and root names
        string[] targetRoots = new[]
        {
            AcademyRootName, "TestRoom", "Egyptian_Parkour_Arena_100m", "Egyptian_Parkour_Arena_45m",
            "Physics_Test_Room_Chambers", "Redliner_Grapple_Arena_200m", "Interactive_Props_Cluster",
            "Enemy_Squad", "Arena_MainFloor", "Chamber1_StartHub", "StartHub_DownhillChute_North",
            "MegaSlide_Ramp_West", "SkiJump_LaunchRamp_East", "SkyDeck_Observation_North",
            "GrappleSpire_Central", "Global_PitHazardTrigger", "Barricade_Plank"
        };
        for (int i = 0; i < targetRoots.Length; i++)
        {
            DestroyIfExists(targetRoots[i]);
        }

        // 2. Scan all root GameObjects in active scene to purge loose old arena objects
        var scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject go = roots[i];
            if (go == null) continue;
            string n = go.name;

            if (n.StartsWith("GrappleTower") || n.StartsWith("Bowling_Crate") || n.StartsWith("Gantry_") ||
                n.StartsWith("Target_") || n.StartsWith("Spire_") || n == "Wall" || n.StartsWith("Wall_") ||
                n.StartsWith("Chamber") || n.StartsWith("C1_") || n.StartsWith("C2_") || n.StartsWith("C3_") ||
                n.StartsWith("C4_") || n.StartsWith("C5_") || n.StartsWith("C6_"))
            {
                Undo.DestroyObjectImmediate(go);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("REBOUND: All legacy arenas, old giant maps, towers, bowling crates, and test rooms have been cleaned from the scene!");
    }

    [MenuItem("Tools/REBOUND/1. Build Tutorial Academy", false, 0)]
    public static void BuildCompleteAcademy()
    {
        // 1. Clean previous academy and all legacy maps/test rooms
        CleanAllArenas();

        // 2. Load Egyptian Materials
        Material sandstone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Sandstone.mat");
        Material brass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Brass.mat");
        Material lapis = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/RoyalLapis.mat");
        Material obsidian = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Obsidian.mat");
        Material jade = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/ScarabJade.mat");

        int envLayer = LayerMask.NameToLayer("Environment");
        if (envLayer == -1) envLayer = 0;
        int propLayer = LayerMask.NameToLayer("Interactable");
        if (propLayer == -1) propLayer = LayerMask.NameToLayer("Interactables");
        if (propLayer == -1) propLayer = 0;

        // 3. Directional Sun
        SetupSun();

        // 4. Academy Root Container
        GameObject academyRoot = new GameObject(AcademyRootName);
        academyRoot.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(academyRoot, "Build Egyptian Tutorial Academy");

        // 5. Ensure Tutorial HUD Prompt exists
        SetupTutorialHUD(academyRoot.transform);

        // 6. Build Chambers
        BuildChamber1_Locomotion(academyRoot.transform, sandstone, brass, lapis, envLayer);
        BuildChamber2_WallRun(academyRoot.transform, sandstone, brass, jade, obsidian, envLayer);
        BuildChamber3_WallClimb(academyRoot.transform, sandstone, brass, obsidian, envLayer);
        BuildChamber4_KineticVault(academyRoot.transform, sandstone, brass, lapis, obsidian, jade, envLayer, propLayer);
        BuildChamber5_LaserCorridor(academyRoot.transform, sandstone, brass, lapis, obsidian, envLayer);
        BuildChamber6_CombatSanctum(academyRoot.transform, sandstone, brass, lapis, obsidian, jade, envLayer, propLayer);

        // 7. Global Void Kill Trigger Plane
        BuildVoidKillPlane(academyRoot.transform);

        // 8. Spawn & Position Player Rig at Chamber 1 Start
        SpawnAndPositionPlayer(new Vector3(0f, 1.2f, 2f));

        Debug.Log("REBOUND: 6-Chamber Egyptian Parkour & Combat Tutorial Academy successfully built!");
    }

    [MenuItem("Tools/REBOUND/2. Spawn Player Only", false, 1)]
    public static void SpawnPlayerOnly()
    {
        SpawnAndPositionPlayer(new Vector3(0f, 1.2f, 2f));
        Debug.Log("REBOUND: Player Rig positioned safely at (0, 1.2, 2)!");
    }

    [MenuItem("Tools/REBOUND/3. Spawn Enemies Only", false, 2)]
    public static void SpawnEnemiesOnly()
    {
        GameObject oldEnemies = GameObject.Find("Enemy_Squad");
        if (oldEnemies != null) Undo.DestroyObjectImmediate(oldEnemies);

        GameObject enemyGroup = new GameObject("Enemy_Squad");
        Undo.RegisterCreatedObjectUndo(enemyGroup, "Spawn Enemy Squad");

        GameObject swarmerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Swarmer.prefab");
        GameObject sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/RapidLaserSentry.prefab");
        GameObject sniperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Sniper.prefab");
        GameObject hulkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/HulkLeaper.prefab");

        if (swarmerPrefab != null)
        {
            GameObject s1 = (GameObject)PrefabUtility.InstantiatePrefab(swarmerPrefab, enemyGroup.transform);
            s1.transform.position = new Vector3(-3f, 1.2f, 12f);
            GameObject s2 = (GameObject)PrefabUtility.InstantiatePrefab(swarmerPrefab, enemyGroup.transform);
            s2.transform.position = new Vector3(3f, 1.2f, 12f);
        }

        if (sentryPrefab != null)
        {
            GameObject sentry = (GameObject)PrefabUtility.InstantiatePrefab(sentryPrefab, enemyGroup.transform);
            sentry.transform.position = new Vector3(0f, 1.2f, 18f);
            sentry.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (sniperPrefab != null)
        {
            GameObject sniper = (GameObject)PrefabUtility.InstantiatePrefab(sniperPrefab, enemyGroup.transform);
            sniper.transform.position = new Vector3(-6f, 1.2f, 24f);
        }

        if (hulkPrefab != null)
        {
            GameObject hulk = (GameObject)PrefabUtility.InstantiatePrefab(hulkPrefab, enemyGroup.transform);
            hulk.transform.position = new Vector3(0f, 1.2f, 15f);
        }

        Debug.Log("REBOUND: Enemy Squad (Swarmers, Sentry, Sniper, Hulk) spawned!");
    }

    private static void SetupSun()
    {
        Light sun = Object.FindFirstObjectByType<Light>();
        if (sun == null)
        {
            GameObject sunObj = new GameObject("Directional_Sun");
            Light light = sunObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.96f, 0.88f);
            light.intensity = 1.35f;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Undo.RegisterCreatedObjectUndo(sunObj, "Create Sun Light");
        }
    }

    private static void SetupTutorialHUD(Transform parent)
    {
        TutorialHUDPrompt hud = Object.FindFirstObjectByType<TutorialHUDPrompt>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("Tutorial_HUD_Prompt");
            hudObj.transform.SetParent(parent);
            hud = hudObj.AddComponent<TutorialHUDPrompt>();
            Undo.RegisterCreatedObjectUndo(hudObj, "Create Tutorial HUD");
        }
    }

    private static void BuildChamber1_Locomotion(Transform parent, Material sandstone, Material brass, Material lapis, int envLayer)
    {
        GameObject c1 = new GameObject("Chamber_1_Locomotion");
        c1.transform.SetParent(parent);

        // Start Hub Platform
        CreateBlock(c1.transform, "C1_StartPlatform", new Vector3(0f, 0f, 4f), new Vector3(12f, 1f, 10f), sandstone, envLayer);
        CreateBlock(c1.transform, "C1_StartBorder_L", new Vector3(-6f, 1.0f, 4f), new Vector3(0.6f, 1.2f, 10f), brass, envLayer);
        CreateBlock(c1.transform, "C1_StartBorder_R", new Vector3(6f, 1.0f, 4f), new Vector3(0.6f, 1.2f, 10f), brass, envLayer);

        // Checkpoint 1 Trigger
        CreateCheckpointTrigger(c1.transform, "C1_Checkpoint", new Vector3(0f, 1.2f, 2f), new Vector3(8f, 3f, 3f),
            1, "CHAMBER 1: LOCOMOTION & DASH-SLIDE",
            "Sprint forward [W+SHIFT] and press [CTRL / C] to slide under the low gate. Tap [SHIFT / Q] to dash across gaps.",
            "[W + SHIFT] Sprint  •  [SHIFT / Q] Dash  •  [CTRL / C] Slide",
            new Vector3(0f, 1.2f, 2f));

        // Stepping stone across 6m chasm
        CreateBlock(c1.transform, "C1_SteppingStone", new Vector3(0f, 0f, 12f), new Vector3(4f, 1f, 4f), sandstone, envLayer);

        // Platform 2 before slide gate
        CreateBlock(c1.transform, "C1_Platform2", new Vector3(0f, 0f, 20f), new Vector3(12f, 1f, 10f), sandstone, envLayer);

        // Low Slide Portcullis Gate
        CreateBlock(c1.transform, "C1_GatePylon_L", new Vector3(-4f, 2.5f, 25f), new Vector3(4f, 5f, 1.5f), sandstone, envLayer);
        CreateBlock(c1.transform, "C1_GatePylon_R", new Vector3(4f, 2.5f, 25f), new Vector3(4f, 5f, 1.5f), sandstone, envLayer);
        CreateBlock(c1.transform, "C1_GateLintel", new Vector3(0f, 4.5f, 25f), new Vector3(6f, 1f, 1.5f), lapis, envLayer);

        // Low sliding barrier (1.2m gap beneath lintel): player must crouch-slide or slide-ram to unlock
        GameObject slideBarrier = CreateBlock(c1.transform, "C1_SlideBarrier", new Vector3(0f, 1.4f, 25f), new Vector3(4f, 1.6f, 0.8f), brass, envLayer);
        var gateComp = slideBarrier.AddComponent<TutorialMasteryGate>();
        var unlockSlideField = typeof(TutorialMasteryGate).GetField("unlockOnPlayerSlide", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (unlockSlideField != null) unlockSlideField.SetValue(gateComp, true);

        // Slide Trigger zone right at gate
        GameObject slideTrigger = new GameObject("C1_SlideTrigger");
        slideTrigger.transform.SetParent(c1.transform);
        slideTrigger.transform.position = new Vector3(0f, 1.2f, 25f);
        BoxCollider sc = slideTrigger.AddComponent<BoxCollider>();
        sc.size = new Vector3(4f, 2.5f, 2f);
        sc.isTrigger = true;
        var trigGate = slideTrigger.AddComponent<TutorialMasteryGate>();
        if (unlockSlideField != null) unlockSlideField.SetValue(trigGate, true);
    }

    private static void BuildChamber2_WallRun(Transform parent, Material sandstone, Material brass, Material jade, Material obsidian, int envLayer)
    {
        GameObject c2 = new GameObject("Chamber_2_WallRun");
        c2.transform.SetParent(parent);

        // Entrance Platform
        CreateBlock(c2.transform, "C2_EntrancePlatform", new Vector3(0f, 0f, 31f), new Vector3(12f, 1f, 8f), sandstone, envLayer);

        // Checkpoint 2
        CreateCheckpointTrigger(c2.transform, "C2_Checkpoint", new Vector3(0f, 1.2f, 31f), new Vector3(8f, 3f, 3f),
            2, "CHAMBER 2: WALL-RUN & WALL-JUMP",
            "Sprint diagonally against the glowing wall [W+SHIFT] to lateral wall-run. Press [SPACE] to wall-jump across to the opposite wall!",
            "[W + SHIFT] Wall-Run  •  [SPACE] Wall-Jump Outward",
            new Vector3(0f, 1.2f, 31f));

        // Lateral Wall-Run Panels over 22m void chasm
        // Left Wall: Sandstone panel with glowing Jade runners
        CreateBlock(c2.transform, "C2_LeftWall", new Vector3(-4.8f, 4.5f, 43f), new Vector3(0.8f, 9f, 18f), sandstone, envLayer);
        CreateBlock(c2.transform, "C2_LeftRuneRailing", new Vector3(-4.3f, 3.5f, 43f), new Vector3(0.2f, 0.4f, 18f), jade, envLayer);

        // Right Wall: Sandstone panel with glowing Jade runners
        CreateBlock(c2.transform, "C2_RightWall", new Vector3(4.8f, 4.5f, 49f), new Vector3(0.8f, 9f, 18f), sandstone, envLayer);
        CreateBlock(c2.transform, "C2_RightRuneRailing", new Vector3(4.3f, 3.5f, 49f), new Vector3(0.2f, 0.4f, 18f), jade, envLayer);

        // Intermediate kick-pillar in middle
        CreateBlock(c2.transform, "C2_KickPillar", new Vector3(0f, 3.5f, 44f), new Vector3(2.5f, 8f, 2.5f), obsidian, envLayer);
        CreateBlock(c2.transform, "C2_KickPillarCap", new Vector3(0f, 7.8f, 44f), new Vector3(3f, 0.6f, 3f), brass, envLayer);

        // Landing Platform 3
        CreateBlock(c2.transform, "C2_LandingPlatform", new Vector3(0f, 0f, 60f), new Vector3(12f, 1f, 8f), sandstone, envLayer);

        // Mastery Gate 2
        GameObject gate2 = CreateBlock(c2.transform, "C2_MasteryGate", new Vector3(0f, 2.5f, 63f), new Vector3(8f, 5f, 1f), brass, envLayer);
        var gateComp = gate2.AddComponent<TutorialMasteryGate>();

        // Gate 2 Unlock Trigger on Landing Platform
        GameObject trigObj = new GameObject("C2_UnlockTrigger");
        trigObj.transform.SetParent(c2.transform);
        trigObj.transform.position = new Vector3(0f, 1.2f, 59f);
        BoxCollider bc = trigObj.AddComponent<BoxCollider>();
        bc.size = new Vector3(8f, 3f, 4f);
        bc.isTrigger = true;
        var stepTrigger = trigObj.AddComponent<TutorialMasteryGate>();
    }

    private static void BuildChamber3_WallClimb(Transform parent, Material sandstone, Material brass, Material obsidian, int envLayer)
    {
        GameObject c3 = new GameObject("Chamber_3_WallClimb");
        c3.transform.SetParent(parent);

        // Base Entrance Platform
        CreateBlock(c3.transform, "C3_BasePlatform", new Vector3(0f, 0f, 68f), new Vector3(12f, 1f, 8f), sandstone, envLayer);

        // Checkpoint 3
        CreateCheckpointTrigger(c3.transform, "C3_Checkpoint", new Vector3(0f, 1.2f, 68f), new Vector3(8f, 3f, 3f),
            3, "CHAMBER 3: WALL-VAULT & ZIG-ZAG WALL-JUMP",
            "Sprint toward a wall and tap [SPACE + W] to vault up +5.2m! Tap [SPACE] with [A/D] during wall-runs to zig-zag jump across chasms, or tap [SPACE] facing a wall to kick off backwards!",
            "[W + SPACE] Wall-Vault  •  [A / D + SPACE] Zig-Zag Wall-Jump  •  [SPACE] Wall-Kick",
            new Vector3(0f, 1.2f, 68f));

        // Tier 1 Vertical Cliff (+4.2m)
        CreateBlock(c3.transform, "C3_CliffFace_1", new Vector3(0f, 2.1f, 73f), new Vector3(12f, 4.2f, 1.5f), sandstone, envLayer);
        CreateBlock(c3.transform, "C3_Terrace_1", new Vector3(0f, 4.2f, 78f), new Vector3(12f, 0.8f, 8.5f), sandstone, envLayer);

        // Tier 2 Vertical Cliff (+4.2m -> Height 8.4m)
        CreateBlock(c3.transform, "C3_CliffFace_2", new Vector3(0f, 6.3f, 82f), new Vector3(12f, 4.2f, 1.5f), sandstone, envLayer);
        CreateBlock(c3.transform, "C3_Terrace_2", new Vector3(0f, 8.4f, 86f), new Vector3(12f, 0.8f, 6.5f), sandstone, envLayer);

        // High Chimney Shaft (X = -2.2m and X = +2.2m, Height 8.4m to 14.5m)
        // Player neutral wall-kicks back-and-forth between the two walls to reach Summit
        CreateBlock(c3.transform, "C3_ChimneyWall_L", new Vector3(-2.4f, 11.5f, 90f), new Vector3(0.6f, 6.5f, 6f), sandstone, envLayer);
        CreateBlock(c3.transform, "C3_ChimneyWall_R", new Vector3(2.4f, 11.5f, 90f), new Vector3(0.6f, 6.5f, 6f), sandstone, envLayer);

        // Summit High Platform (Height = 12.0m)
        CreateBlock(c3.transform, "C3_SummitPlatform", new Vector3(0f, 12.0f, 97f), new Vector3(12f, 1f, 8f), sandstone, envLayer);
        CreateBlock(c3.transform, "C3_SummitTrim", new Vector3(0f, 12.6f, 97f), new Vector3(12.4f, 0.4f, 8.4f), brass, envLayer);

        // Mastery Gate 3
        GameObject gate3 = CreateBlock(c3.transform, "C3_MasteryGate", new Vector3(0f, 14.5f, 101f), new Vector3(8f, 5f, 1f), brass, envLayer);
        gate3.AddComponent<TutorialMasteryGate>();
    }

    private static void BuildChamber4_KineticVault(Transform parent, Material sandstone, Material brass, Material lapis, Material obsidian, Material jade, int envLayer, int propLayer)
    {
        GameObject c4 = new GameObject("Chamber_4_KineticVault");
        c4.transform.SetParent(parent);

        // Vault Courtyard (Height = 12.0m, Z = 102 to 126m)
        CreateBlock(c4.transform, "C4_VaultFloor", new Vector3(0f, 12.0f, 114f), new Vector3(14f, 1f, 24f), sandstone, envLayer);

        // Checkpoint 4
        CreateCheckpointTrigger(c4.transform, "C4_Checkpoint", new Vector3(0f, 13.2f, 104f), new Vector3(8f, 3f, 3f),
            4, "CHAMBER 4: EGYPTIAN KINETIC PROP WEAPONRY",
            "Hold [RMB] to pull props into your pocket. Green = Light Urns, Gold = Medium Boulders, Red = Heavy Obsidian Slabs. Click [LMB] to launch as a kinetic cannon into the barricade and target button!",
            "[RMB] Pull & Carry  •  [LMB] Launch Kinetic Cannon  •  Colors indicate mass & weight class",
            new Vector3(0f, 13.2f, 104f));

        // Light Scarab Urns (Cylinder, fast kinetic launch)
        CreatePhysicsProp(c4.transform, "C4_ScarabUrn_1", new Vector3(-3f, 13.2f, 107f), new Vector3(0.75f, 1.25f, 0.75f), WeightClass.Light, 1.5f, jade, propLayer, PrimitiveType.Cylinder);
        CreatePhysicsProp(c4.transform, "C4_ScarabUrn_2", new Vector3(3f, 13.2f, 107f), new Vector3(0.75f, 1.25f, 0.75f), WeightClass.Light, 1.5f, jade, propLayer, PrimitiveType.Cylinder);

        // Medium Egyptian Sandstone Boulders (Spherical rock projectiles)
        CreatePhysicsProp(c4.transform, "C4_SandstoneBoulder_1", new Vector3(-4f, 13.2f, 110f), new Vector3(1.3f, 1.3f, 1.3f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Sphere);
        CreatePhysicsProp(c4.transform, "C4_SandstoneBoulder_2", new Vector3(4f, 13.2f, 110f), new Vector3(1.3f, 1.3f, 1.3f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Sphere);

        // Medium Carved Sandstone Relic Crates
        CreatePhysicsProp(c4.transform, "C4_SandstoneRelic_1", new Vector3(-2f, 13.2f, 113f), new Vector3(1.1f, 1.1f, 1.1f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Cube);
        CreatePhysicsProp(c4.transform, "C4_SandstoneRelic_2", new Vector3(2f, 13.2f, 113f), new Vector3(1.1f, 1.1f, 1.1f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Cube);

        // Heavy Obsidian Slab (Dense shield barrier)
        CreatePhysicsProp(c4.transform, "C4_ObsidianSlab", new Vector3(0f, 13.2f, 109f), new Vector3(1.4f, 2.0f, 0.6f), WeightClass.Heavy, 45f, obsidian, propLayer, PrimitiveType.Cube);

        // Breakable Barricade at Z = 118m
        GameObject barricade = CreateBlock(c4.transform, "C4_BreakableBarricade", new Vector3(0f, 14.5f, 118f), new Vector3(6f, 4f, 0.8f), obsidian, envLayer);
        var gateComp = barricade.AddComponent<TutorialMasteryGate>();
        var unlockImpactField = typeof(TutorialMasteryGate).GetField("unlockOnKineticImpact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (unlockImpactField != null) unlockImpactField.SetValue(gateComp, true);

        // Elevated Target Button (Ankh Switch) behind barricade at Z = 124m, Y = 16.5m
        GameObject targetSwitch = CreateBlock(c4.transform, "C4_TargetSwitch", new Vector3(0f, 16.5f, 124f), new Vector3(2.5f, 2.5f, 0.6f), lapis, envLayer);
        var targetGate = targetSwitch.AddComponent<TutorialMasteryGate>();
        if (unlockImpactField != null) unlockImpactField.SetValue(targetGate, true);
    }

    private static void BuildChamber5_LaserCorridor(Transform parent, Material sandstone, Material brass, Material lapis, Material obsidian, int envLayer)
    {
        GameObject c5 = new GameObject("Chamber_5_LaserCorridor");
        c5.transform.SetParent(parent);

        // Corridor Floor (Height = 12.0m, Z = 126 to 154m)
        CreateBlock(c5.transform, "C5_CorridorFloor", new Vector3(0f, 12.0f, 140f), new Vector3(10f, 1f, 28f), sandstone, envLayer);

        // Side Walls
        CreateBlock(c5.transform, "C5_Wall_L", new Vector3(-5.2f, 14.5f, 140f), new Vector3(0.6f, 4f, 28f), sandstone, envLayer);
        CreateBlock(c5.transform, "C5_Wall_R", new Vector3(5.2f, 14.5f, 140f), new Vector3(0.6f, 4f, 28f), sandstone, envLayer);

        // Checkpoint 5
        CreateCheckpointTrigger(c5.transform, "C5_Checkpoint", new Vector3(0f, 13.2f, 128f), new Vector3(8f, 3f, 3f),
            5, "CHAMBER 5: CHRONO-DILATION & BULLET DEFLECTION",
            "Press [Z] to enter Slow Motion. Time your [LMB] Push blast to deflect incoming laser bolts back at enemies! Bullets cannot be pulled.",
            "[Z] Toggle Slow Motion  •  [LMB] Push Bullet Deflect  •  [RMB] Cannot Pull Bullets",
            new Vector3(0f, 13.2f, 128f));

        // Pillars for tactical cover
        CreateBlock(c5.transform, "C5_CoverPillar_1", new Vector3(-2.5f, 14.0f, 135f), new Vector3(1.4f, 3.5f, 1.4f), obsidian, envLayer);
        CreateBlock(c5.transform, "C5_CoverPillar_2", new Vector3(2.5f, 14.0f, 142f), new Vector3(1.4f, 3.5f, 1.4f), obsidian, envLayer);

        // Rapid Laser Sentry at terminal end
        GameObject sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/RapidLaserSentry.prefab");
        if (sentryPrefab != null)
        {
            GameObject sentry = (GameObject)PrefabUtility.InstantiatePrefab(sentryPrefab, c5.transform);
            sentry.transform.position = new Vector3(0f, 13.2f, 150f);
            sentry.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Mastery Gate 5 at Z = 153m
        GameObject gate5 = CreateBlock(c5.transform, "C5_MasteryGate", new Vector3(0f, 14.5f, 153f), new Vector3(8f, 5f, 1f), brass, envLayer);
        gate5.AddComponent<TutorialMasteryGate>();
    }

    private static void BuildChamber6_CombatSanctum(Transform parent, Material sandstone, Material brass, Material lapis, Material obsidian, Material jade, int envLayer, int propLayer)
    {
        GameObject c6 = new GameObject("Chamber_6_CombatSanctum");
        c6.transform.SetParent(parent);

        // Grand Arena Courtyard (30m x 36m, Height = 12.0m, Z = 154 to 190m)
        CreateBlock(c6.transform, "C6_ArenaFloor", new Vector3(0f, 12.0f, 172f), new Vector3(30f, 1f, 36f), sandstone, envLayer);

        // Perimeter Ramparts (Height = 8m)
        CreateBlock(c6.transform, "C6_Perimeter_N", new Vector3(0f, 16.0f, 190f), new Vector3(30f, 8f, 1.5f), sandstone, envLayer);
        CreateBlock(c6.transform, "C6_Perimeter_E", new Vector3(15f, 16.0f, 172f), new Vector3(1.5f, 8f, 36f), sandstone, envLayer);
        CreateBlock(c6.transform, "C6_Perimeter_W", new Vector3(-15f, 16.0f, 172f), new Vector3(1.5f, 8f, 36f), sandstone, envLayer);

        // Golden Trim
        CreateBlock(c6.transform, "C6_Trim_N", new Vector3(0f, 20.2f, 190f), new Vector3(30f, 0.6f, 1.8f), brass, envLayer);

        // Checkpoint 6
        CreateCheckpointTrigger(c6.transform, "C6_Checkpoint", new Vector3(0f, 13.2f, 156f), new Vector3(8f, 3f, 3f),
            6, "CHAMBER 6: COMBAT MASTERY GAUNTLET",
            "Prove your mastery! Combine Wall-Running, Air-Dashing, Kinetic Launching, and Slow Motion to eliminate the temple guardians.",
            "Combine All Mechanics  •  Defeat Guardians to Ascend",
            new Vector3(0f, 13.2f, 156f));

        // High elevated vantage gantry for Sentry / Sniper
        CreateBlock(c6.transform, "C6_SniperGantry", new Vector3(0f, 17.5f, 186f), new Vector3(16f, 0.8f, 6f), sandstone, envLayer);
        CreateBlock(c6.transform, "C6_SniperRailing", new Vector3(0f, 18.2f, 183f), new Vector3(16f, 0.6f, 0.4f), brass, envLayer);

        // Center Altar with Golden Pyramidion of Ra
        CreateBlock(c6.transform, "C6_AltarBase", new Vector3(0f, 13.0f, 172f), new Vector3(6f, 1.2f, 6f), obsidian, envLayer);
        CreateBlock(c6.transform, "C6_RaPyramidion", new Vector3(0f, 14.5f, 172f), new Vector3(2.5f, 2.5f, 2.5f), brass, envLayer);
        CreateBlock(c6.transform, "C6_ScarabGem", new Vector3(0f, 16.0f, 172f), new Vector3(1f, 1f, 1f), jade, envLayer);

        // Mastery Gate 6 (Golden Pyramid Door at North End)
        GameObject gate6 = CreateBlock(c6.transform, "C6_MasteryGate", new Vector3(0f, 16.5f, 190f), new Vector3(8f, 6f, 2f), lapis, envLayer);
        var gateComp = gate6.AddComponent<TutorialMasteryGate>();

        // Egyptian Physics Props for Combat (Urns, Boulders, Relics, and Slabs)
        CreatePhysicsProp(c6.transform, "C6_ScarabUrn_1", new Vector3(-6f, 13.2f, 164f), new Vector3(0.75f, 1.25f, 0.75f), WeightClass.Light, 1.5f, jade, propLayer, PrimitiveType.Cylinder);
        CreatePhysicsProp(c6.transform, "C6_ScarabUrn_2", new Vector3(6f, 13.2f, 164f), new Vector3(0.75f, 1.25f, 0.75f), WeightClass.Light, 1.5f, jade, propLayer, PrimitiveType.Cylinder);
        CreatePhysicsProp(c6.transform, "C6_SandstoneBoulder_1", new Vector3(-10f, 13.2f, 172f), new Vector3(1.3f, 1.3f, 1.3f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Sphere);
        CreatePhysicsProp(c6.transform, "C6_SandstoneBoulder_2", new Vector3(10f, 13.2f, 172f), new Vector3(1.3f, 1.3f, 1.3f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Sphere);
        CreatePhysicsProp(c6.transform, "C6_SandstoneBoulder_3", new Vector3(0f, 13.2f, 165f), new Vector3(1.3f, 1.3f, 1.3f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Sphere);
        CreatePhysicsProp(c6.transform, "C6_SandstoneRelic_1", new Vector3(-7f, 13.2f, 180f), new Vector3(1.1f, 1.1f, 1.1f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Cube);
        CreatePhysicsProp(c6.transform, "C6_SandstoneRelic_2", new Vector3(7f, 13.2f, 180f), new Vector3(1.1f, 1.1f, 1.1f), WeightClass.Medium, 12f, sandstone, propLayer, PrimitiveType.Cube);
        CreatePhysicsProp(c6.transform, "C6_ObsidianSlab_1", new Vector3(-3f, 13.2f, 175f), new Vector3(1.4f, 2.0f, 0.6f), WeightClass.Heavy, 45f, obsidian, propLayer, PrimitiveType.Cube);
        CreatePhysicsProp(c6.transform, "C6_ObsidianSlab_2", new Vector3(3f, 13.2f, 175f), new Vector3(1.4f, 2.0f, 0.6f), WeightClass.Heavy, 45f, obsidian, propLayer, PrimitiveType.Cube);

        // Spawn Enemies
        GameObject swarmerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Swarmer.prefab");
        GameObject sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/RapidLaserSentry.prefab");
        GameObject hulkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/HulkLeaper.prefab");

        if (swarmerPrefab != null)
        {
            GameObject s1 = (GameObject)PrefabUtility.InstantiatePrefab(swarmerPrefab, c6.transform);
            s1.transform.position = new Vector3(-7f, 13.5f, 168f);
            gateComp.AddRequiredEnemy(s1);

            GameObject s2 = (GameObject)PrefabUtility.InstantiatePrefab(swarmerPrefab, c6.transform);
            s2.transform.position = new Vector3(7f, 13.5f, 168f);
            gateComp.AddRequiredEnemy(s2);
        }

        if (sentryPrefab != null)
        {
            GameObject sentry = (GameObject)PrefabUtility.InstantiatePrefab(sentryPrefab, c6.transform);
            sentry.transform.position = new Vector3(0f, 18.2f, 186f);
            sentry.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            gateComp.AddRequiredEnemy(sentry);
        }

        if (hulkPrefab != null)
        {
            GameObject hulk = (GameObject)PrefabUtility.InstantiatePrefab(hulkPrefab, c6.transform);
            hulk.transform.position = new Vector3(0f, 13.5f, 176f);
            gateComp.AddRequiredEnemy(hulk);
        }
    }

    private static void BuildVoidKillPlane(Transform parent)
    {
        GameObject killPlane = new GameObject("Global_VoidKill_Trigger");
        killPlane.transform.SetParent(parent);
        killPlane.transform.position = new Vector3(0f, -8.0f, 95f);
        killPlane.tag = "Respawn";

        BoxCollider col = killPlane.AddComponent<BoxCollider>();
        col.size = new Vector3(100f, 2.0f, 240f);
        col.isTrigger = true;
    }

    private static void SpawnAndPositionPlayer(Vector3 spawnPos)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            var pm = Object.FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.gameObject;
        }

        if (player == null)
        {
            player = PlayerRigBuilder.CreatePlayerRig();
        }

        if (player != null)
        {
            player.transform.position = spawnPos;
            player.transform.rotation = Quaternion.identity;

            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetCheckpoint(spawnPos);
            }

            if (player.GetComponent<PlayerHealth>() == null)
            {
                player.AddComponent<PlayerHealth>();
            }

            Undo.RecordObject(player, "Position Player at Academy Chamber 1");
            Debug.Log($"REBOUND: Player Rig positioned at Chamber 1 Entrance {spawnPos}!");
        }
    }

    private static GameObject CreateBlock(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, int layer)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.layer = layer;

        if (mat != null)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = mat;
        }
        return go;
    }

    private static void CreatePhysicsProp(Transform parent, string name, Vector3 pos, Vector3 scale, WeightClass weightClass, float mass, Material mat, int layer, PrimitiveType primitive = PrimitiveType.Cube)
    {
        GameObject prop = GameObject.CreatePrimitive(primitive);
        prop.name = name;
        prop.transform.SetParent(parent);
        prop.transform.position = pos;
        prop.transform.localScale = scale;
        prop.layer = layer;

        if (mat != null)
        {
            Renderer rend = prop.GetComponent<Renderer>();
            if (rend != null) rend.sharedMaterial = mat;
        }

        Rigidbody rb = prop.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        InteractableProp interactable = prop.AddComponent<InteractableProp>();
        var weightField = typeof(InteractableProp).GetField("weightClass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (weightField != null) weightField.SetValue(interactable, weightClass);
    }

    private static void CreateCheckpointTrigger(Transform parent, string name, Vector3 pos, Vector3 size, int index, string title, string objective, string controls, Vector3 spawnPoint)
    {
        GameObject trigObj = new GameObject(name);
        trigObj.transform.SetParent(parent);
        trigObj.transform.position = pos;

        BoxCollider bc = trigObj.AddComponent<BoxCollider>();
        bc.size = size;
        bc.isTrigger = true;

        GameObject spawnAnchor = new GameObject(name + "_SpawnAnchor");
        spawnAnchor.transform.SetParent(trigObj.transform);
        spawnAnchor.transform.position = spawnPoint;

        var checkpoint = trigObj.AddComponent<TutorialCheckpointTrigger>();
        checkpoint.SetChamberData(index, title, objective, controls, spawnAnchor.transform);
    }

    private static void DestroyIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) Undo.DestroyObjectImmediate(go);
    }
}
#endif
