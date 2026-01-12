using UnityEngine;

[System.Serializable]
public class CardData
{
    public enum CardType
    {
        IncreasePlayerSpeed,    // 0: Tăng tốc độ player
        DecreaseNodeRecharge,   // 1: Giảm thời gian hồi PowerNode
        DecreaseEnemySpeed,     // 2: Giảm tốc độ enemy
        HealPlayer              // 3: Hồi máu player (không tăng max health)
    }

    public CardType type;
    public Sprite cardSprite;
    public string cardDescription;

    // Giá trị buff
    public float speedIncrease = 0.5f;
    public float nodeRechargeDecrease = 0.3f;
    public float enemySpeedDecrease = 0.3f;
    public int healAmount = 1;  // Đổi tên từ healthIncrease -> healAmount
}

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Game/Card Database")]
public class CardDatabase_SilkyWoods : ScriptableObject
{
    public CardData[] allCards = new CardData[4];
}