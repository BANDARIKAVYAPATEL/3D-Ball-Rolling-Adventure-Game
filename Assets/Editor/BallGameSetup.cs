using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Builds the game scene (ball, camera, HUD, win panel, level generator) the first time
// the project opens. The levels themselves are generated at Play time by LevelGenerator.
// You can rebuild the scene any time from the menu: Ball Game > Build Scene.
[InitializeOnLoad]
public static class BallGameSetup
{
    const string ScenePath = "Assets/Scenes/BallLevel.unity";

    static BallGameSetup()
    {
        EditorApplication.delayCall += AutoRun;
    }

    static void AutoRun()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;

        if (!File.Exists(ScenePath))
        {
            BuildLevel();
            return;
        }

        if (!SessionState.GetBool("BallGameOpened", false))
        {
            SessionState.SetBool("BallGameOpened", true);
            var active = EditorSceneManager.GetActiveScene();
            if (active.path != ScenePath && !active.isDirty)
                EditorSceneManager.OpenScene(ScenePath);
        }
    }

    [MenuItem("Ball Game/Build Scene")]
    public static void BuildLevel()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ---------- ball ----------
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -12f);
        player.GetComponent<Renderer>().sharedMaterial = BallMaterial();
        player.AddComponent<Rigidbody>();
        player.AddComponent<BallController>();

        // ---------- camera ----------
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraFollow follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.target = player.transform;
            cam.transform.position = player.transform.position + follow.offset;
            cam.transform.LookAt(player.transform.position);
        }

        // ---------- UI ----------
        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Text coinText = MakeText("CoinText", canvasGo.transform, "COINS: 0 / 0", 56, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(700f, 90f), Color.white);
        Text levelText = MakeText("LevelText", canvasGo.transform, "LEVEL 1", 60, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(1100f, 100f), new Color(1f, 0.8f, 0.1f));
        Text timeText = MakeText("TimeText", canvasGo.transform, "", 52, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(600f, 80f), Color.white);
        MakeText("HelpText", canvasGo.transform, "Roll: W A S D / Arrows    M: music", 32, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(700f, 60f), new Color(1f, 1f, 1f, 0.85f));
        Text hintText = MakeText("HintText", canvasGo.transform, "", 48, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(1500f, 100f), new Color(1f, 0.92f, 0.3f));

        GameObject winPanel = new GameObject("WinPanel", typeof(RectTransform), typeof(Image));
        winPanel.transform.SetParent(canvasGo.transform, false);
        RectTransform wr = winPanel.GetComponent<RectTransform>();
        wr.anchorMin = Vector2.zero;
        wr.anchorMax = Vector2.one;
        wr.offsetMin = Vector2.zero;
        wr.offsetMax = Vector2.zero;
        winPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        Text winTitle = MakeText("WinTitle", winPanel.transform, "LEVEL COMPLETE!", 110, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(1600f, 180f), new Color(1f, 0.8f, 0.1f));
        Button restartBtn = MakeButton("RestartButton", winPanel.transform, "RESTART", new Vector2(-260f, -60f), Color.white);
        Button nextBtn = MakeButton("NextLevelButton", winPanel.transform, "NEXT LEVEL", new Vector2(260f, -60f), new Color(0.6f, 1f, 0.6f));

        // ---------- event system (needed for the buttons) ----------
        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<StandaloneInputModule>();

        // ---------- level generator + game manager ----------
        new GameObject("LevelGenerator").AddComponent<LevelGenerator>();

        GameObject gmGo = new GameObject("GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        gm.coinText = coinText;
        gm.levelText = levelText;
        gm.timeText = timeText;
        gm.hintText = hintText;
        gm.winTitle = winTitle;
        gm.winPanel = winPanel;
        gm.restartButton = restartBtn;
        gm.nextButton = nextBtn;

        // ---------- save scene + add to Build Settings ----------
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SessionState.SetBool("BallGameOpened", true);
        Debug.Log("Ball Rolling Adventure scene built. Press Play to run it.");
    }

    static Button MakeButton(string name, Transform parent, string label, Vector2 pos, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(480f, 120f);
        Image img = go.GetComponent<Image>();
        img.color = color;
        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        MakeText("Label", go.transform, label, 52, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 120f), Color.black);
        return btn;
    }

    static Text MakeText(string name, Transform parent, string content, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        Text t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static Material BallMaterial()
    {
        Directory.CreateDirectory("Assets/Materials");
        const string path = "Assets/Materials/Ball.mat";
        AssetDatabase.DeleteAsset(path);
        Shader sh = Shader.Find("Standard");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        Material m = new Material(sh);
        m.color = new Color(0.2f, 0.65f, 1f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
