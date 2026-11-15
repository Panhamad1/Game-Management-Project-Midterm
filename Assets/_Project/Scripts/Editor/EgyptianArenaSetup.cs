#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Procedurally builds the grand 100m x 100m Egyptian Parkour & Combat Arena.
/// Supports both full one-click complete generation and separated modular builders.
/// </summary>
public static class EgyptianArenaSetup
{
    public const string ArenaRootName = "Egyptian_Parkour_Arena_100m";
    public const string PropsRootName = "Interactive_Props_Cluster";
    public const string EnemiesRootName = "Enemy_Squad";

    public static void BuildCompleteGame()
    {
        BuildArenaGeometry();
        SpawnOrResetPlayer();
        SpawnPhysicsProps();
        SpawnEnemySquad();
        Debug.Log("REBOUND: Complete 100m Egyptian Parkour Arena, Player Rig, 60+ Props, and Enemies successfully built!");
    }

    public static void BuildArenaGeometry()
    {
        // 1. Remove previous arena root if present
        GameObject oldArena = GameObject.Find(ArenaRootName);
        if (oldArena != null) Undo.DestroyObjectImmediate(oldArena);

        // Hide legacy rooms to prevent overlapping
        GameObject oldRoom = GameObject.Find("Physics_Test_Room_Chambers");
        if (oldRoom != null) oldRoom.SetActive(false);
        GameObject oldGrappleArena = GameObject.Find("Redliner_Grapple_Arena_200m");
        if (oldGrappleArena != null) oldGrappleArena.SetActive(false);
        GameObject old45mArena = GameObject.Find("Egyptian_Parkour_Arena_45m");
        if (old45mArena != null) old45mArena.SetActive(false);

        // 2. Materials
        Material sandstone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Sandstone.mat");
        Material brass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Brass.mat");
        Material lapis = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/RoyalLapis.mat");
        Material obsidian = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Obsidian.mat");
        Material jade = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/ScarabJade.mat");

        int envLayer = LayerMask.NameToLayer("Environment");
        if (envLayer == -1) envLayer = 0;

        // 3. Directional Sun
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

        // 4. Arena Root Container
        GameObject arenaRoot = new GameObject(ArenaRootName);
        arenaRoot.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(arenaRoot, "Build 100m Egyptian Arena");

        // 5. Grand Sandstone Courtyard Floor (100m x 100m)
        CreateBlock(arenaRoot.transform, "Courtyard_Floor_100m", new Vector3(0f, -0.5f, 0f), new Vector3(100f, 1f, 100f), sandstone, envLayer);

        // 6. Perimeter Wall-Running Ramparts (10m high, 100m length)
        float half = 50f;
        float wallHeight = 10f;
        float wallThickness = 2.0f;

        CreateBlock(arenaRoot.transform, "Wall_North", new Vector3(0f, wallHeight * 0.5f, half), new Vector3(100f + wallThickness * 2f, wallHeight, wallThickness), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Wall_East", new Vector3(half, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, 100f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Wall_West", new Vector3(-half, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, 100f), sandstone, envLayer);

        // South entrance wall with monumental pylon gate
        CreateBlock(arenaRoot.transform, "Wall_South_Left", new Vector3(-32f, wallHeight * 0.5f, -half), new Vector3(36f, wallHeight, wallThickness), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Wall_South_Right", new Vector3(32f, wallHeight * 0.5f, -half), new Vector3(36f, wallHeight, wallThickness), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Gate_Arch_Lapis", new Vector3(0f, 8.5f, -half), new Vector3(28f, 3f, wallThickness), lapis, envLayer);

        // Golden Wall Trim
        CreateBlock(arenaRoot.transform, "Trim_North", new Vector3(0f, wallHeight + 0.3f, half - 0.25f), new Vector3(100f, 0.6f, 1.4f), brass, envLayer);
        CreateBlock(arenaRoot.transform, "Trim_East", new Vector3(half - 0.25f, wallHeight + 0.3f, 0f), new Vector3(1.4f, 0.6f, 100f), brass, envLayer);
        CreateBlock(arenaRoot.transform, "Trim_West", new Vector3(-half + 0.25f, wallHeight + 0.3f, 0f), new Vector3(1.4f, 0.6f, 100f), brass, envLayer);

        // 7. Four High Parkour Rampart Gantries (East, West, North-East, North-West at Height = 5m)
        CreateBlock(arenaRoot.transform, "Gantry_East", new Vector3(44f, 5.0f, 0f), new Vector3(8f, 0.8f, 55f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Gantry_West", new Vector3(-44f, 5.0f, 0f), new Vector3(8f, 0.8f, 55f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Gantry_North_High", new Vector3(0f, 6.0f, 44f), new Vector3(50f, 0.8f, 8f), sandstone, envLayer);

        // Access Ramps to Gantries
        GameObject rampE = CreateBlock(arenaRoot.transform, "Ramp_East", new Vector3(44f, 2.5f, -34f), new Vector3(6f, 0.6f, 14f), sandstone, envLayer);
        rampE.transform.rotation = Quaternion.Euler(-21f, 0f, 0f);

        GameObject rampW = CreateBlock(arenaRoot.transform, "Ramp_West", new Vector3(-44f, 2.5f, -34f), new Vector3(6f, 0.6f, 14f), sandstone, envLayer);
        rampW.transform.rotation = Quaternion.Euler(-21f, 0f, 0f);

        // 8. Four Monumental Obelisks (Diagonals for Wall-Running Loops)
        Vector3[] obeliskCoords = new[]
        {
            new Vector3(25f, 8f, 25f),
            new Vector3(-25f, 8f, 25f),
            new Vector3(25f, 8f, -25f),
            new Vector3(-25f, 8f, -25f)
        };
        for (int i = 0; i < obeliskCoords.Length; i++)
        {
            Vector3 pos = obeliskCoords[i];
            CreateBlock(arenaRoot.transform, $"Monument_Obelisk_{i + 1}", pos, new Vector3(4.5f, 16f, 4.5f), sandstone, envLayer);
            CreateBlock(arenaRoot.transform, $"Obelisk_Brass_Cap_{i + 1}", pos + Vector3.up * 9.2f, new Vector3(3.2f, 2.4f, 3.2f), brass, envLayer);
        }

        // 9. Grand 4-Tier Stepped Central Pyramid (Center of 100m Arena)
        CreateBlock(arenaRoot.transform, "Pyramid_Tier1", new Vector3(0f, 1.0f, 0f), new Vector3(26f, 2.0f, 26f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Pyramid_Tier2", new Vector3(0f, 3.0f, 0f), new Vector3(19f, 2.0f, 19f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Pyramid_Tier3", new Vector3(0f, 5.0f, 0f), new Vector3(12f, 2.0f, 12f), sandstone, envLayer);
        CreateBlock(arenaRoot.transform, "Pyramid_Tier4", new Vector3(0f, 7.0f, 0f), new Vector3(6.5f, 2.0f, 6.5f), sandstone, envLayer);
        // Apex Shrine
        CreateBlock(arenaRoot.transform, "Pyramid_Apex_Altar", new Vector3(0f, 8.4f, 0f), new Vector3(3.2f, 0.8f, 3.2f), lapis, envLayer);
        CreateBlock(arenaRoot.transform, "Pyramid_Scarab_Idol", new Vector3(0f, 9.3f, 0f), new Vector3(1.2f, 1.0f, 1.2f), jade, envLayer);

        // 10. Colonnade Avenue (12 Sandstone Pillars leading to the Pyramid)
        float[] pillarZ = new[] { -38f, -31f, -24f, -17f, 17f, 24f };
        for (int i = 0; i < pillarZ.Length; i++)
        {
            float z = pillarZ[i];
            CreateBlock(arenaRoot.transform, $"Colonnade_L_{i + 1}", new Vector3(-8f, 3.5f, z), new Vector3(2.2f, 7f, 2.2f), sandstone, envLayer);
            CreateBlock(arenaRoot.transform, $"Colonnade_R_{i + 1}", new Vector3(8f, 3.5f, z), new Vector3(2.2f, 7f, 2.2f), sandstone, envLayer);
            CreateBlock(arenaRoot.transform, $"Colonnade_Cap_L_{i + 1}", new Vector3(-8f, 7.2f, z), new Vector3(2.8f, 0.6f, 2.8f), obsidian, envLayer);
            CreateBlock(arenaRoot.transform, $"Colonnade_Cap_R_{i + 1}", new Vector3(8f, 7.2f, z), new Vector3(2.8f, 0.6f, 2.8f), obsidian, envLayer);
        }

        Debug.Log("REBOUND: 100m x 100m Egyptian Arena Geometry successfully built!");
    }

    public static void SpawnOrResetPlayer()
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
            Vector3 spawnPos = new Vector3(0f, 0.8f, -42f); // South entrance
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

            Undo.RecordObject(player, "Reset Player Rig to 100m Arena");
            Debug.Log($"REBOUND: Player Rig positioned safely at South Entrance {spawnPos}!");
        }
    }

