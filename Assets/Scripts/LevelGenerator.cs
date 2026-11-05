using System.Collections.Generic;
using UnityEngine;

// Builds level number N when the game starts.
// Every level gets its own THEME (background, ground, walls, scenery, lighting, ball feel)
// and its own DIFFICULTY (narrower path, more obstacles, new obstacle types, time limit).
//   Level 1: walls only
//   Level 2: + moving red blocks and a time limit
//   Level 3: + spinning bars
//   Level 4: + pits with a narrow bridge
// After that every level adds more obstacles and a narrower platform.
public class LevelGenerator : MonoBehaviour
{
    public Renderer goalRenderer { get; private set; }
    public string ThemeName { get; private set; }
    public int ThemeIndex { get; private set; }
    public float TimeLimit { get; private set; }   // 0 = no time limit
    public string Tip { get; private set; }

    struct Theme
    {
        public string name;
        public Color sky, ground, wall, decor, light;
        public PrimitiveType shape;
        public float force, drag;
    }

    static Theme[] Themes()
    {
        return new[]
        {
            new Theme { name = "GREEN MEADOW", sky = new Color(0.53f, 0.81f, 0.98f), ground = new Color(0.25f, 0.65f, 0.30f),
                wall = new Color(0.55f, 0.50f, 0.45f), decor = new Color(0.12f, 0.5f, 0.2f), light = Color.white,
                shape = PrimitiveType.Sphere, force = 10f, drag = 1.2f },
            new Theme { name = "DESERT", sky = new Color(0.98f, 0.76f, 0.45f), ground = new Color(0.72f, 0.52f, 0.28f),
                wall = new Color(0.55f, 0.30f, 0.18f), decor = new Color(0.3f, 0.55f, 0.25f), light = new Color(1f, 0.92f, 0.75f),
                shape = PrimitiveType.Cylinder, force = 10f, drag = 1.2f },
            new Theme { name = "ICE WORLD", sky = new Color(0.72f, 0.9f, 1f), ground = new Color(0.78f, 0.93f, 1f),
                wall = new Color(0.35f, 0.6f, 0.9f), decor = new Color(1f, 1f, 1f), light = new Color(0.85f, 0.95f, 1f),
                shape = PrimitiveType.Sphere, force = 4f, drag = 0.35f },
            new Theme { name = "NEON NIGHT", sky = new Color(0.03f, 0.03f, 0.12f), ground = new Color(0.12f, 0.12f, 0.3f),
                wall = new Color(0f, 0.8f, 0.9f), decor = new Color(0.85f, 0.2f, 0.95f), light = new Color(0.55f, 0.6f, 1f),
                shape = PrimitiveType.Cube, force = 10f, drag = 1.2f },
            new Theme { name = "LAVA LAND", sky = new Color(0.28f, 0.05f, 0.03f), ground = new Color(0.22f, 0.16f, 0.15f),
                wall = new Color(0.6f, 0.22f, 0.1f), decor = new Color(1f, 0.4f, 0.05f), light = new Color(1f, 0.6f, 0.35f),
                shape = PrimitiveType.Cylinder, force = 10f, drag = 1.2f },
            new Theme { name = "OUTER SPACE", sky = new Color(0f, 0f, 0.04f), ground = new Color(0.32f, 0.32f, 0.45f),
                wall = new Color(0.65f, 0.5f, 0.95f), decor = new Color(0.5f, 0.3f, 0.85f), light = new Color(0.8f, 0.8f, 1f),
                shape = PrimitiveType.Sphere, force = 10f, drag = 1.2f },
        };
    }

