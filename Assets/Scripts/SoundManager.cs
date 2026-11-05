using UnityEngine;

// Makes every game sound in code, so no audio files are needed:
// coin chimes (louder and higher with each coin in a row), goal unlock, win fanfare,
// crash sound, last-10-seconds ticks, a rolling rumble, and a different background
// music tune for each level theme. Press M to mute / unmute the music.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    const int SR = 44100;

    AudioSource sfx, coinSrc, music, roll;
    AudioClip coinClip, unlockClip, winClip, hitClip, tickClip, rollClip;
    AudioClip[] musicClips = new AudioClip[6];
    int currentMusic = -1;
    float lastCoinTime = -10f;
    int combo;
    Rigidbody ball;

    public static SoundManager Ensure()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("SoundManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<SoundManager>();
        }
        return Instance;
    }

    void Awake()
    {
        sfx = NewSource(false, 1f);
        coinSrc = NewSource(false, 1f);
        music = NewSource(true, 0.28f);
        roll = NewSource(true, 0f);

        coinClip = BuildCoin();
        unlockClip = BuildUnlock();
        winClip = BuildWin();
        hitClip = BuildHit();
        tickClip = BuildTick();
        rollClip = BuildRoll();

        roll.clip = rollClip;
        roll.Play();
    }

    AudioSource NewSource(bool loop, float volume)
    {
        AudioSource a = gameObject.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.loop = loop;
        a.volume = volume;
        a.spatialBlend = 0f;
        return a;
    }

    void Update()
    {
        if (ball == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) ball = p.GetComponent<Rigidbody>();
        }

        float speed = 0f;
        bool grounded = false;
        if (ball != null)
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 v = ball.linearVelocity;
#else
            Vector3 v = ball.velocity;