    public static void SpawnEnemySquad()
    {
        GameObject oldEnemies = GameObject.Find(EnemiesRootName);
        if (oldEnemies != null) Undo.DestroyObjectImmediate(oldEnemies);

        GameObject arenaRoot = GameObject.Find(ArenaRootName);
        Transform parent = arenaRoot != null ? arenaRoot.transform : null;

        GameObject enemyGroup = new GameObject(EnemiesRootName);
        if (parent != null) enemyGroup.transform.SetParent(parent);
        Undo.RegisterCreatedObjectUndo(enemyGroup, "Spawn Enemy Squad");

        GameObject swarmerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Swarmer.prefab");
        GameObject sentryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/RapidLaserSentry.prefab");
        GameObject sniperPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Sniper.prefab");
        GameObject hulkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/HulkLeaper.prefab");

        // Swarmers patrolling the courtyard
        if (swarmerPrefab != null)
        {
            InstantiateEnemy(swarmerPrefab, enemyGroup.transform, new Vector3(-14f, 2.5f, -10f));
            InstantiateEnemy(swarmerPrefab, enemyGroup.transform, new Vector3(14f, 2.5f, -10f));
            InstantiateEnemy(swarmerPrefab, enemyGroup.transform, new Vector3(-12f, 3.5f, 15f));
            InstantiateEnemy(swarmerPrefab, enemyGroup.transform, new Vector3(12f, 3.5f, 15f));
            InstantiateEnemy(swarmerPrefab, enemyGroup.transform, new Vector3(0f, 6.0f, 0f));
        }

        // Sentries mounted on elevated gantries
        if (sentryPrefab != null)
        {
            InstantiateEnemy(sentryPrefab, enemyGroup.transform, new Vector3(44f, 5.4f, 8f));
            InstantiateEnemy(sentryPrefab, enemyGroup.transform, new Vector3(-44f, 5.4f, 8f));
            InstantiateEnemy(sentryPrefab, enemyGroup.transform, new Vector3(0f, 6.4f, 44f));
        }

        // Snipers on elevated obelisk vantage platforms
        if (sniperPrefab != null)
        {
            InstantiateEnemy(sniperPrefab, enemyGroup.transform, new Vector3(25f, 16.5f, 25f));
            InstantiateEnemy(sniperPrefab, enemyGroup.transform, new Vector3(-25f, 16.5f, 25f));
        }

        // Hulk Leapers in open battle zones
        if (hulkPrefab != null)
        {
            InstantiateEnemy(hulkPrefab, enemyGroup.transform, new Vector3(-18f, 0.2f, 22f));
            InstantiateEnemy(hulkPrefab, enemyGroup.transform, new Vector3(18f, 0.2f, 22f));
        }

        Debug.Log("REBOUND: Enemy Squad successfully deployed in 100m Arena!");
    }

