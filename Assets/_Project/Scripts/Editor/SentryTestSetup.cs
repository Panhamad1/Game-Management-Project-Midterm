#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SentryTestSetup
{
    private const string PrefabFolder = "Assets/_Project/Prefabs";

    [MenuItem("Tools/REBOUND/Enemies/Spawn Sentry Test Encounter")]
    private static void SpawnEncounter()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        int projectileLayer = LayerMask.NameToLayer("Projectiles");
        int propLayer = LayerMask.NameToLayer("Interactables");
        if (player == null || enemyLayer < 0 || projectileLayer < 0 || propLayer < 0)
        {
            Debug.LogWarning("The sentry test needs a Player_Rig and the Enemies, Projectiles, and Interactables layers.");
            return;
        }

        Transform rig = player.transform;
        Vector3 ahead = Vector3.ProjectOnPlane(rig.forward, Vector3.up).normalized;
        if (!FindFloor(rig.position + ahead * 9f, out Vector3 sentryFloor) ||
            !FindFloor(rig.position + ahead * 3f + rig.right * 1.4f, out Vector3 crateFloor))
        {
            Debug.LogWarning("Point Player_Rig toward an open stretch of floor before spawning the sentry.");
            return;
        }
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/RapidLaserSentry.prefab");
        if (prefab == null) prefab = CreateSentryPrefab(enemyLayer, projectileLayer);
        if (prefab == null) return;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Sentry Test Encounter");
        if (player.GetComponent<SwarmerTestHealth>() == null)
        {
            Undo.AddComponent<SwarmerTestHealth>(player.gameObject);
        }

        GameObject sentry = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.gameObject.scene);
        sentry.transform.SetPositionAndRotation(sentryFloor, Quaternion.LookRotation(-ahead));
        Undo.RegisterCreatedObjectUndo(sentry, "Spawn Sentry");

        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Sentry_Shield_Crate";
        crate.layer = propLayer;
        crate.transform.position = crateFloor + Vector3.up * 0.81f;
        crate.transform.localScale = Vector3.one * 1.6f;
        crate.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        crate.AddComponent<InteractableProp>();
        Undo.RegisterCreatedObjectUndo(crate, "Spawn Shield Crate");
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = sentry;
    }

    private static bool FindFloor(Vector3 position, out Vector3 point)
    {
        bool hitFloor = Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit,
            8f, LayerMask.GetMask("Environment", "Default"), QueryTriggerInteraction.Ignore);
        point = hit.point;
        return hitFloor && hit.normal.y >= 0.7f;
    }

    private static GameObject CreateSentryPrefab(int enemyLayer, int projectileLayer)
    {
        EnemyProjectile bolt = GetOrCreateBolt(projectileLayer);
        if (bolt == null) return null;

        GameObject root = new GameObject("RapidLaserSentry");
        root.layer = enemyLayer;
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = Vector3.up * 0.85f;
        collider.size = new Vector3(1.3f, 1.7f, 1.3f);

        GameObject baseVisual = CreateVisual(root.transform, "Base", Vector3.up * 0.45f,
            new Vector3(1.3f, 0.9f, 1.3f));
        GameObject head = new GameObject("Head");
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = Vector3.up * 1.2f;
        GameObject headVisual = CreateVisual(head.transform, "Turret", Vector3.zero,
            new Vector3(1f, 0.55f, 1.25f));
        baseVisual.GetComponent<Renderer>().sharedMaterial = GetMaterial("SentryBase", new Color(0.25f, 0.3f, 0.35f));
        headVisual.GetComponent<Renderer>().sharedMaterial = GetMaterial("SentryTurret", new Color(0.8f, 0.2f, 0.06f));
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(head.transform, false);
        muzzle.transform.localPosition = Vector3.forward * 0.85f;

        SerializedObject setup = new SerializedObject(root.AddComponent<SentryEnemy>());
        setup.FindProperty("head").objectReferenceValue = head.transform;
        setup.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
        setup.FindProperty("boltPrefab").objectReferenceValue = bolt;
        setup.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/RapidLaserSentry.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static EnemyProjectile GetOrCreateBolt(int layer)
    {
        string path = PrefabFolder + "/SentryBolt.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null) return prefab.GetComponent<EnemyProjectile>();

        GameObject bolt = CreateVisual(null, "SentryBolt", Vector3.zero, new Vector3(0.11f, 0.11f, 0.45f));
        bolt.layer = layer;
        bolt.GetComponent<Renderer>().sharedMaterial = GetMaterial("SentryBolt", new Color(1f, 0.35f, 0.04f));
        bolt.AddComponent<EnemyProjectile>();
        prefab = PrefabUtility.SaveAsPrefabAsset(bolt, path);
        Object.DestroyImmediate(bolt);
        return prefab != null ? prefab.GetComponent<EnemyProjectile>() : null;
    }

    private static GameObject CreateVisual(Transform parent, string objectName, Vector3 position, Vector3 scale)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = objectName;
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = position;
        visual.transform.localScale = scale;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        return visual;
    }

    private static Material GetMaterial(string materialName, Color color)
    {
        string path = PrefabFolder + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        material = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}
#endif