#endif
            speed = new Vector3(v.x, 0f, v.z).magnitude;
            grounded = Mathf.Abs(v.y) < 0.6f && ball.position.y < 1.2f;
        }
        roll.volume = grounded ? Mathf.Clamp01(speed / 8f) * 0.5f : 0f;
        roll.pitch = 0.7f + speed * 0.05f;

        if (Input.GetKeyDown(KeyCode.M)) music.mute = !music.mute;
    }

    // ---------------- public sounds ----------------

    public void Coin()
    {
        if (Time.unscaledTime - lastCoinTime > 1.5f) combo = 0;
        lastCoinTime = Time.unscaledTime;
        coinSrc.pitch = 1f + 0.07f * Mathf.Min(combo, 8);   // rises with every coin in a row
        combo++;
        coinSrc.PlayOneShot(coinClip, 1f);
    }

    public void Unlock() { sfx.pitch = 1f; sfx.PlayOneShot(unlockClip, 1f); }

    public void Win()
    {
        music.volume = 0.12f;
        sfx.pitch = 1f;
        sfx.PlayOneShot(winClip, 1f);
    }

    public void Hit() { sfx.pitch = 1f; sfx.PlayOneShot(hitClip, 1f); }

    public void Tick() { sfx.pitch = 1f; sfx.PlayOneShot(tickClip, 0.7f); }

    public void PlayMusic(int themeIndex)
    {
        int index = Mathf.Abs(themeIndex) % 6;
        music.volume = 0.28f;
        if (currentMusic == index && music.isPlaying) return;
        currentMusic = index;
        if (musicClips[index] == null) musicClips[index] = BuildMusic(index);
        music.clip = musicClips[index];
        music.Play();
    }

    // ---------------- sound building ----------------

    AudioClip BuildCoin()
    {
        float[] b = new float[(int)(0.7f * SR)];
        AddNote(b, 0.00f, 987.8f, 0.14f, 0.55f, 0, 2f, false);    // B5
        AddNote(b, 0.07f, 1318.5f, 0.55f, 0.60f, 0, 4f, false);   // E6
        AddNote(b, 0.07f, 2637.0f, 0.45f, 0.20f, 0, 5f, false);   // sparkle
        AddNote(b, 0.14f, 1975.5f, 0.40f, 0.25f, 0, 5f, false);   // B6
        return Make("coin", b);
    }

    AudioClip BuildUnlock()
    {
        float[] b = new float[(int)(1.0f * SR)];
        float[] f = { 523.25f, 659.25f, 783.99f, 1046.5f };
        for (int i = 0; i < f.Length; i++)
            AddNote(b, i * 0.11f, f[i], 0.5f, 0.5f, 1, 3f, false);
        return Make("unlock", b);
    }

    AudioClip BuildWin()
    {
        float[] b = new float[(int)(2.2f * SR)];
        float[] f = { 523.25f, 523.25f, 523.25f, 659.25f, 783.99f, 659.25f, 783.99f, 1046.5f };
        float[] t = { 0f, 0.15f, 0.30f, 0.45f, 0.75f, 1.05f, 1.2f, 1.5f };
        float[] d = { 0.15f, 0.15f, 0.15f, 0.3f, 0.3f, 0.15f, 0.3f, 0.7f };
        for (int i = 0; i < f.Length; i++)
        {
            AddNote(b, t[i], f[i], d[i] + 0.1f, 0.5f, 1, 2f, false);
            AddNote(b, t[i], f[i] * 2f, d[i], 0.2f, 0, 3f, false);
        }
        return Make("win", b);
    }

    AudioClip BuildHit()
    {
        int n = (int)(0.55f * SR);
        float[] b = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float freq = Mathf.Lerp(260f, 55f, t / 0.55f);
            phase += freq / SR;
            float frac = phase - Mathf.Floor(phase);
            b[i] = (2f * frac - 1f) * Mathf.Exp(-3f * t / 0.55f);
        }
        return Make("hit", b);
    }

    AudioClip BuildTick()
    {
        float[] b = new float[(int)(0.12f * SR)];
        AddNote(b, 0f, 1800f, 0.08f, 0.6f, 2, 6f, false);
        return Make("tick", b);
    }

    AudioClip BuildRoll()
    {
        float[] b = new float[SR];
        System.Random r = new System.Random(5);
        float y = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float noise = (float)(r.NextDouble() * 2.0 - 1.0);
            y += 0.06f * (noise - y);
            b[i] = y;
        }
        return Make("roll", b);
    }

    // A different 32-step tune for each theme.
    AudioClip BuildMusic(int index)
    {
        float[][] scales =
        {
            new[] { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f, 587.33f, 659.25f },    // meadow: bright major
            new[] { 293.66f, 311.13f, 369.99f, 392.00f, 440.00f, 587.33f, 622.25f, 739.99f },    // desert: exotic
            new[] { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f, 1046.5f, 1174.7f, 1318.5f },    // ice: bells
            new[] { 220.00f, 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f, 587.33f },    // neon: minor, fast
            new[] { 164.81f, 196.00f, 220.00f, 246.94f, 293.66f, 329.63f, 392.00f, 440.00f },    // lava: dark
            new[] { 261.63f, 329.63f, 392.00f, 493.88f, 587.33f, 659.25f, 783.99f, 987.77f },    // space: dreamy
        };
        float[] bpm = { 120f, 100f, 90f, 128f, 110f, 70f };
        int[] melodyWave = { 1, 1, 0, 2, 3, 0 };
        int[] bassWave = { 0, 0, 0, 1, 1, 0 };
        float[] decay = { 3f, 3f, 5f, 4f, 3f, 1.5f };

        float[] scale = scales[index];
        float step = 60f / bpm[index] / 2f;
        int steps = 32;
        float[] b = new float[(int)(steps * step * SR)];
        System.Random r = new System.Random(index * 31 + 7);

        int pos = 3;
        for (int s = 0; s < steps; s++)
        {
            if (r.NextDouble() < 0.72)
            {
                pos = Mathf.Clamp(pos + r.Next(-2, 3), 0, scale.Length - 1);
                AddNote(b, s * step, scale[pos], step * 1.8f, 0.45f, melodyWave[index], decay[index], true);
            }
            if (s % 8 == 0)
            {
                float bass = scale[(s / 8 % 2 == 0) ? 0 : 2] / 2f;
                AddNote(b, s * step, bass, step * 7f, 0.5f, bassWave[index], 1.5f, true);
            }
        }
        return Make("music" + index, b);
    }

    // Adds one note to a sample buffer. wave: 0 sine, 1 triangle, 2 square, 3 saw.
    static void AddNote(float[] buf, float startSec, float freq, float dur, float amp, int wave, float decay, bool wrap)
    {
        int start = (int)(startSec * SR);
        int n = (int)(dur * SR);
        for (int i = 0; i < n; i++)
        {
            int idx = start + i;
            if (idx >= buf.Length)
            {
                if (!wrap) break;
                idx %= buf.Length;
            }
            float t = i / (float)SR;
            float ph = t * freq;
            float frac = ph - Mathf.Floor(ph);
            float s;
            if (wave == 0) s = Mathf.Sin(2f * Mathf.PI * ph);
            else if (wave == 1) s = 4f * Mathf.Abs(frac - 0.5f) - 1f;
            else if (wave == 2) s = frac < 0.5f ? 0.6f : -0.6f;
            else s = 2f * frac - 1f;
            float env = Mathf.Min(1f, t / 0.004f) * Mathf.Exp(-decay * t / dur) * Mathf.Min(1f, (dur - t) / 0.01f);
            buf[idx] += s * amp * env;
        }
    }

    // Normalises the samples and turns them into an AudioClip.
    static AudioClip Make(string name, float[] buf)
    {
        float peak = 0.0001f;
        for (int i = 0; i < buf.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(buf[i]));
        float gain = 0.95f / peak;
        for (int i = 0; i < buf.Length; i++) buf[i] *= gain;
        AudioClip clip = AudioClip.Create(name, buf.Length, 1, SR, false);
        clip.SetData(buf, 0);
        return clip;
    }
}