    public static void SpawnPhysicsProps()
    {
        GameObject oldProps = GameObject.Find(PropsRootName);
        if (oldProps != null) Undo.DestroyObjectImmediate(oldProps);

        GameObject arenaRoot = GameObject.Find(ArenaRootName);
        Transform parent = arenaRoot != null ? arenaRoot.transform : null;

        GameObject propGroup = new GameObject(PropsRootName);
        if (parent != null) propGroup.transform.SetParent(parent);
        Undo.RegisterCreatedObjectUndo(propGroup, "Spawn Physics Props");

        Material sandstone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Sandstone.mat");
        Material brass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Brass.mat");
        Material obsidian = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/PyramidMaterials/Obsidian.mat");

        int propLayer = LayerMask.NameToLayer("Interactable");
        if (propLayer == -1) propLayer = LayerMask.NameToLayer("Interactables");
        if (propLayer == -1) propLayer = 0;

        // 28 Medium Sandstone Crates (Mass 12)
        Vector3[] mediumPositions = new[]
        {
            new Vector3(-4f, 0.6f, -32f), new Vector3(4f, 0.6f, -32f), new Vector3(-6f, 0.6f, -30f), new Vector3(6f, 0.6f, -30f),
            new Vector3(-5f, 1.7f, -31f), new Vector3(5f, 1.7f, -31f),
            new Vector3(-15f, 0.6f, -18f), new Vector3(15f, 0.6f, -18f), new Vector3(-17f, 0.6f, -16f), new Vector3(17f, 0.6f, -16f),
            new Vector3(-14f, 2.2f, 0f), new Vector3(14f, 2.2f, 0f), new Vector3(0f, 2.2f, -14f), new Vector3(0f, 2.2f, 14f),
            new Vector3(-10f, 4.2f, 0f), new Vector3(10f, 4.2f, 0f),
            new Vector3(-20f, 0.6f, 28f), new Vector3(20f, 0.6f, 28f), new Vector3(-22f, 0.6f, 30f), new Vector3(22f, 0.6f, 30f),
            new Vector3(44f, 5.8f, 14f), new Vector3(44f, 5.8f, -14f), new Vector3(-44f, 5.8f, 14f), new Vector3(-44f, 5.8f, -14f),
            new Vector3(-30f, 0.6f, 0f), new Vector3(30f, 0.6f, 0f), new Vector3(0f, 0.6f, 38f), new Vector3(0f, 0.6f, -22f)
        };
        for (int i = 0; i < mediumPositions.Length; i++)
        {
            CreatePhysicsProp(propGroup.transform, $"Sandstone_Crate_{i + 1}", mediumPositions[i], new Vector3(1.2f, 1.2f, 1.2f), WeightClass.Medium, 12f, sandstone, propLayer);
        }

        // 22 Light Canopic Urns & Vases (Mass 4.5)
        Vector3[] lightPositions = new[]
        {
            new Vector3(-2f, 0.5f, -38f), new Vector3(2f, 0.5f, -38f),
            new Vector3(-7f, 0.5f, -26f), new Vector3(7f, 0.5f, -26f),
            new Vector3(-7f, 0.5f, -19f), new Vector3(7f, 0.5f, -19f),
            new Vector3(-18f, 0.5f, -5f), new Vector3(18f, 0.5f, -5f),
            new Vector3(-18f, 0.5f, 5f), new Vector3(18f, 0.5f, 5f),
            new Vector3(-8f, 4.2f, -8f), new Vector3(8f, 4.2f, -8f),
            new Vector3(-8f, 4.2f, 8f), new Vector3(8f, 4.2f, 8f),
            new Vector3(-4f, 6.2f, 0f), new Vector3(4f, 6.2f, 0f),
            new Vector3(44f, 5.6f, 0f), new Vector3(-44f, 5.6f, 0f),
            new Vector3(0f, 6.6f, 40f), new Vector3(0f, 6.6f, 48f),
            new Vector3(-35f, 0.5f, -30f), new Vector3(35f, 0.5f, -30f)
        };
        for (int i = 0; i < lightPositions.Length; i++)
        {
            CreatePhysicsProp(propGroup.transform, $"Canopic_Urn_{i + 1}", lightPositions[i], new Vector3(0.8f, 1.0f, 0.8f), WeightClass.Light, 4.5f, brass, propLayer);
        }

        // 12 Heavy Obsidian Monoliths & Slabs (Mass 30)
        Vector3[] heavyPositions = new[]
        {
            new Vector3(-12f, 0.9f, -36f), new Vector3(12f, 0.9f, -36f),
            new Vector3(-28f, 0.9f, -15f), new Vector3(28f, 0.9f, -15f),
            new Vector3(-28f, 0.9f, 15f), new Vector3(28f, 0.9f, 15f),
            new Vector3(-16f, 0.9f, 35f), new Vector3(16f, 0.9f, 35f),
            new Vector3(44f, 5.9f, -22f), new Vector3(-44f, 5.9f, -22f),
            new Vector3(0f, 1.9f, 0f), new Vector3(0f, 0.9f, 28f)
        };
        for (int i = 0; i < heavyPositions.Length; i++)
        {
            CreatePhysicsProp(propGroup.transform, $"Heavy_Monolith_{i + 1}", heavyPositions[i], new Vector3(1.5f, 1.8f, 1.5f), WeightClass.Heavy, 30f, obsidian, propLayer);
        }

        Debug.Log("REBOUND: 62 Interactive Physics Props deployed across 100m Arena!");
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

    private static void CreatePhysicsProp(Transform parent, string name, Vector3 pos, Vector3 scale, WeightClass weightClass, float mass, Material mat, int layer)
    {
        GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
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

    private static void InstantiateEnemy(GameObject prefab, Transform parent, Vector3 pos)
    {
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        enemy.transform.position = pos;
    }
}
#endif
