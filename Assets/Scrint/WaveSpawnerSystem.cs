using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int spawnCount = 1;
    public float spawnDelay = 0f;
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave 1";
    public List<Transform> enemySpawnPoints = new List<Transform>();
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    public List<GameObject> powerNodePrefabs = new List<GameObject>();
    public List<Transform> powerNodeSpawnPoints = new List<Transform>();
}

public class WaveSpawnerSystem : MonoBehaviour
{
    [Header("Wave Configuration")]
    public List<Wave> waves = new List<Wave>();

    [Header("VFX")]
    public GameObject spawnVFX;
    public float vfxDuration = 1f;

    [Header("Settings")]
    public float timeBetweenWaves = 5f;
    public bool autoStartFirstWave = false; // ✅ ĐẢM BẢO LÀ FALSE
    public float powerNodeSpawnDelay = 0.5f;
    public float powerNodeDespawnDelay = 0.5f;
    public float delayBeforeEnemySpawn = 2f;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool isSpawningWave = false;
    private bool isWaitingForNextWave = false;

    private List<GameObject> currentPowerNodes = new List<GameObject>();

    public delegate void WaveEvent(int waveIndex);
    public event WaveEvent OnWaveStart;
    public event WaveEvent OnWaveComplete;
    public event WaveEvent OnAllWavesComplete;

    void Start()
    {
        // ❌ KHÔNG TỰ ĐỘNG BẮT ĐẦU WAVE
        // Wave chỉ bắt đầu khi gọi StartNextWave() từ UIHome
        Debug.Log("WaveSpawner ready. Waiting for Play button...");
    }

    void Update()
    {
        if (isSpawningWave && !isWaitingForNextWave && enemiesAlive <= 0)
        {
            StartCoroutine(CompleteWave());
        }
    }

    IEnumerator StartWaveSequence(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("All waves completed!");
            OnAllWavesComplete?.Invoke(waveIndex);
            yield break;
        }

        currentWaveIndex = waveIndex;
        Wave wave = waves[waveIndex];

        Debug.Log($"=== STARTING {wave.waveName} ===");
        OnWaveStart?.Invoke(waveIndex);

        Debug.Log("Step 1: Spawning PowerNodes sequentially...");
        yield return StartCoroutine(SpawnPowerNodesSequential(wave));

        Debug.Log($"Step 2: Waiting {delayBeforeEnemySpawn}s before spawning enemies...");
        yield return new WaitForSeconds(delayBeforeEnemySpawn);

