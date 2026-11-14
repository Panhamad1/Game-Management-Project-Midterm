#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SwarmerTestSetup
{
    private const string PrefabPath = "Assets/_Project/Prefabs/Swarmer.prefab";

    [MenuItem("Tools/REBOUND/Enemies/Spawn Swarmer Test Pack")]
    private static void SpawnPack()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        int enemyLayer = LayerMask.NameToLayer("Enemies");
        int floorMask = LayerMask.GetMask("Environment", "Default");
        if (player == null || enemyLayer < 0)
        {
            Debug.LogWarning("The Swarmer test needs a Player_Rig and the Enemies layer.");
            return;
        }

        Vector3 center = player.transform.position + player.transform.forward * 8f;
        Vector3[] positions = new Vector3[4];
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 offset = player.transform.right * (i - 1.5f) * 1.6f;
            if (!Physics.Raycast(center + offset + Vector3.up * 2f, Vector3.down, out RaycastHit hit,
                    8f, floorMask, QueryTriggerInteraction.Ignore) ||
                Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
            {
                Debug.LogWarning("Face an open floor with space in front of Player_Rig and run the Swarmer test setup again.");
                return;
            }
            positions[i] = hit.point + Vector3.up * 0.4f;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) prefab = CreatePrefab(enemyLayer);
        if (prefab == null) return;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Swarmer Test Pack");
        if (player.GetComponent<SwarmerTestHealth>() == null)
        {
            Undo.AddComponent<SwarmerTestHealth>(player.gameObject);
        }

        GameObject pack = new GameObject("Swarmer_Test_Pack");
        Undo.RegisterCreatedObjectUndo(pack, "Create Swarmer Pack");
        foreach (Vector3 position in positions)
        {
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pack.transform);
            enemy.transform.position = position;
            Undo.RegisterCreatedObjectUndo(enemy, "Spawn Swarmer");
        }
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = pack;
    }

    private static GameObject CreatePrefab(int enemyLayer)
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemy.name = "Swarmer";
        enemy.layer = enemyLayer;
        enemy.transform.localScale = Vector3.one * 0.7f;
        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        // Detect a fast slide before solid contact can slow the player.
        SphereCollider ramSensor = enemy.AddComponent<SphereCollider>();
        ramSensor.radius = 1.35f;
        ramSensor.isTrigger = true;
        enemy.AddComponent<SwarmerEnemy>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, PrefabPath);
        Object.DestroyImmediate(enemy);
        return prefab;
    }
}
#endif
