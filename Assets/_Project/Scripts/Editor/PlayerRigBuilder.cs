#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity Editor utility that constructs the complete upgraded Player Rig, HUD, and physics room with one click.
/// Upgraded with:
/// - Ledge mantling & 1.6m jump
/// - Visible air blast shockwave & proximity boost
/// - Swirling vacuum vortex & trajectory aiming arc
/// - Electric cyan focus vision
/// - Zero-file procedural audio suite
/// </summary>
public static class PlayerRigBuilder
{
    [MenuItem("Tools/REBOUND/1. Setup Complete Game (Arena + Player)", false, 0)]
    public static void SetupCompleteGame()
    {
        CreateRedlinerGrappleArena();
        CreatePlayerRig();
        SnapPlayerToSafeSpawn();
        Debug.Log("REBOUND: Setup Complete! 200m Redliner Grapple Arena & Player Rig are fully generated and ready to play!");
    }

    [InitializeOnLoadMethod]
    [MenuItem("Tools/REBOUND/2. Run Movement Unit Tests", false, 1)]
    public static void RunMovementUnitTests()
    {
        Debug.Log("==================================================");
        Debug.Log("REBOUND: EXECUTING MOVEMENT PHYSICS TEST SUITE...");
        Debug.Log("==================================================");

        var tests = new MovementPhysicsTests();
        int passed = 0;
        int failed = 0;

        void ExecuteTest(string name, System.Action testAction)
        {
            try
            {
                testAction();
                Debug.Log($"[PASS] {name}");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] {name}: {ex.Message}");
                failed++;
            }
        }

        ExecuteTest("Reconciled Jump Impulse Yields 1.6m Height", tests.Test_ReconciledJumpImpulse_YieldsTarget1Point6MeterHeight);
        ExecuteTest("Diagonal Input Normalization Unit Magnitude", tests.Test_DiagonalInputNormalization_NeverExceedsUnitMagnitude);
        ExecuteTest("Quake/Source Air Strafing Acceleration & Preservation", tests.Test_AirStrafing_PreservesSpeedAndAddsAccelerationOnCurve);
        ExecuteTest("Slide Aerodynamic Drag Curve Deceleration", tests.Test_SlideDrag_DeceleratesSmoothlyAtHighSpeed);
        ExecuteTest("Slope Surfing Downhill/Uphill Dynamics", tests.Test_SlopeSurfing_DownhillAcceleratesUphillDecelerates);
        ExecuteTest("Grapple Tangential Velocity 100% Preservation", tests.Test_GrappleDecomposition_Preserves100PercentTangentialVelocity);
        ExecuteTest("Surface Proximity Recoil Inverse Scaling", tests.Test_SurfaceProximityRecoil_ZeroBeyondThresholdMaxAtPointBlank);
        ExecuteTest("Weight-Class Responsive Prop Reel Dynamics", tests.Test_WeightClassReelDynamics_ScalesPullSpeedByMass);