        Debug.Log("Step 3: Spawning enemies...");
        StartWave(waveIndex);
    }

    public void StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("All waves completed!");
            OnAllWavesComplete?.Invoke(waveIndex);
            return;
        }

        currentWaveIndex = waveIndex;
        Wave wave = waves[waveIndex];

        isSpawningWave = true;
        isWaitingForNextWave = false;

        enemiesAlive = 0;
        foreach (EnemySpawnData enemyData in wave.enemyTypes)
        {
            enemiesAlive += enemyData.spawnCount;
        }

        Debug.Log($"Wave {waveIndex + 1}: Total enemies to spawn = {enemiesAlive}");

        foreach (EnemySpawnData enemyData in wave.enemyTypes)
        {
            StartCoroutine(SpawnEnemyType(wave, enemyData));
        }
    }

    IEnumerator SpawnEnemyType(Wave wave, EnemySpawnData enemyData)
    {
        if (wave.enemySpawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available for wave!");
            yield break;
        }

        for (int i = 0; i < enemyData.spawnCount; i++)
        {
            Transform randomSpawnPoint = wave.enemySpawnPoints[Random.Range(0, wave.enemySpawnPoints.Count)];
            StartCoroutine(SpawnEnemyWithVFX(enemyData.enemyPrefab, randomSpawnPoint.position));

            if (i < enemyData.spawnCount - 1 && enemyData.spawnDelay > 0)
            {
                yield return new WaitForSeconds(enemyData.spawnDelay);
            }
        }
    }

    IEnumerator SpawnEnemyWithVFX(GameObject enemyPrefab, Vector3 spawnPos)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab is null!");
            enemiesAlive--;
            yield break;
        }

        GameObject vfx = null;
        if (spawnVFX != null)
        {
            vfx = Instantiate(spawnVFX, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(vfxDuration);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyBase_SilkyWoods enemyBase = enemy.GetComponent<EnemyBase_SilkyWoods>();
        if (enemyBase != null)
        {
            StartCoroutine(WaitForEnemyDeath(enemy));
        }

        if (vfx != null)
        {
            Destroy(vfx);
        }
    }

    IEnumerator WaitForEnemyDeath(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }

        enemiesAlive--;

    }

    IEnumerator CompleteWave()
    {
        isWaitingForNextWave = true;

        Wave completedWave = waves[currentWaveIndex];


        int subscriberCount = OnWaveComplete != null ? OnWaveComplete.GetInvocationList().Length : 0;


        bool isLastWave = (currentWaveIndex >= waves.Count - 1);

        if (isLastWave)
        {

            OnAllWavesComplete?.Invoke(currentWaveIndex);
            yield break;
        }


        OnWaveComplete?.Invoke(currentWaveIndex);

        yield return new WaitForSeconds(0.3f);

        UIUpdate_SilkyWoods uiUpdate = null;
        if (UIManager_SilkyWoods.Instance != null)
        {
            uiUpdate = UIManager_SilkyWoods.Instance.GetUI<UIUpdate_SilkyWoods>();
            if (uiUpdate != null)
            {
                CanvasGroup cg = uiUpdate.GetComponent<CanvasGroup>();
                bool uiOpened = (cg != null && cg.alpha > 0.5f);

                if (!uiOpened)
                {
                    Debug.LogWarning("UI Update did not open! Check UIGameplay_Full.OnWaveCompleted()");
                }
            }
            else
            {
                Debug.LogError("UIUpdate_Full not found in UIManager!");
            }
        }

        float waitTime = 0f;
        float maxWaitTime = 60f;

        while (waitTime < maxWaitTime)
        {
            if (UIManager_SilkyWoods.Instance != null && uiUpdate != null)
            {
                CanvasGroup cg = uiUpdate.GetComponent<CanvasGroup>();
                bool stillOpen = (cg != null && cg.alpha > 0.5f);

                if (!stillOpen && waitTime > 1f)
                {

                    break;
                }
            }
            else
            {
                break;
            }

            waitTime += Time.deltaTime;
            yield return null;
        }

        if (waitTime >= maxWaitTime)
        {
            Debug.LogWarning("UI Update timeout! Force continuing...");
        }



        ElectricLine3D_SilkyWoods[] allLines = FindObjectsByType<ElectricLine3D_SilkyWoods>(FindObjectsSortMode.None);
        int linesDestroyed = 0;
        foreach (ElectricLine3D_SilkyWoods line in allLines)
        {
            if (line != null && line.gameObject != null)
            {
                bool isInZone = line.GetComponentInParent<ElectricZone_SilkyWoods>() != null;

                if (!isInZone)
                {
                    Destroy(line.gameObject);
                    linesDestroyed++;
                }
            }
        }
       ;

        yield return null;

        CableManager3D_SilkyWoods cableManager = FindFirstObjectByType<CableManager3D_SilkyWoods>();
        if (cableManager != null)
        {
            cableManager.enabled = false;
            yield return null;
            cableManager.enabled = true;
            Debug.Log("CableManager reset");
        }


        yield return StartCoroutine(DespawnPowerNodesSequential());

        yield return new WaitForSeconds(timeBetweenWaves);

        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            isSpawningWave = false;
            yield return StartCoroutine(StartWaveSequence(currentWaveIndex));
        }
        else
        {
            Debug.Log("=== ALL WAVES COMPLETED! ===");
            OnAllWavesComplete?.Invoke(currentWaveIndex);
            SoundManager_SilkyWoods.Instance.PlayVFXSound(0);
        }
    }

    IEnumerator SpawnPowerNodesSequential(Wave wave)
    {
        if (wave.powerNodePrefabs.Count == 0 || wave.powerNodeSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No PowerNode prefabs or spawn points!");
            yield break;
        }

        for (int i = 0; i < wave.powerNodeSpawnPoints.Count; i++)
        {
            if (wave.powerNodeSpawnPoints[i] == null) continue;

            GameObject randomPrefab = wave.powerNodePrefabs[Random.Range(0, wave.powerNodePrefabs.Count)];
            if (randomPrefab == null) continue;

            Vector3 spawnPos = wave.powerNodeSpawnPoints[i].position;
            Quaternion spawnRot = wave.powerNodeSpawnPoints[i].rotation;

            GameObject node = Instantiate(randomPrefab, spawnPos, spawnRot);
            node.transform.localScale = Vector3.zero;
            currentPowerNodes.Add(node);

            StartCoroutine(ScaleUpPowerNode(node));

            Debug.Log($"✓ Spawned PowerNode {i + 1}/{wave.powerNodeSpawnPoints.Count}");

            yield return new WaitForSeconds(powerNodeSpawnDelay);
        }

        Debug.Log($"=== All {currentPowerNodes.Count} PowerNodes spawned ===");
    }

    IEnumerator DespawnPowerNodesSequential()
    {
        List<GameObject> nodesToDespawn = new List<GameObject>(currentPowerNodes);

        foreach (GameObject node in nodesToDespawn)
        {
            if (node != null)
            {
                StartCoroutine(ScaleDownAndDestroyPowerNode(node));
                yield return new WaitForSeconds(powerNodeDespawnDelay);
            }
        }

        currentPowerNodes.Clear();
        Debug.Log("=== All PowerNodes despawned ===");
    }

    IEnumerator ScaleUpPowerNode(GameObject node)
    {
        if (node == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;

        while (elapsed < duration && node != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            node.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        if (node != null)
        {
            node.transform.localScale = targetScale;
        }
    }

    IEnumerator ScaleDownAndDestroyPowerNode(GameObject node)
    {
        if (node == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = node.transform.localScale;

        while (elapsed < duration && node != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Pow(t, 3f);

            node.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        if (node != null)
        {
            Destroy(node);
        }
    }

    public void StartNextWave()
    {
        if (!isSpawningWave)
        {
            StartCoroutine(StartWaveSequence(currentWaveIndex));
        }
    }

    public void RestartWaves()
    {
        Debug.Log("=== RESTARTING WAVES ===");

        StopAllCoroutines();

        // Xóa tất cả enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"Destroying {enemies.Length} enemies...");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) Destroy(enemy);
        }

        // Xóa tất cả lines
        ElectricLine3D_SilkyWoods[] lines = FindObjectsByType<ElectricLine3D_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {lines.Length} electric lines...");
        foreach (ElectricLine3D_SilkyWoods line in lines)
        {
            if (line != null && line.gameObject != null)
            {
                Destroy(line.gameObject);
            }
        }

        // Xóa tất cả zones
        ElectricZone_SilkyWoods[] zones = FindObjectsByType<ElectricZone_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {zones.Length} electric zones...");
        foreach (ElectricZone_SilkyWoods zone in zones)
        {
            if (zone != null && zone.gameObject != null)
            {
                Destroy(zone.gameObject);
            }
        }

        // Xóa tất cả PowerNodes
        List<GameObject> nodesToDestroy = new List<GameObject>(currentPowerNodes);
        Debug.Log($"Destroying {nodesToDestroy.Count} power nodes from list...");
        foreach (GameObject node in nodesToDestroy)
        {
            if (node != null) Destroy(node);
        }
        currentPowerNodes.Clear();

        PowerNode3D_SilkyWoods[] remainingNodes = FindObjectsByType<PowerNode3D_SilkyWoods>(FindObjectsSortMode.None);
        Debug.Log($"Destroying {remainingNodes.Length} remaining power nodes...");
        foreach (PowerNode3D_SilkyWoods node in remainingNodes)
        {
            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }

        // Reset CableManager
        CableManager3D_SilkyWoods cableManager = FindFirstObjectByType<CableManager3D_SilkyWoods>();
        if (cableManager != null)
        {
            cableManager.enabled = false;
            cableManager.enabled = true;
            Debug.Log("CableManager reset complete");
        }

        // Reset variables
        currentWaveIndex = 0;
        enemiesAlive = 0;
        isSpawningWave = false;
        isWaitingForNextWave = false;

        Debug.Log("Wave system reset complete. Starting wave 1...");

        // Bắt đầu lại từ wave 1
        if (waves.Count > 0)
        {
            StartCoroutine(StartWaveSequence(0));
        }
    }

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public int GetTotalWaves()
    {
        return waves.Count;
    }

    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }

    void OnDrawGizmos()
    {
        if (waves == null || waves.Count == 0) return;

        for (int w = 0; w < waves.Count; w++)
        {
            Wave wave = waves[w];

            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in wave.enemySpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position,
                                   spawnPoint.position + Vector3.up * 2f);
                }
            }

            Gizmos.color = Color.cyan;
            foreach (Transform nodePoint in wave.powerNodeSpawnPoints)
            {
                if (nodePoint != null)
                {
                    Gizmos.DrawWireCube(nodePoint.position, Vector3.one * 0.5f);
                }
            }
        }
    }
    // ✅ THÊM HÀM NÀY VÀO CUỐI FILE
    public void ResetWaveVariables()
    {
        Debug.Log("Resetting wave variables...");

        currentWaveIndex = 0;
        enemiesAlive = 0;
        isSpawningWave = false;
        isWaitingForNextWave = false;
        currentPowerNodes.Clear();
    }
}