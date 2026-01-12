using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGameplay_SilkyWoods : UICanvas_SilkyWoods
{
    [Header("UI Elements")]
    public TextMeshProUGUI waveText;
    public Button homeButton;
    public Button resetButton;

    [Header("Joystick")]
    public GameObject joystickObject;

    private WaveSpawnerSystem waveSpawner;
    private bool hasSubscribed = false; // ✅ THÊM FLAG để tránh subscribe nhiều lần

    protected override void Awake()
    {
        base.Awake();

        Debug.Log("[UIGameplay] Awake called");

        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    // ✅ SUBSCRIBE TRỰC TIẾP TRONG OPEN() thay vì OnEnable()
    public override void Open()
    {
        base.Open();
        Debug.Log("[UIGameplay] Open() called - Setting up WaveSpawner events");
        
        SubscribeToWaveEvents();
    }

    // ✅ HÀM SUBSCRIBE RIÊNG - Gọi từ nhiều nơi
    void SubscribeToWaveEvents()
    {
        Debug.Log("[UIGameplay] SubscribeToWaveEvents called");

        // ✅ TÌM LẠI WaveSpawner
        waveSpawner = FindFirstObjectByType<WaveSpawnerSystem>();

        if (waveSpawner != null)
        {
            Debug.Log("[UIGameplay] WaveSpawner found!");

            // ✅ LUÔN Unsubscribe trước để tránh duplicate
            waveSpawner.OnWaveStart -= UpdateWaveText;
            waveSpawner.OnWaveComplete -= OnWaveCompleted;

            // ✅ Subscribe lại
            waveSpawner.OnWaveStart += UpdateWaveText;
            waveSpawner.OnWaveComplete += OnWaveCompleted;

            hasSubscribed = true;
            Debug.Log("[UIGameplay] Successfully subscribed to WaveSpawner events");

            // ✅ CẬP NHẬT TEXT NGAY LẬP TỨC
            int currentWave = waveSpawner.GetCurrentWaveIndex();
            UpdateWaveText(currentWave);
            Debug.Log($"[UIGameplay] Current wave index: {currentWave}");
        }
        else
        {
            Debug.LogError("[UIGameplay] WaveSpawnerSystem not found!");
            hasSubscribed = false;
        }
    }

    void UpdateWaveText(int waveIndex)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {waveIndex + 1}";
            Debug.Log($"[UIGameplay] Updated wave text to: Wave {waveIndex + 1}");
        }
    }

    void OnWaveCompleted(int waveIndex)
    {
        Debug.Log($"[UIGameplay] OnWaveCompleted called for wave {waveIndex + 1}");

        if (UIManager_SilkyWoods.Instance != null)
        {
            Debug.Log("[UIGameplay] Opening UI Update with EnableUpdate(true)...");
            UIManager_SilkyWoods.Instance.EnableUpdate(true);
        }
        else
        {
            Debug.LogError("[UIGameplay] UIManager not found!");
        }
    }

    void OnHomeButtonClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log("Home button clicked - Resetting game and returning to home...");

        // ✅ UNSUBSCRIBE trước khi reset
        UnsubscribeFromWaveEvents();

        // ✅ RESET TOÀN BỘ GAME
        ResetCompleteGame();

        // Đóng UI Update nếu đang mở
        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableUpdate(false);
            UIManager_SilkyWoods.Instance.EnableGameplay(false);
            UIManager_SilkyWoods.Instance.EnableHome(true);
        }
    }

    void OnResetButtonClicked()
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log("Reset button clicked - Restarting game from wave 1...");

        // ✅ UNSUBSCRIBE trước khi reset
        UnsubscribeFromWaveEvents();

        // ✅ RESET TOÀN BỘ GAME
        ResetCompleteGame();

        // Đóng UI Update nếu đang mở
        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableUpdate(false);
        }

        // ✅ SUBSCRIBE LẠI NGAY
        SubscribeToWaveEvents();

        // ✅ START WAVE
        if (waveSpawner != null)
        {
            Debug.Log("Restarting waves from wave 1...");
            waveSpawner.RestartWaves();
        }
    }

    // ✅ HÀM UNSUBSCRIBE RIÊNG
    void UnsubscribeFromWaveEvents()
    {
        if (waveSpawner != null && hasSubscribed)
        {
            Debug.Log("[UIGameplay] Unsubscribing from WaveSpawner events");
            waveSpawner.OnWaveStart -= UpdateWaveText;
            waveSpawner.OnWaveComplete -= OnWaveCompleted;
            hasSubscribed = false;
        }
    }

    void ResetCompleteGame()
    {
        Debug.Log("=== STARTING COMPLETE GAME RESET ===");

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

        Debug.Log("=== COMPLETE GAME RESET FINISHED ===");
    }

    public override void CloseDirectly()
    {
        // ✅ UNSUBSCRIBE khi đóng
        UnsubscribeFromWaveEvents();

        base.CloseDirectly();
    }

    // ✅ THÊM OnDestroy để đảm bảo cleanup
    void OnDestroy()
    {
        UnsubscribeFromWaveEvents();
    }
}