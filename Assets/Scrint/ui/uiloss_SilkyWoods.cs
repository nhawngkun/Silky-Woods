using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UILoss_SilkyWoods : UICanvas_SilkyWoods
{
    [Header("UI Elements")]
    public Button homeButton;
    public Button retryButton;
    public TextMeshProUGUI finalWaveText;

    protected override void Awake()
    {
        base.Awake();

        // Setup buttons
        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClicked);
    }

    public override void Open()
    {
        base.Open();

        // Pause game
        Time.timeScale = 0f;

        // Hiện wave đã đạt được
        WaveSpawnerSystem waveSpawner = FindFirstObjectByType<WaveSpawnerSystem>();
        if (waveSpawner != null && finalWaveText != null)
        {
            finalWaveText.text = $"Reached Wave {waveSpawner.GetCurrentWaveIndex() + 1}";
        }

        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(2); // Sound thua
    }

    void OnHomeButtonClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log("Home button clicked from Loss screen - Resetting and returning to home...");

        // ✅ Resume time trước khi reset
        Time.timeScale = 1f;

        // ✅ RESET TOÀN BỘ GAME
        ResetCompleteGame();

        if (UIManager_SilkyWoods.Instance != null)
        {
            // ✅ Đóng tất cả UI
            UIManager_SilkyWoods.Instance.EnableUpdate(false);
            UIManager_SilkyWoods.Instance.EnableLoss(false);
            UIManager_SilkyWoods.Instance.EnableGameplay(false);

            // ✅ Về Home
            UIManager_SilkyWoods.Instance.EnableHome(true);
        }
    }

    void OnRetryButtonClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log("Retry button clicked - Restarting game from wave 1...");

        // ✅ Resume time
        Time.timeScale = 1f;

        // ✅ RESET TOÀN BỘ GAME
        ResetCompleteGame();

        // ✅ Đóng UI Loss và Update
        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableUpdate(false);
            UIManager_SilkyWoods.Instance.EnableLoss(false);
        }

        // ✅ START COROUTINE
        StartCoroutine(RestartGameSequence());
    }

    // ✅ COROUTINE - MỞ GAMEPLAY VÀ SUBSCRIBE TRƯỚC KHI START WAVE
    IEnumerator RestartGameSequence()
    {
        Debug.Log("[UILoss] Starting RestartGameSequence...");

        // Chờ 1 frame để UI đóng
        yield return null;

        // ✅ MỞ UI GAMEPLAY
        if (UIManager_SilkyWoods.Instance != null)
        {
            Debug.Log("[UILoss] Opening UIGameplay...");
            UIManager_SilkyWoods.Instance.EnableGameplay(true);
        }

        // ✅ Chờ 1 frame để UIGameplay.Open() chạy
        yield return null;

        // ✅ TÌM UIGameplay và GỌI SUBSCRIBE TRỰC TIẾP
        UIGameplay_SilkyWoods uiGameplay = UIManager_SilkyWoods.Instance.GetUI<UIGameplay_SilkyWoods>();
        if (uiGameplay != null)
        {
            Debug.Log("[UILoss] Forcing UIGameplay to subscribe...");
            // ✅ GỌI LẠI Open() để chắc chắn subscribe
            uiGameplay.Open();
        }

        // Chờ thêm 1 frame
        yield return null;

        // ✅ BÂY GIỜ START WAVE
        WaveSpawnerSystem waveSpawner = FindFirstObjectByType<WaveSpawnerSystem>();
        if (waveSpawner != null)
        {
            Debug.Log("[UILoss] Starting wave system...");
            waveSpawner.RestartWaves();
        }
        else
        {
            Debug.LogError("[UILoss] WaveSpawner not found!");
        }
    }

    void ResetCompleteGame()
    {
        Debug.Log("=== STARTING COMPLETE GAME RESET FROM LOSS SCREEN ===");

        // 1. Reset tất cả stats về base values
        if (GameStatsManager_SilkyWoods.Instance != null)
        {
            Debug.Log("Resetting game stats to initial values...");
            GameStatsManager_SilkyWoods.Instance.ResetAllStats();
        }

        // 2. Xóa tất cả enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"Destroying {enemies.Length} enemies...");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) Destroy(enemy);
        }

        // 3. Xóa tất cả electric lines
        ElectricLine3D_SilkyWoods[] lines = FindObjectsByType<ElectricLine3D_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {lines.Length} electric lines...");
        foreach (ElectricLine3D_SilkyWoods line in lines)
        {
            if (line != null && line.gameObject != null)
            {
                Destroy(line.gameObject);
            }
        }

        // 4. Xóa tất cả zones
        ElectricZone_SilkyWoods[] zones = FindObjectsByType<ElectricZone_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {zones.Length} electric zones...");
        foreach (ElectricZone_SilkyWoods zone in zones)
        {
            if (zone != null && zone.gameObject != null)
            {
                Destroy(zone.gameObject);
            }
        }

        // 5. Xóa tất cả PowerNodes
        PowerNode3D_SilkyWoods[] nodes = FindObjectsByType<PowerNode3D_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {nodes.Length} power nodes...");
        foreach (PowerNode3D_SilkyWoods node in nodes)
        {
            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }

        // 6. Reset CableManager
        CableManager3D_SilkyWoods cableManager = FindFirstObjectByType<CableManager3D_SilkyWoods>();
        if (cableManager != null)
        {
            Debug.Log("Resetting CableManager...");
            cableManager.enabled = false;
            cableManager.enabled = true;
        }

        // 7. Dừng WaveSpawner
        WaveSpawnerSystem waveSpawner = FindFirstObjectByType<WaveSpawnerSystem>();
        if (waveSpawner != null)
        {
            Debug.Log("Stopping wave spawner...");
            waveSpawner.StopAllCoroutines();
            waveSpawner.ResetWaveVariables();
        }

        // 8. Reset player health và position
        PlayerHealth_SilkyWoods playerHealth = FindFirstObjectByType<PlayerHealth_SilkyWoods>();
        if (playerHealth != null)
        {
            Debug.Log("Resetting player to initial state...");

            // Enable lại movement nếu bị disable
            PlayerMovement3D_SilkyWoods movement = playerHealth.GetComponent<PlayerMovement3D_SilkyWoods>();
            if (movement != null)
            {
                movement.enabled = true;
            }

            // Reset về vị trí spawn
            playerHealth.transform.position = Vector3.zero;

            // Reset Rigidbody velocity
            Rigidbody rb = playerHealth.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Reset health về trạng thái ban đầu
            if (GameStatsManager_SilkyWoods.Instance != null)
            {
                playerHealth.maxHealth = GameStatsManager_SilkyWoods.Instance.GetPlayerMaxHealth(); // 5
            }
            playerHealth.currentHealth = 3; // Bắt đầu với 3 máu
            playerHealth.UpdateHealthUI();
        }

        Debug.Log("=== COMPLETE GAME RESET FROM LOSS SCREEN FINISHED ===");
    }

    public override void CloseDirectly()
    {
        // Resume game
        Time.timeScale = 1f;

        base.CloseDirectly();
    }
}