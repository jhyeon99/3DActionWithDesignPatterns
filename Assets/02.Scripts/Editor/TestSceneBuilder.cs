using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 플레이어 / 몬스터 / 증강 UI / 디버그 오버레이를 한 번에 확인할 수 있는 통합 테스트 씬을 만든다.
// 메뉴: Tools > Build Integrated Test Scene
public static class TestSceneBuilder
{
    private const string ScenePath = "Assets/01.Scenes/IntegratedTestScene.unity";
    private const string PlayerPrefabPath = "Assets/03.Prefabs/Player/Player.prefab";
    private const string BulletPrefabPath = "Assets/03.Prefabs/PF_PlayerBullet.prefab";
    private const string GroundMaterialPath = "Assets/09.Materials/GroundMaterial.mat";
    private const string FontPath = "Assets/08.Fonts/LevelUp-Regular SDF.asset";

    private static readonly string[] MonsterPrefabPaths =
    {
        "Assets/03.Prefabs/NormalMonster.prefab",
        "Assets/03.Prefabs/DashMonster.prefab",
        "Assets/03.Prefabs/DebuffMonster.prefab",
    };

    private const float ArenaHalfSize = 48f;
    private const float SpawnRadius = 28f;
    private const int SpawnPointCount = 8;

    private static TMP_FontAsset font;

    [MenuItem("Tools/Build Integrated Test Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
        if (playerPrefab == null || bulletPrefab == null)
        {
            Debug.LogError("[TestSceneBuilder] Player / Bullet prefab not found.");
            return;
        }

        GameObject[] monsterPrefabs = new GameObject[MonsterPrefabPaths.Length];
        for (int i = 0; i < monsterPrefabs.Length; i++)
        {
            monsterPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(MonsterPrefabPaths[i]);
            if (monsterPrefabs[i] == null)
            {
                Debug.LogError($"[TestSceneBuilder] Monster prefab not found: {MonsterPrefabPaths[i]}");
                return;
            }
        }

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLight();
        CreateArena();
        GameObject player = CreatePlayer(playerPrefab, bulletPrefab);
        CreateCamera(player);
        Transform[] spawnPoints = CreateSpawnPoints();
        CreateGameManager(monsterPrefabs, spawnPoints);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log($"[TestSceneBuilder] Created {ScenePath}");
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateArena()
    {
        Material ground = AssetDatabase.LoadAssetAtPath<Material>(GroundMaterialPath);

        GameObject root = new GameObject("Arena");

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.localScale = new Vector3(ArenaHalfSize * 2f / 10f, 1f, ArenaHalfSize * 2f / 10f);
        if (ground != null)
        {
            floor.GetComponent<MeshRenderer>().sharedMaterial = ground;
        }

        // 몬스터가 아레나 밖으로 나가지 않도록 사방에 벽을 세운다.
        CreateWall(root.transform, "Wall_North", new Vector3(0f, 2f, ArenaHalfSize), new Vector3(ArenaHalfSize * 2f, 4f, 1f));
        CreateWall(root.transform, "Wall_South", new Vector3(0f, 2f, -ArenaHalfSize), new Vector3(ArenaHalfSize * 2f, 4f, 1f));
        CreateWall(root.transform, "Wall_East", new Vector3(ArenaHalfSize, 2f, 0f), new Vector3(1f, 4f, ArenaHalfSize * 2f));
        CreateWall(root.transform, "Wall_West", new Vector3(-ArenaHalfSize, 2f, 0f), new Vector3(1f, 4f, ArenaHalfSize * 2f));
    }

    private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
    }

    private static GameObject CreatePlayer(GameObject playerPrefab, GameObject bulletPrefab)
    {
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0.87f, 0f);

        Player playerComponent = player.AddComponent<Player>();
        // GameManager의 테스트 키(1~4)와 겹치지 않게 변경
        playerComponent.HealKey = KeyCode.H;
        playerComponent.DamageKey = KeyCode.J;

        player.AddComponent<PlayerMove>();

        PlayerFire fire = player.AddComponent<PlayerFire>();
        fire.BulletPrefab = bulletPrefab;

        return player;
    }

