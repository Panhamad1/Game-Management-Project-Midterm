#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SniperTestSetup
{
    private const string Folder = "Assets/_Project/Prefabs";

    [MenuItem("Tools/REBOUND/Enemies/Spawn Sniper Test Encounter")]
    private static void SpawnEncounter()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        int roundLayer = LayerMask.NameToLayer("Projectiles");
        if (player == null || enemyLayer < 0 || roundLayer < 0)
        {
            Debug.LogWarning("The sniper test needs a Player_Rig and the Enemies and Projectiles layers.");
            return;
        }

        Transform rig = player.transform;
        Vector3 ahead = Vector3.ProjectOnPlane(rig.forward, Vector3.up).normalized;
        Vector3 probe = rig.position + ahead * 15f + Vector3.up * 2f;
        if (!Physics.Raycast(probe, Vector3.down, out RaycastHit floor, 22f,
                LayerMask.GetMask("Default", "Environment"), QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("Point Player_Rig toward open floor before spawning the sniper.");
            return;
        }
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        string path = Folder + "/Sniper.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) prefab = CreatePrefab(path, enemyLayer, roundLayer);
        if (prefab == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Sniper Test Encounter");
        if (player.GetComponent<SwarmerTestHealth>() == null) Undo.AddComponent<SwarmerTestHealth>(player.gameObject);
        GameObject sniper = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.gameObject.scene);
        sniper.transform.SetPositionAndRotation(floor.point, Quaternion.LookRotation(-ahead));
        Undo.RegisterCreatedObjectUndo(sniper, "Spawn Sniper");
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = sniper;
    }

    private static GameObject CreatePrefab(string path, int enemyLayer, int roundLayer)
    {
        Material glow = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/SniperGlow.mat");
        if (glow == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            glow = new Material(shader) { color = new Color(1f, 0.08f, 0.04f) };
            AssetDatabase.CreateAsset(glow, Folder + "/SniperGlow.mat");
        }

        string roundPath = Folder + "/SniperRound.prefab";
        GameObject roundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roundPath);
        if (roundPrefab == null)
        {
            GameObject round = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            round.name = "SniperRound";
            round.layer = roundLayer;
            round.transform.localScale = new Vector3(0.24f, 0.24f, 0.65f);
            // The collider lets the player's Push ray target the round.
            Rigidbody body = round.AddComponent<Rigidbody>();
            body.isKinematic = true;
            round.GetComponent<Renderer>().sharedMaterial = glow;
            round.AddComponent<EnemyProjectile>();
            roundPrefab = PrefabUtility.SaveAsPrefabAsset(round, roundPath);
            Object.DestroyImmediate(round);
        }
        if (roundPrefab == null) return null;

        GameObject root = new GameObject("Sniper");
        root.layer = enemyLayer;
        BoxCollider hitbox = root.AddComponent<BoxCollider>();
        hitbox.center = Vector3.up;
        hitbox.size = new Vector3(1f, 2f, 1f);
        GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        shell.name = "Body";
        shell.transform.SetParent(root.transform, false);
        shell.transform.localPosition = Vector3.up;
        Object.DestroyImmediate(shell.GetComponent<Collider>());

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, 1.55f, 0.55f);
        LineRenderer laser = muzzle.AddComponent<LineRenderer>();
        laser.sharedMaterial = glow;
        laser.positionCount = 2;
        laser.useWorldSpace = true;
        laser.enabled = false;

        SerializedObject setup = new SerializedObject(root.AddComponent<SniperEnemy>());
        setup.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
        setup.FindProperty("laser").objectReferenceValue = laser;
        setup.FindProperty("roundPrefab").objectReferenceValue = roundPrefab.GetComponent<EnemyProjectile>();
        setup.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }
}
#endif
