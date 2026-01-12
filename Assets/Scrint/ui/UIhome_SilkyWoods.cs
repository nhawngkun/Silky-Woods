using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHome_SilkyWoods : UICanvas_SilkyWoods
{
    [Header("UI Elements")]
    public Button playButton;
    public Button howToPlayButton;
    public Button settingsButton;

    protected override void Awake()
    {
        base.Awake();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (howToPlayButton != null)
            howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(seting);
    }

    void OnPlayButtonClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log("Play button clicked - Starting game...");

        // ✅ Đóng Home trước
        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableHome(false);
        }

        // ✅ START COROUTINE
        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        Debug.Log("[UIHome] Starting StartGameSequence...");
        
        // Chờ 1 frame
        yield return null;

        // ✅ MỞ UI GAMEPLAY
        if (UIManager_SilkyWoods.Instance != null)
        {
            Debug.Log("[UIHome] Opening UIGameplay...");
            UIManager_SilkyWoods.Instance.EnableGameplay(true);
        }

        // ✅ Chờ 1 frame để UIGameplay.Open() chạy
        yield return null;

        // ✅ TÌM UIGameplay và GỌI SUBSCRIBE TRỰC TIẾP
        UIGameplay_SilkyWoods uiGameplay = UIManager_SilkyWoods.Instance.GetUI<UIGameplay_SilkyWoods>();
        if (uiGameplay != null)
        {
            Debug.Log("[UIHome] Forcing UIGameplay to subscribe...");
            // ✅ GỌI LẠI Open() để chắc chắn subscribe
            uiGameplay.Open();
        }

        // Chờ thêm 1 frame
        yield return null;
        
        // ✅ BÂY GIỜ START WAVE
        WaveSpawnerSystem waveSpawner = FindFirstObjectByType<WaveSpawnerSystem>();
        if (waveSpawner != null)
        {
            Debug.Log("[UIHome] Starting wave system...");
            waveSpawner.StartNextWave();
        }
        else
        {
            Debug.LogError("[UIHome] WaveSpawner not found!");
        }
    }
    void seting()
    {
        UIManager_SilkyWoods.Instance.EnableHome(false);
        UIManager_SilkyWoods.Instance.EnableSettingPanel(true);
    }

    void OnHowToPlayClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableHome(false);
            UIManager_SilkyWoods.Instance.EnableHowToPlay(true);
        }
    }
}