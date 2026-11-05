using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int CurrentLevel = 1;   // survives scene reloads, resets when you stop Play

    public Text coinText;
    public Text levelText;
    public Text timeText;
    public Text hintText;
    public Text winTitle;
    public GameObject winPanel;
    public Button restartButton;
    public Button nextButton;

    int collected;
    int targetCoins;
    float hintTimer;
    float timeLeft;
    float timeLimit;
    int lastSecond = -1;
    bool finished;
    bool dying;
    LevelGenerator generator;

    public bool GoalUnlocked { get; private set; }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        SoundManager.Ensure();
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (nextButton != null) nextButton.onClick.AddListener(NextLevel);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (hintText != null) hintText.text = "";

        generator = FindFirstObjectByType<LevelGenerator>();
        targetCoins = generator != null ? generator.Generate(CurrentLevel) : 10;
        timeLimit = generator != null ? generator.TimeLimit : 0f;
        timeLeft = timeLimit;

        UpdateHUD();
        UpdateTime();
        if (generator != null)
        {
            SoundManager.Instance.PlayMusic(generator.ThemeIndex);
            if (!string.IsNullOrEmpty(generator.Tip)) ShowHint(generator.Tip, 4f);
        }
    }

    void Update()
    {
        if (hintTimer > 0f)
        {
            hintTimer -= Time.unscaledDeltaTime;
            if (hintTimer <= 0f && hintText != null) hintText.text = "";
        }

        if (timeLimit > 0f && !finished)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                finished = true;
                ShowHint("TIME'S UP!", 2f);
                SoundManager.Instance.Hit();
                Invoke(nameof(Restart), 1.5f);
            }
            UpdateTime();
        }
    }

    public void CollectCoin()
    {
        collected++;
        SoundManager.Instance.Coin();
        UpdateHUD();
        if (collected >= targetCoins && !GoalUnlocked)
        {
            GoalUnlocked = true;
            SoundManager.Instance.Unlock();
            if (generator != null && generator.goalRenderer != null)
                generator.goalRenderer.material.color = Color.green;
            ShowHint("Goal unlocked! Roll onto the green pad.", 2.5f);
        }
    }

    public void GoalTouched()
    {
        if (GoalUnlocked) WinGame();
        else ShowHint("Collect all " + targetCoins + " coins first!", 2.5f);
    }

    void ShowHint(string message, float seconds)
    {
        if (hintText == null) return;
        hintText.text = message;
        hintTimer = seconds;
    }

    void UpdateHUD()
    {
        if (coinText != null) coinText.text = "COINS: " + collected + " / " + targetCoins;
        if (levelText != null)
        {
            string themeName = generator != null ? generator.ThemeName : "";
            levelText.text = "LEVEL " + CurrentLevel + (themeName != "" ? " - " + themeName : "");
        }
    }

    void UpdateTime()
    {
        if (timeText == null) return;
        if (timeLimit <= 0f) { timeText.text = ""; return; }
        int sec = Mathf.CeilToInt(timeLeft);
        timeText.text = "TIME: " + sec;
        timeText.color = timeLeft < 10f ? new Color(1f, 0.3f, 0.3f) : Color.white;
        if (timeLeft < 10f && !finished && sec != lastSecond) SoundManager.Instance.Tick();
        lastSecond = sec;
    }

    public void WinGame()
    {
        if (finished) return;
        finished = true;
        SoundManager.Instance.Win();
        if (winTitle != null) winTitle.text = "LEVEL " + CurrentLevel + " COMPLETE!";
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Called when the ball touches a hazard or falls off: crash sound, then restart the level.
    public void Die()
    {
        if (dying || finished) return;
        dying = true;
        SoundManager.Instance.Hit();
        Reload();
    }

    public void NextLevel()
    {
        CurrentLevel++;
        Reload();
    }

    public void Restart()
    {
        Reload();
    }

    void Reload()
    {
        CancelInvoke();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }
}