    private static void CreateCamera(GameObject player)
    {
        // CameraFollow는 target의 위치로 이동하므로, 플레이어 머리 위(CameraPosition)를 target으로 쓴다.
        Transform target = player.transform.Find("CameraPosition");
        if (target == null)
        {
            target = player.transform;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = target.position;
        cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
        cameraObject.AddComponent<AudioListener>();

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        SerializedObject followObject = new SerializedObject(follow);
        followObject.FindProperty("_target").objectReferenceValue = target;
        followObject.ApplyModifiedPropertiesWithoutUndo();

        player.GetComponent<PlayerMove>().CameraTransform = cameraObject.transform;
    }

    private static Transform[] CreateSpawnPoints()
    {
        GameObject root = new GameObject("SpawnPoints");
        Transform[] points = new Transform[SpawnPointCount];

        for (int i = 0; i < SpawnPointCount; i++)
        {
            float angle = Mathf.PI * 2f * i / SpawnPointCount;
            GameObject point = new GameObject($"SpawnPoint_{i}");
            point.transform.SetParent(root.transform);
            point.transform.position = new Vector3(Mathf.Cos(angle) * SpawnRadius, 1f, Mathf.Sin(angle) * SpawnRadius);
            points[i] = point.transform;
        }

        return points;
    }

    private static void CreateGameManager(GameObject[] monsterPrefabs, Transform[] spawnPoints)
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(null);

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvasTransform = canvasObject.transform;

        GameObject managerObject = new GameObject("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();

        // 게임 UI
        manager.CurrentTimeText = CreateText(canvasTransform, "CurrentTimeText", "00:00", 44, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(300f, 60f));
        manager.LevelText = CreateText(canvasTransform, "LevelText", "1", 40, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(-360f, -100f), new Vector2(100f, 50f));
        manager.ExperienceText = CreateText(canvasTransform, "ExperienceText", "0 / 100", 24, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(300f, 30f));
        manager.ScoreText = CreateText(canvasTransform, "ScoreText", "0", 44, TextAlignmentOptions.Right,
            new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(300f, 60f));
        manager.ExperienceSlider = CreateSlider(canvasTransform, "ExperienceSlider",
            new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(600f, 24f));

        // 증강 선택 패널
        manager.UpgradePanel = CreatePanel(canvasTransform, "UpgradePanel", 0.75f);
        CreateText(manager.UpgradePanel.transform, "Title", "증강 선택", 56, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(600f, 80f));
        manager.UpgradeButtons = new Button[3];
        manager.UpgradeLabels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            manager.UpgradeButtons[i] = CreateButton(manager.UpgradePanel.transform, $"UpgradeButton{i + 1}", "",
                new Vector2((i - 1) * 420f, 0f), new Vector2(380f, 260f), 34, out TMP_Text label);
            manager.UpgradeLabels[i] = label;
        }

        // 게임 오버 패널
        manager.GameOverPanel = CreatePanel(canvasTransform, "GameOverPanel", 0.8f);
        CreateText(manager.GameOverPanel.transform, "GameOverText", "GAME OVER", 80, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(900f, 120f));
        Button restartButton = CreateButton(manager.GameOverPanel.transform, "RestartButton", "다시 시작 (R)",
            new Vector2(0f, -60f), new Vector2(360f, 90f), 36, out _);
        UnityEventTools.AddVoidPersistentListener(restartButton.onClick, manager.RestartRun);

        // 일시 정지 패널
        manager.PausePanel = CreatePanel(canvasTransform, "PausePanel", 0.6f);
        CreateText(manager.PausePanel.transform, "PauseText", "PAUSED", 80, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(900f, 120f));
        Button resumeButton = CreateButton(manager.PausePanel.transform, "ResumeButton", "계속하기 (ESC)",
            new Vector2(0f, -60f), new Vector2(360f, 90f), 36, out _);
        UnityEventTools.AddVoidPersistentListener(resumeButton.onClick, manager.GameResume);

        manager.MonsterPrefabs = monsterPrefabs;
        manager.SpawnPoints = spawnPoints;

        // GameManager.Start에서 GameStart가 패널을 다시 숨기지만, 저장되는 초기 상태도 숨김으로 맞춘다.
        manager.UpgradePanel.SetActive(false);
        manager.GameOverPanel.SetActive(false);
        manager.PausePanel.SetActive(false);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize,
        TextAlignmentOptions alignment, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }

        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObject.transform, false);
        Stretch(background.GetComponent<RectTransform>());
        background.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(sliderObject.transform, false);
        Stretch(fill.GetComponent<RectTransform>());
        fill.GetComponent<Image>().color = new Color(0.3f, 0.85f, 0.4f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = null;
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 0f;
        return slider;
    }

    private static GameObject CreatePanel(Transform parent, string name, float alpha)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, alpha);
        return panel;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size,
        float fontSize, out TMP_Text labelText)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Label", label, fontSize, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), Vector2.zero, size);
        Stretch(text.rectTransform);
        labelText = text;
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AddToBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == ScenePath)
            {
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