        Debug.Log("==================================================");
        Debug.Log($"REBOUND: TEST SUITE COMPLETE. Passed: {passed}, Failed: {failed}");
        Debug.Log("==================================================");
    }

    [MenuItem("Tools/REBOUND/Advanced/Snap Player to Safe Spawn", false, 20)]
    public static void SnapPlayerToSafeSpawn()
    {
        GameObject playerObj = GameObject.Find("Player_Rig");
        if (playerObj == null)
        {
            Debug.LogWarning("REBOUND: No Player_Rig found in current scene!");
            return;
        }

        Vector3 safePos = GetSafeSpawnPosition();

        Undo.RecordObject(playerObj.transform, "Snap Player to Safe Spawn");
        playerObj.transform.position = safePos;
        playerObj.transform.rotation = Quaternion.identity;

        PlayerMovement pMove = playerObj.GetComponent<PlayerMovement>();
        if (pMove != null)
        {
            Undo.RecordObject(pMove, "Update Safe Checkpoint");
            pMove.SetCheckpoint(safePos);
        }

        Selection.activeGameObject = playerObj;
        Debug.Log($"REBOUND: Player_Rig successfully relocated to safe platform surface: {safePos}");
    }

    public static Vector3 GetSafeSpawnPosition()
    {
        GameObject startHub = GameObject.Find("Chamber1_StartHub");
        if (startHub != null)
        {
            float hubTop = startHub.transform.position.y + (startHub.transform.localScale.y * 0.5f);
            return new Vector3(startHub.transform.position.x, hubTop + 1.1f, startHub.transform.position.z - 2.5f);
        }

        GameObject testRoom = GameObject.Find("TestRoom");
        if (testRoom != null)
        {
            Transform floorChild = testRoom.transform.Find("Floor");
            if (floorChild != null)
            {
                float floorTop = floorChild.position.y + (floorChild.localScale.y * 0.5f);
                return new Vector3(0f, floorTop + 1.1f, 0f);
            }
        }

        return new Vector3(0f, 7.6f, -2.5f);
    }

    [MenuItem("Tools/REBOUND/Advanced/Create Player Rig Only", false, 21)]
    [MenuItem("GameObject/REBOUND/Setup Complete Player Rig", false, 10)]
    public static GameObject CreatePlayerRig()
    {
        // 1. Remove existing Player_Rig if present
        GameObject existing = GameObject.Find("Player_Rig");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        // 2. Physical Root Body
        GameObject root = new GameObject("Player_Rig");
        root.layer = LayerMask.NameToLayer("Player") != -1 ? LayerMask.NameToLayer("Player") : 0;
        root.tag = "Player";
        Vector3 initialSpawn = GetSafeSpawnPosition();
        root.transform.position = initialSpawn;

        // Rigidbody: Freeze all rotations so PhysX never applies torque fighting mouse look
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 75f;
        rb.linearDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Capsule Collider with frictionless material to eliminate wall/floor drag
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.height = 2.0f;
        col.radius = 0.45f;
        col.center = Vector3.zero;
        col.material = new PhysicsMaterial("PlayerFrictionless")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };

        // Audio & Locomotion Components
        PlayerAudioEffects audioEffects = root.AddComponent<PlayerAudioEffects>();
        PlayerMovement movement = root.AddComponent<PlayerMovement>();
        PlayerAbilityInput abilityInput = root.AddComponent<PlayerAbilityInput>();
        PlayerMovementTelemetry telemetry = root.AddComponent<PlayerMovementTelemetry>();

        // Ground Check sensor point
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(root.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.95f, 0f);

        // 3. Camera Pivot (Eye Height = 1.65m)
        GameObject pivot = new GameObject("CameraPivot");
        pivot.transform.SetParent(root.transform);
        pivot.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        pivot.transform.localRotation = Quaternion.identity;

        MouseLook mouseLook = pivot.AddComponent<MouseLook>();

        // 4. Main Camera
        Camera existingCam = Camera.main;
        GameObject camObj;
        if (existingCam != null)
        {
            camObj = existingCam.gameObject;
            camObj.transform.SetParent(pivot.transform);
            camObj.transform.localPosition = Vector3.zero;
            camObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            camObj = new GameObject("MainCamera");
            camObj.transform.SetParent(pivot.transform);
            camObj.transform.localPosition = Vector3.zero;
            camObj.transform.localRotation = Quaternion.identity;
            camObj.tag = "MainCamera";
        }

        Camera camComponent = camObj.GetComponent<Camera>();
        if (camComponent == null) camComponent = camObj.AddComponent<Camera>();
        camComponent.nearClipPlane = 0.03f;
        if (camObj.GetComponent<AudioListener>() == null) camObj.AddComponent<AudioListener>();
        camObj.tag = "MainCamera";

        // 5. Ability Ray Origin (0.35m ahead of lens)
        GameObject rayOrigin = new GameObject("AbilityRayOrigin");
        rayOrigin.transform.SetParent(pivot.transform);
        rayOrigin.transform.localPosition = new Vector3(0f, 0f, 0.35f);

        // 6. Left Hand (Push / Wind Cannon)
        GameObject leftHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftHand.name = "LeftHandVisual";
        leftHand.transform.SetParent(pivot.transform);
        leftHand.transform.localPosition = new Vector3(-0.35f, -0.25f, 0.5f);
        leftHand.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        leftHand.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);
        Object.DestroyImmediate(leftHand.GetComponent<Collider>());

        PushAbility push = leftHand.AddComponent<PushAbility>();

        // 7. Right Hand (Pull / Vacuum Vortex)
        GameObject rightHand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightHand.name = "RightHandVisual";
        rightHand.transform.SetParent(pivot.transform);
        rightHand.transform.localPosition = new Vector3(0.35f, -0.25f, 0.5f);
        rightHand.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rightHand.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);
        Object.DestroyImmediate(rightHand.GetComponent<Collider>());

        PullAbility pull = rightHand.AddComponent<PullAbility>();

        // 8. Slow Motion Ability (attached to root)
        SlowMotionAbility slowMo = root.AddComponent<SlowMotionAbility>();

        // 9. Tactical HUD Canvas
        BuildTacticalHUD(root, push, pull, slowMo);

        // 10. Wire Serialized References
        SerializedObject moveSO = new SerializedObject(movement);
        moveSO.FindProperty("currentCheckpointPos").vector3Value = initialSpawn;
        moveSO.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        moveSO.FindProperty("mouseLook").objectReferenceValue = mouseLook;
        moveSO.FindProperty("audioEffects").objectReferenceValue = audioEffects;
        moveSO.FindProperty("pullAbility").objectReferenceValue = pull;
        LayerMask mask = ~LayerMask.GetMask("Player", "Ignore Raycast");
        moveSO.FindProperty("groundLayerMask").intValue = mask.value;
        moveSO.ApplyModifiedProperties();

        SerializedObject lookSO = new SerializedObject(mouseLook);
        lookSO.FindProperty("playerBody").objectReferenceValue = root.transform;
        lookSO.ApplyModifiedProperties();

        SerializedObject pushSO = new SerializedObject(push);
        pushSO.FindProperty("abilityRayOrigin").objectReferenceValue = rayOrigin.transform;
        pushSO.ApplyModifiedProperties();

        SerializedObject pullSO = new SerializedObject(pull);
        pullSO.FindProperty("abilityRayOrigin").objectReferenceValue = rayOrigin.transform;
        pullSO.FindProperty("handTetherOrigin").objectReferenceValue = rightHand.transform;
        pullSO.ApplyModifiedProperties();

        SerializedObject teleSO = new SerializedObject(telemetry);
        teleSO.FindProperty("showOverlay").boolValue = false;
        teleSO.ApplyModifiedProperties();

        Selection.activeGameObject = root;
        Undo.RegisterCreatedObjectUndo(root, "Create Upgraded Player Rig");

        Debug.Log("REBOUND: Complete Player Rig with Ledge Mantling, Tactical HUD, and Procedural Audio successfully created!");
        return root;
    }

    private static void BuildTacticalHUD(GameObject playerRoot, PushAbility push, PullAbility pull, SlowMotionAbility slowMo)
    {
        GameObject canvasObj = new GameObject("AbilityHUD_Canvas");
        canvasObj.transform.SetParent(playerRoot.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        AbilityHUD hud = canvasObj.AddComponent<AbilityHUD>();

        // 1. Focus Vision Overlay (Kept hidden for crystal clear perspective)
        GameObject focusObj = new GameObject("ElectricCyanFocusOverlay");
        focusObj.transform.SetParent(canvasObj.transform, false);
        Image focusImg = focusObj.AddComponent<Image>();
        focusImg.color = new Color(0f, 0f, 0f, 0f);
        focusImg.raycastTarget = false;
        focusObj.SetActive(false);

        RectTransform focusRT = focusObj.GetComponent<RectTransform>();
        focusRT.anchorMin = Vector2.zero;
        focusRT.anchorMax = Vector2.one;
        focusRT.sizeDelta = Vector2.zero;

        // 2. Central Crosshair Root
        GameObject crosshairRoot = new GameObject("CrosshairRoot");
        crosshairRoot.transform.SetParent(canvasObj.transform, false);
        RectTransform chRootRT = crosshairRoot.AddComponent<RectTransform>();
        chRootRT.anchorMin = new Vector2(0.5f, 0.5f);
        chRootRT.anchorMax = new Vector2(0.5f, 0.5f);
        chRootRT.sizeDelta = new Vector2(40f, 40f);

        // Center Dot
        GameObject dotObj = new GameObject("CenterDot");
        dotObj.transform.SetParent(crosshairRoot.transform, false);
        Image dotImg = dotObj.AddComponent<Image>();
        dotImg.color = new Color(1f, 1f, 1f, 0.8f);
        dotImg.raycastTarget = false;
        RectTransform dotRT = dotObj.GetComponent<RectTransform>();
        dotRT.sizeDelta = new Vector2(6f, 6f);

        // 4 Brackets
        Image[] brackets = new Image[4];
        brackets[0] = CreateBracket(crosshairRoot.transform, "Top", new Vector2(0f, 14f), new Vector2(10f, 2f));
        brackets[1] = CreateBracket(crosshairRoot.transform, "Bottom", new Vector2(0f, -14f), new Vector2(10f, 2f));
        brackets[2] = CreateBracket(crosshairRoot.transform, "Left", new Vector2(-14f, 0f), new Vector2(2f, 10f));
        brackets[3] = CreateBracket(crosshairRoot.transform, "Right", new Vector2(14f, 0f), new Vector2(2f, 10f));

        // 3. Target Readout Badge
        GameObject badgeObj = new GameObject("TargetBadge");
        badgeObj.transform.SetParent(canvasObj.transform, false);
        RectTransform badgeRT = badgeObj.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(0f, -32f);
        badgeRT.sizeDelta = new Vector2(280f, 22f);

        Text badgeText = badgeObj.AddComponent<Text>();
        badgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        badgeText.fontSize = 12;
        badgeText.alignment = TextAnchor.MiddleCenter;
        badgeText.color = new Color(0.1f, 0.95f, 1.0f, 0.9f);
        badgeText.text = "";
        badgeObj.SetActive(false);

        // 4. Energy Bar
        GameObject energyObj = new GameObject("SlowMoEnergySlider");
        energyObj.transform.SetParent(canvasObj.transform, false);
        Slider slider = energyObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        RectTransform sliderRT = energyObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.5f, 0.08f);
        sliderRT.anchorMax = new Vector2(0.5f, 0.08f);
        sliderRT.sizeDelta = new Vector2(220f, 8f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(energyObj.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.1f, 0.85f, 1.0f, 0.9f);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.sizeDelta = Vector2.zero;

        slider.targetGraphic = fillImg;
        slider.fillRect = fillRT;

        // 5. Contextual Controls Overlay (Bottom-Left)
        GameObject guideObj = new GameObject("ControlsGuide");
        guideObj.transform.SetParent(canvasObj.transform, false);
        Text guideText = guideObj.AddComponent<Text>();
        guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        guideText.fontSize = 11;
        guideText.alignment = TextAnchor.LowerLeft;
        guideText.color = new Color(1f, 1f, 1f, 0.7f);
        guideText.text = "[SHIFT] Sprint   [C] Slide   [SPACE] Bunny-Hop   [LMB] Push/Boost   [RMB] Pull   [Q/F] Focus";
        RectTransform guideRT = guideObj.GetComponent<RectTransform>();
        guideRT.anchorMin = new Vector2(0f, 0f);
        guideRT.anchorMax = new Vector2(0f, 0f);
        guideRT.pivot = new Vector2(0f, 0f);
        guideRT.anchoredPosition = new Vector2(25f, 25f);
        guideRT.sizeDelta = new Vector2(650f, 28f);

        // 6. Speedometer HUD Text (Bottom-Center above energy bar)
        GameObject speedObj = new GameObject("SpeedometerHUD");
        speedObj.transform.SetParent(canvasObj.transform, false);
        Text speedText = speedObj.AddComponent<Text>();
        speedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        speedText.fontSize = 14;
        speedText.fontStyle = FontStyle.Bold;
        speedText.alignment = TextAnchor.MiddleCenter;
        speedText.color = new Color(0.1f, 0.95f, 1.0f, 0.95f);
        speedText.text = "0.0 m/s";
        RectTransform speedRT = speedObj.GetComponent<RectTransform>();
        speedRT.anchorMin = new Vector2(0.5f, 0.08f);
        speedRT.anchorMax = new Vector2(0.5f, 0.08f);
        speedRT.anchoredPosition = new Vector2(0f, 18f);
        speedRT.sizeDelta = new Vector2(220f, 24f);

        // Wire HUD SO
        SerializedObject hudSO = new SerializedObject(hud);
        hudSO.FindProperty("crosshairDot").objectReferenceValue = dotImg;
        SerializedProperty bracketsProp = hudSO.FindProperty("crosshairBrackets");
        bracketsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            bracketsProp.GetArrayElementAtIndex(i).objectReferenceValue = brackets[i];
        }
        hudSO.FindProperty("targetBadgeRoot").objectReferenceValue = badgeObj;
        hudSO.FindProperty("targetBadgeText").objectReferenceValue = badgeText;
        hudSO.FindProperty("focusOverlay").objectReferenceValue = focusImg;
        hudSO.FindProperty("energyBar").objectReferenceValue = slider;
        hudSO.FindProperty("speedometerText").objectReferenceValue = speedText;
        hudSO.FindProperty("controlsGuideText").objectReferenceValue = guideText;
        hudSO.FindProperty("pushAbility").objectReferenceValue = push;
        hudSO.FindProperty("pullAbility").objectReferenceValue = pull;
        hudSO.FindProperty("slowMotionAbility").objectReferenceValue = slowMo;
        hudSO.ApplyModifiedProperties();
    }

    private static Image CreateBracket(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject("Bracket_" + name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.65f);
        img.raycastTarget = false;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return img;
    }

    [MenuItem("Tools/REBOUND/Advanced/Create Arena Only (200m)", false, 22)]
    [MenuItem("GameObject/REBOUND/Create Redliner Grapple Arena (200m)", false, 10)]
    public static void CreateRedlinerGrappleArena()
    {
        GameObject existing = GameObject.Find("TestRoom");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject room = new GameObject("TestRoom");
        int envLayer = LayerMask.NameToLayer("Environment") != -1 ? LayerMask.NameToLayer("Environment") : 0;
        int propLayer = LayerMask.NameToLayer("Interactables") != -1 ? LayerMask.NameToLayer("Interactables") : 0;
        int hazardLayer = LayerMask.NameToLayer("Hazards") != -1 ? LayerMask.NameToLayer("Hazards") : 0;

        // 1. Massive Arena Floor (200m x 200m centered at Z = 60m)
        CreatePlatform(room.transform, envLayer, "Arena_MainFloor", new Vector3(0f, -0.5f, 60f), new Vector3(200f, 1f, 200f));

        // 2. High Perimeter Boundary Walls (35m tall, enclosing 200m x 200m arena)
        CreateWall(room.transform, envLayer, new Vector3(0f, 17f, 160f), new Vector3(200f, 35f, 2f)); // North
        CreateWall(room.transform, envLayer, new Vector3(0f, 17f, -40f), new Vector3(200f, 35f, 2f)); // South
        CreateWall(room.transform, envLayer, new Vector3(-100f, 17f, 60f), new Vector3(2f, 35f, 200f)); // West
        CreateWall(room.transform, envLayer, new Vector3(100f, 17f, 60f), new Vector3(2f, 35f, 200f)); // East

        // 3. Central Staging Hub (Y = 8m, 22m x 22m elevated platform)
        GameObject startHub = CreatePlatform(room.transform, envLayer, "Chamber1_StartHub", new Vector3(0f, 8.0f, 0f), new Vector3(22f, 1f, 22f));

        // Surrounding safety rails on start hub
        CreateWall(room.transform, envLayer, new Vector3(-11f, 9.5f, 0f), new Vector3(0.5f, 2f, 22f));
        CreateWall(room.transform, envLayer, new Vector3(11f, 9.5f, 0f), new Vector3(0.5f, 2f, 22f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 9.5f, -11f), new Vector3(22f, 2f, 0.5f));

        // 4. Downhill Speed Chute from Start Hub (20-degree incline, launching directly into 25+ m/s slides)
        GameObject speedChuteNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        speedChuteNorth.name = "StartHub_DownhillChute_North";
        speedChuteNorth.layer = envLayer;
        speedChuteNorth.transform.SetParent(room.transform);
        speedChuteNorth.transform.position = new Vector3(0f, 4.0f, 22f);
        speedChuteNorth.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        speedChuteNorth.transform.localScale = new Vector3(14f, 0.5f, 26f);

        // 5. Central Grapple Spire (36m Tall!) with top overhang anchor brackets
        CreatePlatform(room.transform, envLayer, "GrappleSpire_Central", new Vector3(0f, 18f, 60f), new Vector3(4f, 36f, 4f));
        CreatePlatform(room.transform, envLayer, "Spire_Bracket_N", new Vector3(0f, 34f, 64f), new Vector3(6f, 1f, 4f));
        CreatePlatform(room.transform, envLayer, "Spire_Bracket_S", new Vector3(0f, 34f, 56f), new Vector3(6f, 1f, 4f));
        CreatePlatform(room.transform, envLayer, "Spire_Bracket_W", new Vector3(-4f, 34f, 60f), new Vector3(4f, 1f, 6f));
        CreatePlatform(room.transform, envLayer, "Spire_Bracket_E", new Vector3(4f, 34f, 60f), new Vector3(4f, 1f, 6f));

        // 6. Four High Quadrant Grapple Towers (28m Tall) for pendulum swinging
        CreatePlatform(room.transform, envLayer, "GrappleTower_NW", new Vector3(-55f, 14f, 25f), new Vector3(5f, 28f, 5f));
        CreatePlatform(room.transform, envLayer, "GrappleTower_NE", new Vector3(55f, 14f, 25f), new Vector3(5f, 28f, 5f));
        CreatePlatform(room.transform, envLayer, "GrappleTower_SW", new Vector3(-55f, 14f, 95f), new Vector3(5f, 28f, 5f));
        CreatePlatform(room.transform, envLayer, "GrappleTower_SE", new Vector3(55f, 14f, 95f), new Vector3(5f, 28f, 5f));

        // 7. High Aerial Swing Gantries (Y = 25m) connecting towers across the sky
        CreatePlatform(room.transform, envLayer, "Gantry_WestSpan", new Vector3(-55f, 26f, 60f), new Vector3(2f, 1.5f, 75f));
        CreatePlatform(room.transform, envLayer, "Gantry_EastSpan", new Vector3(55f, 26f, 60f), new Vector3(2f, 1.5f, 75f));
        CreatePlatform(room.transform, envLayer, "Gantry_NorthCross", new Vector3(0f, 26f, 95f), new Vector3(115f, 1.5f, 2f));
        CreatePlatform(room.transform, envLayer, "Gantry_SouthCross", new Vector3(0f, 26f, 25f), new Vector3(115f, 1.5f, 2f));

        // 8. Mega Slide Surfing Ramp (Drops from 16m down to 0m at 26 degrees)
        GameObject megaRamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        megaRamp.name = "MegaSlide_Ramp_West";
        megaRamp.layer = envLayer;
        megaRamp.transform.SetParent(room.transform);
        megaRamp.transform.position = new Vector3(-25f, 8f, 60f);
        megaRamp.transform.rotation = Quaternion.Euler(26f, 0f, 0f);
        megaRamp.transform.localScale = new Vector3(12f, 0.8f, 38f);

        // 9. Aerial Ski-Jump Launch Ramp (18-degree upward kicker for launching across the arena)
        GameObject skiJump1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skiJump1.name = "SkiJump_LaunchRamp_East";
        skiJump1.layer = envLayer;
        skiJump1.transform.SetParent(room.transform);
        skiJump1.transform.position = new Vector3(25f, 2.5f, 45f);
        skiJump1.transform.rotation = Quaternion.Euler(-18f, 0f, 0f);
        skiJump1.transform.localScale = new Vector3(12f, 0.6f, 18f);

        // 10. High Observation Sky-Deck (Y = 18m, size 25m x 25m)
        CreatePlatform(room.transform, envLayer, "SkyDeck_Observation_North", new Vector3(0f, 17.5f, 135f), new Vector3(25f, 1f, 25f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 19.5f, 147.5f), new Vector3(25f, 3f, 0.5f));

        // 11. Destructible Barricades & Bowling Pin Crates for Slide Ramming & Crate Cannon
        for (int row = 0; row < 5; row++)
        {
            CreateBarricadePlank(room.transform, propLayer, new Vector3(0f, 0.6f + row * 1.1f, 85f), new Vector3(6f, 1f, 0.4f));
        }

        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_1", WeightClass.Light, new Vector3(0f, 0.5f, 95f), Color.cyan);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_2", WeightClass.Light, new Vector3(-2f, 0.5f, 98f), Color.cyan);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_3", WeightClass.Light, new Vector3(2f, 0.5f, 98f), Color.cyan);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_4", WeightClass.Medium, new Vector3(-4f, 0.5f, 101f), Color.yellow);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_5", WeightClass.Medium, new Vector3(0f, 0.5f, 101f), Color.yellow);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_6", WeightClass.Medium, new Vector3(4f, 0.5f, 101f), Color.yellow);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_7", WeightClass.Heavy, new Vector3(-6f, 0.5f, 104f), Color.red);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_8", WeightClass.Heavy, new Vector3(-2f, 0.5f, 104f), Color.red);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_9", WeightClass.Heavy, new Vector3(2f, 0.5f, 104f), Color.red);
        CreateTestCrate(room.transform, propLayer, "Bowling_Crate_10", WeightClass.Heavy, new Vector3(6f, 0.5f, 104f), Color.red);

        // 12. Interactive Target Switches for Swing-and-Shoot Target Practice
        CreateTargetButton(room.transform, propLayer, "Target_Spire_Apex", new Vector3(0f, 30f, 57.5f), Quaternion.identity, new Vector3(3.5f, 3.5f, 0.5f));
        CreateTargetButton(room.transform, propLayer, "Target_Tower_NW", new Vector3(-55f, 20f, 28f), Quaternion.identity, new Vector3(3f, 3f, 0.5f));
        CreateTargetButton(room.transform, propLayer, "Target_Tower_NE", new Vector3(55f, 20f, 28f), Quaternion.identity, new Vector3(3f, 3f, 0.5f));
        CreateTargetButton(room.transform, propLayer, "Target_MegaRamp_Goal", new Vector3(-25f, 3f, 82f), Quaternion.identity, new Vector3(3.5f, 3.5f, 0.5f));
        CreateTargetButton(room.transform, propLayer, "Target_SkiJump_Aerial", new Vector3(25f, 10f, 75f), Quaternion.identity, new Vector3(4f, 4f, 0.5f));
        CreateTargetButton(room.transform, propLayer, "Target_SkyDeck_North", new Vector3(0f, 21f, 147f), Quaternion.identity, new Vector3(4f, 4f, 0.5f));

        // 13. Global Hazard Reset Trigger
        GameObject pitHazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pitHazard.name = "Global_PitHazardTrigger";
        pitHazard.layer = hazardLayer;
        pitHazard.transform.SetParent(room.transform);
        pitHazard.transform.position = new Vector3(0f, -12.0f, 60f);
        pitHazard.transform.localScale = new Vector3(280f, 2f, 280f);

        BoxCollider pitCol = pitHazard.GetComponent<BoxCollider>();
        if (pitCol != null) pitCol.isTrigger = true;
        Renderer pitRend = pitHazard.GetComponent<Renderer>();
        if (pitRend != null) pitRend.enabled = false;

        // Position player at Start Hub
        GameObject playerObj = GameObject.Find("Player_Rig");
        if (playerObj != null)
        {
            Vector3 startPos = new Vector3(0f, 9.6f, -2.0f);
            playerObj.transform.position = startPos;
            playerObj.transform.rotation = Quaternion.identity;

            PlayerMovement pMove = playerObj.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.SetCheckpoint(startPos);
            }
        }

        Undo.RegisterCreatedObjectUndo(room, "Create Redliner Grapple Arena");
        Debug.Log("REBOUND: Redliner Grapple Arena (200m x 200m x 35m) created! 36m Central Spire, 4 High Towers, Gantries, Mega Ramps & Bowling Alley!");
    }

    [MenuItem("Tools/REBOUND/Advanced/Create Test Physics Room", false, 23)]
    [MenuItem("GameObject/REBOUND/Create Test Physics Room", false, 11)]
    public static void CreateTestRoom()
    {
        GameObject existing = GameObject.Find("TestRoom");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject room = new GameObject("TestRoom");
        int envLayer = LayerMask.NameToLayer("Environment") != -1 ? LayerMask.NameToLayer("Environment") : 0;
        int propLayer = LayerMask.NameToLayer("Interactables") != -1 ? LayerMask.NameToLayer("Interactables") : 0;
        int hazardLayer = LayerMask.NameToLayer("Hazards") != -1 ? LayerMask.NameToLayer("Hazards") : 0;

        // =========================================================================
        // CHAMBER 1: Starting Hub & Downhill Speed Chute (25° slope for 25+ m/s)
        // =========================================================================
        GameObject startHub = CreatePlatform(room.transform, envLayer, "Chamber1_StartHub", new Vector3(0f, 6.0f, 0f), new Vector3(12f, 1f, 12f));

        // Surrounding safety rails on start hub
        CreateWall(room.transform, envLayer, new Vector3(-6f, 7.5f, 0f), new Vector3(0.5f, 2f, 12f));
        CreateWall(room.transform, envLayer, new Vector3(6f, 7.5f, 0f), new Vector3(0.5f, 2f, 12f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 7.5f, -6f), new Vector3(12f, 2f, 0.5f));

        // Downhill Speed Chute (22-degree incline, 24m long, dropping from Y=6 to Y=0)
        GameObject speedChute = GameObject.CreatePrimitive(PrimitiveType.Cube);
        speedChute.name = "Chamber1_DownhillSpeedChute";
        speedChute.layer = envLayer;
        speedChute.transform.SetParent(room.transform);
        speedChute.transform.position = new Vector3(0f, 3.0f, 17f);
        speedChute.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        speedChute.transform.localScale = new Vector3(8f, 0.5f, 24f);

        // =========================================================================
        // CHAMBER 2: Low Duct Gauntlet (1.3m clearance testing slide ducking)
        // =========================================================================
        CreatePlatform(room.transform, envLayer, "Chamber2_RunwayFloor", new Vector3(0f, -0.5f, 42f), new Vector3(8f, 1f, 24f));

        // Tunnel roof positioned at Y = 1.3m (requires crouching/sliding)
        GameObject tunnelRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tunnelRoof.name = "Chamber2_LowDuctRoof_1.3m";
        tunnelRoof.layer = envLayer;
        tunnelRoof.transform.SetParent(room.transform);
        tunnelRoof.transform.position = new Vector3(0f, 1.35f, 42f);
        tunnelRoof.transform.localScale = new Vector3(8.5f, 0.3f, 16f);

        // Tunnel side walls
        CreateWall(room.transform, envLayer, new Vector3(-4.2f, 1.5f, 42f), new Vector3(0.4f, 3f, 24f));
        CreateWall(room.transform, envLayer, new Vector3(4.2f, 1.5f, 42f), new Vector3(0.4f, 3f, 24f));

        // =========================================================================
        // CHAMBER 3: Bunny-Hop Pit Gap (12-meter void requiring jump out of slide)
        // =========================================================================
        CreatePlatform(room.transform, envLayer, "Chamber3_ApproachRunway", new Vector3(0f, -0.5f, 60f), new Vector3(8f, 1f, 12f));

        // Void from Z = 66m to Z = 78m (12m chasm)
        CreatePlatform(room.transform, envLayer, "Chamber3_LandingPlatform", new Vector3(0f, -0.5f, 85f), new Vector3(10f, 1f, 14f));

        // =========================================================================
        // CHAMBER 4: Wall-Kick Alley (Narrow corridor for air-dash wall kicks)
        // =========================================================================
        CreatePlatform(room.transform, envLayer, "Chamber4_AlleyFloor", new Vector3(0f, -0.5f, 115f), new Vector3(6f, 1f, 44f));

        // Alternating kick walls
        CreateWall(room.transform, envLayer, new Vector3(-2.8f, 3f, 102f), new Vector3(0.5f, 6f, 10f));
        CreateWall(room.transform, envLayer, new Vector3(2.8f, 3f, 115f), new Vector3(0.5f, 6f, 10f));
        CreateWall(room.transform, envLayer, new Vector3(-2.8f, 3f, 128f), new Vector3(0.5f, 6f, 10f));

        // =========================================================================
        // CHAMBER 5: Crate Cannon Barricade (Wooden barrier smash)
        // =========================================================================
        CreatePlatform(room.transform, envLayer, "Chamber5_BarricadeChamber", new Vector3(0f, -0.5f, 150f), new Vector3(12f, 1f, 26f));

        // Doorframe
        CreateWall(room.transform, envLayer, new Vector3(-3.5f, 3f, 152f), new Vector3(3f, 6f, 1f));
        CreateWall(room.transform, envLayer, new Vector3(3.5f, 3f, 152f), new Vector3(3f, 6f, 1f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 5.5f, 152f), new Vector3(4f, 1.5f, 1f));

        // Stacked Breakable Planks blocking doorway (Z = 152m)
        CreateBarricadePlank(room.transform, propLayer, new Vector3(0f, 0.6f, 152f), new Vector3(3.8f, 1f, 0.4f));
        CreateBarricadePlank(room.transform, propLayer, new Vector3(0f, 1.7f, 152f), new Vector3(3.8f, 1f, 0.4f));
        CreateBarricadePlank(room.transform, propLayer, new Vector3(0f, 2.8f, 152f), new Vector3(3.8f, 1f, 0.4f));
        CreateBarricadePlank(room.transform, propLayer, new Vector3(0f, 3.9f, 152f), new Vector3(3.8f, 1f, 0.4f));

        // Ammo crates to pick up with Pull (RMB) and fire with Crate Cannon (LMB)
        CreateTestCrate(room.transform, propLayer, "Ammo_Light_Crate", WeightClass.Light, new Vector3(-2.5f, 0.5f, 142f), Color.cyan);
        CreateTestCrate(room.transform, propLayer, "Ammo_Medium_Crate", WeightClass.Medium, new Vector3(0f, 0.5f, 140f), Color.yellow);
        CreateTestCrate(room.transform, propLayer, "Ammo_Heavy_Block", WeightClass.Heavy, new Vector3(2.5f, 0.5f, 142f), Color.red);

        // =========================================================================
        // CHAMBER 6: Ski-Jump Launch & Final Testing Arena (50m x 50m)
        // =========================================================================
        // Ski-Jump ramp (slopes UPWARD at 16 degrees to launch player sky-high)
        GameObject skiJump = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skiJump.name = "Chamber6_SkiJumpRamp";
        skiJump.layer = envLayer;
        skiJump.transform.SetParent(room.transform);
        skiJump.transform.position = new Vector3(0f, 1.6f, 170f);
        skiJump.transform.rotation = Quaternion.Euler(-16f, 0f, 0f);
        skiJump.transform.localScale = new Vector3(8f, 0.5f, 14f);

        // Grand Arena Floor (50m x 50m)
        CreatePlatform(room.transform, envLayer, "Chamber6_ArenaFloor", new Vector3(0f, -0.5f, 205f), new Vector3(50f, 1f, 50f));

        // High Arena Platform (Height = 6m)
        CreatePlatform(room.transform, envLayer, "Chamber6_HighObservationPlatform", new Vector3(0f, 5.5f, 215f), new Vector3(14f, 1f, 14f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 2.5f, 207f), new Vector3(14f, 5f, 0.5f));

        // Arena boundary walls
        CreateWall(room.transform, envLayer, new Vector3(25f, 4f, 205f), new Vector3(1f, 8f, 50f));
        CreateWall(room.transform, envLayer, new Vector3(-25f, 4f, 205f), new Vector3(1f, 8f, 50f));
        CreateWall(room.transform, envLayer, new Vector3(0f, 4f, 230f), new Vector3(50f, 8f, 1f));

        // Arena Playground Crates
        CreateTestCrate(room.transform, propLayer, "Arena_Crate_1", WeightClass.Light, new Vector3(-10f, 0.5f, 195f), Color.cyan);
        CreateTestCrate(room.transform, propLayer, "Arena_Crate_2", WeightClass.Medium, new Vector3(10f, 0.5f, 195f), Color.yellow);
        CreateTestCrate(room.transform, propLayer, "Arena_Crate_3", WeightClass.Light, new Vector3(0f, 6.5f, 215f), Color.cyan);

        // =========================================================================
        // GLOBAL HAZARD PIT TRIGGER (Underneath track: instant respawn on fall)
        // =========================================================================
        GameObject pitHazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pitHazard.name = "Global_PitHazardTrigger";
        pitHazard.layer = hazardLayer;
        pitHazard.transform.SetParent(room.transform);
        pitHazard.transform.position = new Vector3(0f, -7.0f, 110f);
        pitHazard.transform.localScale = new Vector3(120f, 2f, 260f);

        BoxCollider pitCol = pitHazard.GetComponent<BoxCollider>();
        if (pitCol != null) pitCol.isTrigger = true;

        // Make hazard transparent/invisible
        Renderer pitRend = pitHazard.GetComponent<Renderer>();
        if (pitRend != null) pitRend.enabled = false;

        // Position player at Start Hub
        GameObject playerObj = GameObject.Find("Player_Rig");
        if (playerObj != null)
        {
            Vector3 startPos = new Vector3(0f, 7.6f, -2.5f);
            playerObj.transform.position = startPos;
            playerObj.transform.rotation = Quaternion.identity;

            PlayerMovement pMove = playerObj.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.SetCheckpoint(startPos);
            }
        }

        Undo.RegisterCreatedObjectUndo(room, "Create 6-Chamber Flow Course");
        Debug.Log("REBOUND: 6-Chamber Linear Flow Map created! (Chutes, Low Duct, Bunny-Hop Gap, Wall-Kick Alley, Barricade, Ski-Jump Arena & Hazard Reset)");
    }

    [InitializeOnLoadMethod]
    private static void AutoEnsurePlayerSafeSpawn()
    {
        EditorApplication.delayCall += () =>
        {
            GameObject testRoom = GameObject.Find("TestRoom");
            if (testRoom == null || testRoom.transform.Find("Arena_MainFloor") == null || testRoom.transform.Find("Target_Spire_Apex") == null)
            {
                Debug.Log("REBOUND: Upgrading scene to 200m Redliner Grapple Arena (36m Spire, 4 Towers, Mega Ramps & Target Switches)...");
                CreateRedlinerGrappleArena();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                return;
            }

            GameObject playerObj = GameObject.Find("Player_Rig");
            if (playerObj == null) return;

            GameObject startHub = GameObject.Find("Chamber1_StartHub");
            if (startHub != null)
            {
                float hubTop = startHub.transform.position.y + (startHub.transform.localScale.y * 0.5f);
                if (playerObj.transform.position.y < hubTop || (playerObj.transform.position - startHub.transform.position).sqrMagnitude > 400f)
                {
                    Vector3 safePos = new Vector3(startHub.transform.position.x, hubTop + 1.1f, startHub.transform.position.z - 2.5f);
                    playerObj.transform.position = safePos;
                    playerObj.transform.rotation = Quaternion.identity;

                    PlayerMovement pMove = playerObj.GetComponent<PlayerMovement>();
                    if (pMove != null) pMove.SetCheckpoint(safePos);

                    Debug.Log($"REBOUND: Automatically relocated Player_Rig to safe spawn on Chamber 1 StartHub: {safePos}");
                }
            }
        };
    }

    private static GameObject CreatePlatform(Transform parent, int layer, string name, Vector3 pos, Vector3 scale)
    {
        GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat.name = name;
        plat.layer = layer;
        plat.transform.SetParent(parent);
        plat.transform.position = pos;
        plat.transform.localScale = scale;
        return plat;
    }

    private static void CreateBarricadePlank(Transform parent, int layer, Vector3 pos, Vector3 scale)
    {
        GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plank.name = "Barricade_Plank";
        plank.layer = layer;
        plank.transform.SetParent(parent);
        plank.transform.position = pos;
        plank.transform.localScale = scale;

        Rigidbody rb = plank.AddComponent<Rigidbody>();
        rb.mass = 1.5f;

        InteractableProp prop = plank.AddComponent<InteractableProp>();
        SerializedObject so = new SerializedObject(prop);
        so.FindProperty("weightClass").enumValueIndex = (int)WeightClass.Light;
        so.ApplyModifiedProperties();

        Renderer rend = plank.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.6f, 0.4f, 0.2f); // Wood brown
            rend.material = mat;
        }
    }

    private static void CreateWall(Transform parent, int layer, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.layer = layer;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
    }

    private static void CreateTestCrate(Transform parent, int layer, string name, WeightClass wc, Vector3 pos, Color color)
    {
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = name;
        crate.layer = layer;
        crate.transform.SetParent(parent);
        crate.transform.position = pos;

        Rigidbody rb = crate.AddComponent<Rigidbody>();
        InteractableProp prop = crate.AddComponent<InteractableProp>();

        SerializedObject so = new SerializedObject(prop);
        so.FindProperty("weightClass").enumValueIndex = (int)wc;
        so.ApplyModifiedProperties();

        Renderer rend = crate.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            rend.material = mat;
        }
    }

    private static GameObject CreateTargetButton(Transform parent, int layer, string name, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.layer = layer;
        buttonObj.transform.SetParent(parent);
        buttonObj.transform.position = pos;
        buttonObj.transform.rotation = rot;

        // Base frame plate
        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "FramePlate";
        frame.layer = layer;
        frame.transform.SetParent(buttonObj.transform, false);
        frame.transform.localScale = scale;

        Renderer frameRend = frame.GetComponent<Renderer>();
        if (frameRend != null)
        {
            Material frameMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            frameMat.color = new Color(0.2f, 0.22f, 0.26f); // Dark industrial casing
            frameRend.material = frameMat;
        }

        // Glowing center core (Cylinder facing forward along Z)
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        core.name = "GlowCore";
        core.layer = layer;
        core.transform.SetParent(buttonObj.transform, false);
        core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        core.transform.localPosition = new Vector3(0f, 0f, -scale.z * 0.35f);
        core.transform.localScale = new Vector3(scale.x * 0.65f, scale.z * 0.5f, scale.y * 0.65f);

        MeshRenderer coreRend = core.GetComponent<MeshRenderer>();
        if (coreRend != null)
        {
            Material coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            coreMat.color = new Color(1.0f, 0.18f, 0.18f, 1.0f); // Bright Red
            coreRend.material = coreMat;
        }

        // Add TargetButton logic
        TargetButton tb = buttonObj.AddComponent<TargetButton>();
        SerializedObject so = new SerializedObject(tb);
        so.FindProperty("buttonName").stringValue = name;
        so.FindProperty("buttonCoreRenderer").objectReferenceValue = coreRend;
        so.FindProperty("movingCore").objectReferenceValue = core.transform;
        so.FindProperty("depressDistance").floatValue = 0.2f;
        so.ApplyModifiedProperties();

        // BoxCollider on the root object so rays and flying crates hit it cleanly
        BoxCollider boxCol = buttonObj.AddComponent<BoxCollider>();
        boxCol.size = scale + new Vector3(0.2f, 0.2f, 0.3f);

        return buttonObj;
    }
}
#endif