    static Color Shift(Color c, int lap)
    {
        if (lap == 0) return c;
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);
        return Color.HSVToRGB((h + 0.12f * lap) % 1f, s, v);
    }

    // Returns how many coins this level contains.
    public int Generate(int level)
    {
        Theme[] themes = Themes();
        int themeIndex = (level - 1) % themes.Length;
        int lap = (level - 1) / themes.Length;
        Theme th = themes[themeIndex];
        Color sky = Shift(th.sky, lap);
        Color groundCol = Shift(th.ground, lap);
        Color wallCol = Shift(th.wall, lap);
        Color decorCol = Shift(th.decor, lap);
        ThemeName = th.name;
        ThemeIndex = themeIndex;

        // ---------- background, lighting, fog ----------
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = sky;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(sky, Color.white, 0.35f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = sky;
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 150f;
        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) l.color = th.light;

        // ---------- difficulty ----------
        System.Random rng = new System.Random(level * 7919);
        float width = Mathf.Max(10f - 0.5f * (level - 1), 5f);   // the path gets narrower
        float half = width / 2f;
        float len = Mathf.Min(100f + 25f * level, 400f);   // long levels: 125 units on level 1, up to 400
        float zStart = -len / 2f + 3f;
        float zGoal = len / 2f - 3f;
        int slots = Mathf.Min(6 + 2 * level, 40);
        int coinCount = Mathf.Min(12 + 3 * level, 80);
        float hazardSpeed = Mathf.Min(0.3f + 0.04f * level, 0.9f);
        float spinSpeed = Mathf.Min(60f + 8f * level, 200f) * ((level % 2 == 0) ? 1f : -1f);
        TimeLimit = level >= 2 ? Mathf.Round(len / 3f + 20f) : 0f;

        List<string> tips = new List<string>();
        if (level == 1) tips.Add("Collect all the coins, then roll onto the red pad!");
        if (level == 2) tips.Add("New: moving red blocks and a time limit!");
        if (level == 3) tips.Add("New: spinning bars!");
        if (level == 4) tips.Add("New: pits - cross the narrow bridge!");
        if (themeIndex == 2 && lap == 0) tips.Add("The ice is slippery!");
        Tip = string.Join(" ", tips);

        List<int> avail = new List<int> { 0 };      // 0 wall, 1 moving block, 2 spinner, 3 pit
        if (level >= 2) avail.Add(1);
        if (level >= 3) avail.Add(2);
        if (level >= 4) avail.Add(3);

        GameObject root = new GameObject("Level " + level + " - " + th.name);

        // ---------- player ----------
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 1f, zStart);
            BallController bc = player.GetComponent<BallController>();
            if (bc != null) bc.SetPhysics(th.force, th.drag);
        }

        // ---------- goal ----------
        GameObject goal = Cube("Goal", root, new Vector3(0f, 0.15f, zGoal), new Vector3(3f, 0.3f, 3f), new Color(0.9f, 0.2f, 0.2f));
        goal.GetComponent<Collider>().isTrigger = true;
        goal.AddComponent<GoalZone>();
        goalRenderer = goal.GetComponent<Renderer>();

        // ---------- obstacles ----------
        float spacing = (len - 14f) / slots;
        List<Vector2> walls = new List<Vector2>();   // x = z position, y = open side x
        List<Vector2> pits = new List<Vector2>();    // x = z position, y = bridge x
        int wallIndex = 0;
        for (int k = 0; k < slots; k++)
        {
            float z = zStart + 4f + (k + 0.5f) * spacing;
            int type = avail[(k + level) % avail.Count];

            if (type == 1)
            {
                GameObject hz = Cube("Moving Block", root, new Vector3(0f, 0.5f, z), Vector3.one, new Color(0.95f, 0.15f, 0.15f));
                hz.AddComponent<Rigidbody>().isKinematic = true;
                Hazard hs = hz.AddComponent<Hazard>();
                hs.moveOffset = new Vector3(width - 2f, 0f, 0f);
                hs.speed = hazardSpeed;
            }
            else if (type == 2)
            {
                GameObject bar = Cube("Spinning Bar", root, new Vector3(0f, 0.5f, z), new Vector3(width * 0.45f, 0.8f, 0.4f), new Color(1f, 0.6f, 0f));
                bar.AddComponent<Rigidbody>().isKinematic = true;
                SpinningBar sb = bar.AddComponent<SpinningBar>();
                sb.degreesPerSecond = spinSpeed;
            }
            else if (type == 3)
            {
                float bridgeX = ((float)rng.NextDouble() * 2f - 1f) * (half - 1.8f);
                pits.Add(new Vector2(z, bridgeX));
            }
            else
            {
                float side = (wallIndex % 2 == 0) ? -1f : 1f;
                Cube("Wall", root, new Vector3(side * half / 2f, 0.5f, z), new Vector3(half, 1f, 1f), wallCol);
                walls.Add(new Vector2(z, -side * half / 2f));
                wallIndex++;
            }
        }

        // ---------- ground (with pits) ----------
        float cursor = -len / 2f;
        foreach (Vector2 p in pits)
        {
            Segment(root, cursor, p.x - 1.5f, width, groundCol);
            Cube("Bridge", root, new Vector3(p.y, -0.5f, p.x), new Vector3(2.5f, 1f, 3f), new Color(groundCol.r * 0.85f, groundCol.g * 0.85f, groundCol.b * 0.85f));
            cursor = p.x + 1.5f;
        }
        Segment(root, cursor, len / 2f, width, groundCol);

        // ---------- scenery on both sides ----------
        int decorCount = Mathf.Min(Mathf.RoundToInt(len / 2.5f), 150);
        for (int i = 0; i < decorCount; i++)
        {
            float side = (i % 2 == 0) ? -1f : 1f;
            float z = -len / 2f + (float)rng.NextDouble() * len;
            float x = side * (half + 2f + (float)rng.NextDouble() * 12f);
            float s = 1f + (float)rng.NextDouble() * 3.5f;
            float y = (float)rng.NextDouble() * 3f - 0.5f;
            GameObject d = GameObject.CreatePrimitive(th.shape);
            d.name = "Scenery";
            d.transform.SetParent(root.transform);
            d.transform.position = new Vector3(x, y, z);
            d.transform.localScale = th.shape == PrimitiveType.Cylinder ? new Vector3(s, s * 1.5f, s) : new Vector3(s, s, s);
            float tint = 0.8f + (float)rng.NextDouble() * 0.4f;
            d.GetComponent<Renderer>().material.color = new Color(decorCol.r * tint, decorCol.g * tint, decorCol.b * tint);
            Destroy(d.GetComponent<Collider>());
        }

        // ---------- coins (kept out of walls and off pits) ----------
        Color coinCol = new Color(1f, 0.85f, 0.1f);
        for (int i = 0; i < coinCount; i++)
        {
            float z = Mathf.Lerp(zStart + 3f, zGoal - 3f, (i + 0.5f) / coinCount);
            float x = ((float)rng.NextDouble() * 2f - 1f) * (half - 1f);
            foreach (Vector2 wl in walls)
                if (Mathf.Abs(z - wl.x) < 1.3f) x = wl.y;
            foreach (Vector2 pt in pits)
                if (Mathf.Abs(z - pt.x) < 2.2f) x = pt.y;

            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            c.name = "Coin";
            c.transform.SetParent(root.transform);
            c.transform.position = new Vector3(x, 0.7f, z);
            c.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            c.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
            c.GetComponent<Renderer>().material.color = coinCol;
            Destroy(c.GetComponent<Collider>());
            SphereCollider sc = c.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1f;
            c.AddComponent<CoinPickup>();
        }

        return coinCount;
    }

    void Segment(GameObject root, float z0, float z1, float width, Color color)
    {
        if (z1 - z0 < 0.1f) return;
        Cube("LevelGround", root, new Vector3(0f, -0.5f, (z0 + z1) / 2f), new Vector3(width, 1f, z1 - z0), color);
    }

    GameObject Cube(string name, GameObject parent, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent.transform);
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().material.color = color;
        return g;
    }
}
