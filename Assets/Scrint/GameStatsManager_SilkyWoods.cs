using UnityEngine;

public class GameStatsManager_SilkyWoods : Singleton<GameStatsManager_SilkyWoods>
{
    [Header("Base Stats")]
    public float basePlayerSpeed = 8f;
    public float baseNodeRechargeDuration = 3f;
    public float baseEnemySpeed = 3f;
    public int basePlayerMaxHealth = 5;

    [Header("Current Stats")]
    private float currentPlayerSpeedBonus = 0f;
    private float currentNodeRechargeReduction = 0f;
    private float currentEnemySpeedReduction = 0f;

    // Getter cho các stats hiện tại
    public float GetPlayerSpeed()
    {
        return basePlayerSpeed + currentPlayerSpeedBonus;
    }

    public float GetNodeRechargeDuration()
    {
        return Mathf.Max(0.5f, baseNodeRechargeDuration - currentNodeRechargeReduction);
    }

    public float GetEnemySpeed()
    {
        return Mathf.Max(1f, baseEnemySpeed - currentEnemySpeedReduction);
    }

    public int GetPlayerMaxHealth()
    {
        return basePlayerMaxHealth; // Max health không đổi
    }

    // Apply card effects
    public void ApplyCardEffect(CardData card)
    {
        switch (card.type)
        {
            case CardData.CardType.IncreasePlayerSpeed:
                currentPlayerSpeedBonus += card.speedIncrease;
                Debug.Log($"Player speed increased! New speed: {GetPlayerSpeed()}");
                ApplyPlayerSpeedToPlayer();
                break;

            case CardData.CardType.DecreaseNodeRecharge:
                currentNodeRechargeReduction += card.nodeRechargeDecrease;
                Debug.Log($"Node recharge decreased! New duration: {GetNodeRechargeDuration()}");
                ApplyNodeRechargeToPowerNodes();
                break;

            case CardData.CardType.DecreaseEnemySpeed:
                currentEnemySpeedReduction += card.enemySpeedDecrease;
                Debug.Log($"Enemy speed decreased! New speed: {GetEnemySpeed()}");
                ApplyEnemySpeedToEnemies();
                break;

            case CardData.CardType.HealPlayer:
                Debug.Log($"Healing player by {card.healAmount}!");
                HealPlayer(card.healAmount);
                break;
        }
    }

    // Reset tất cả stats về ban đầu (khi restart game)
    public void ResetAllStats()
    {
        currentPlayerSpeedBonus = 0f;
        currentNodeRechargeReduction = 0f;
        currentEnemySpeedReduction = 0f;

        Debug.Log("All stats reset to base values!");

        // Apply base stats
        ApplyPlayerSpeedToPlayer();
    }

    // Apply stats to game objects
    private void ApplyPlayerSpeedToPlayer()
    {
        PlayerMovement3D_SilkyWoods player = FindFirstObjectByType<PlayerMovement3D_SilkyWoods>();
        if (player != null)
        {
            player.moveSpeed = GetPlayerSpeed();
        }
    }

    private void ApplyNodeRechargeToPowerNodes()
    {
        PowerNode3D_SilkyWoods[] nodes = FindObjectsByType<PowerNode3D_SilkyWoods>(FindObjectsSortMode.None);
        foreach (PowerNode3D_SilkyWoods node in nodes)
        {
            node.rechargeDuration = GetNodeRechargeDuration();
        }
    }

    private void ApplyEnemySpeedToEnemies()
    {
        EnemyBase_SilkyWoods[] enemies = FindObjectsByType<EnemyBase_SilkyWoods>(FindObjectsSortMode.None);
        foreach (EnemyBase_SilkyWoods enemy in enemies)
        {
            enemy.moveSpeed = GetEnemySpeed();
        }
    }

    private void HealPlayer(int amount)
    {
        PlayerHealth_SilkyWoods playerHealth = FindFirstObjectByType<PlayerHealth_SilkyWoods>();
        if (playerHealth != null)
        {
            playerHealth.Heal(amount);
            Debug.Log($"Player healed! Current HP: {playerHealth.currentHealth}/{playerHealth.maxHealth}");
        }
    }

    // Gọi khi bắt đầu game mới
    public void InitializeGame()
    {
        ResetAllStats();
    }
}