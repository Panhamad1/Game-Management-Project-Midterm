#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class HulkTestSetup
{
    private const string Folder = "Assets/_Project/Prefabs";
    private const string PrefabPath = Folder + "/HulkLeaper.prefab";

    [MenuItem("Tools/REBOUND/Enemies/Spawn Hulk Test Encounter")]
    private static void SpawnEncounter()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        int enemies = LayerMask.NameToLayer("Enemies");
        int props = LayerMask.NameToLayer("Interactables");
        if (player == null || enemies < 0 || props < 0)
        {
            Debug.LogWarning("The Hulk test needs a Player_Rig and the Enemies and Interactables layers.");
            return;
        }

        Transform rig = player.transform;
        if (!FindFloor(rig.position + rig.forward * 9f, out Vector3 hulkFloor) ||
            !FindFloor(rig.position + rig.forward * 3f + rig.right * 2f, out Vector3 crateFloor))
        {
            Debug.LogWarning("Point Player_Rig toward an open stretch of floor before spawning the Hulk.");
            return;
        }
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) prefab = CreatePrefab(enemies);
        if (prefab == null) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Hulk Test Encounter");
        if (player.GetComponent<SwarmerTestHealth>() == null) Undo.AddComponent<SwarmerTestHealth>(player.gameObject);
        GameObject hulk = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.gameObject.scene);
        hulk.transform.SetPositionAndRotation(hulkFloor + Vector3.up * 0.05f,
            Quaternion.LookRotation(Vector3.ProjectOnPlane(-rig.forward, Vector3.up)));
        Undo.RegisterCreatedObjectUndo(hulk, "Spawn Hulk");

        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Hulk_Test_Crate";
        crate.layer = props;
        crate.transform.position = crateFloor + Vector3.up * 0.55f;
        crate.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        crate.AddComponent<InteractableProp>();
        Undo.RegisterCreatedObjectUndo(crate, "Spawn Hulk Test Crate");
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = hulk;
    }

    private static bool FindFloor(Vector3 near, out Vector3 point)
    {
        bool found = Physics.Raycast(near + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 8f,
            LayerMask.GetMask("Environment", "Default"), QueryTriggerInteraction.Ignore);
        point = hit.point;
        return found && hit.normal.y >= 0.7f;
    }

    private static GameObject CreatePrefab(int layer)
    {
        GameObject root = new GameObject("HulkLeaper");
        root.layer = layer;
        HulkLeaperEnemy hulk = root.AddComponent<HulkLeaperEnemy>();
        Rigidbody body = root.GetComponent<Rigidbody>();
        body.mass = 1500f;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
        capsule.height = 3f;
        capsule.radius = 0.85f;
        capsule.center = Vector3.up * 1.5f;

        GameObject armor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armor.name = "Armor";
        armor.transform.SetParent(root.transform, false);
        armor.transform.localPosition = Vector3.up * 1.5f;
        armor.transform.localScale = new Vector3(1.7f, 1.5f, 1.7f);
        Object.DestroyImmediate(armor.GetComponent<Collider>());

        Material glow = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/HulkSlamGlow.mat");
        if (glow == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            glow = new Material(shader) { color = new Color(1f, 0.38f, 0.04f) };
            AssetDatabase.CreateAsset(glow, Folder + "/HulkSlamGlow.mat");
        }
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "Back_Core";
        core.layer = layer;
        core.transform.SetParent(root.transform, false);
        core.transform.localPosition = new Vector3(0f, 1.6f, -1.05f);
        core.transform.localScale = Vector3.one * 0.75f;
        core.GetComponent<Renderer>().sharedMaterial = glow;
        core.AddComponent<HulkWeakPoint>();
        core.SetActive(false);

        GameObject wave = new GameObject("Shockwave");
        wave.transform.SetParent(root.transform, false);
        LineRenderer ring = wave.AddComponent<LineRenderer>();
        ring.sharedMaterial = glow;
        ring.widthMultiplier = 0.2f;
        ring.enabled = false;
        HulkShockwave shockwave = wave.AddComponent<HulkShockwave>();

        SerializedObject setup = new SerializedObject(hulk);
        setup.FindProperty("armor").objectReferenceValue = armor.GetComponent<Renderer>();
        setup.FindProperty("weakPoint").objectReferenceValue = core;
        setup.FindProperty("shockwave").objectReferenceValue = shockwave;
        setup.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }
}
#endif
