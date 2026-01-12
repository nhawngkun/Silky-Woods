using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIUpdate_SilkyWoods : UICanvas_SilkyWoods
{
    [Header("Card UI Elements")]
    public CardUI[] cardUISlots = new CardUI[3];

    [Header("Card Database")]
    public CardDatabase_SilkyWoods cardDatabase;

    [System.Serializable]
    public class CardUI
    {
        public GameObject cardObject;
        public Image cardImage;
        public TextMeshProUGUI cardDescriptionText;
        public Button cardButton;
        [HideInInspector] public CardData assignedCard;
    }

    // ★ DÙNG OnEnable để setup lại buttons mỗi khi mở
    private void OnEnable()
    {
        Debug.Log("UIUpdate_Full OnEnable - Generating cards...");

        // Generate 3 random cards
        GenerateRandomCards();
    }

    private void OnDisable()
    {
        Debug.Log("UIUpdate_Full OnDisable");

        // Clear tất cả listeners khi đóng
        foreach (var cardUI in cardUISlots)
        {
            if (cardUI.cardButton != null)
            {
                cardUI.cardButton.onClick.RemoveAllListeners();
            }
        }
    }

    public override void Open()
    {
        base.Open();
    }

    void GenerateRandomCards()
    {
        if (cardDatabase == null || cardDatabase.allCards.Length == 0)
        {
            Debug.LogError("Card Database is null or empty!");
            return;
        }

        List<CardData> availableCards = new List<CardData>(cardDatabase.allCards);

        for (int i = 0; i < cardUISlots.Length && availableCards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            CardData selectedCard = availableCards[randomIndex];

            SetupCardUI(cardUISlots[i], selectedCard);

            availableCards.RemoveAt(randomIndex);
        }

        Debug.Log($"Generated {cardUISlots.Length} random cards");
    }

    void SetupCardUI(CardUI cardUI, CardData cardData)
    {
        if (cardUI == null || cardData == null) return;

        cardUI.assignedCard = cardData;

        if (cardUI.cardImage != null && cardData.cardSprite != null)
        {
            cardUI.cardImage.sprite = cardData.cardSprite;
        }

        if (cardUI.cardDescriptionText != null)
        {
            cardUI.cardDescriptionText.text = GetCardDescription(cardData);
        }

        // ★ QUAN TRỌNG: RemoveAllListeners trước khi add mới
        if (cardUI.cardButton != null)
        {
            cardUI.cardButton.onClick.RemoveAllListeners();
            cardUI.cardButton.onClick.AddListener(() => OnCardSelected(cardData));
            Debug.Log($"Setup button for card: {cardData.type}");
        }

        if (cardUI.cardObject != null)
        {
            cardUI.cardObject.SetActive(true);
        }
    }

    string GetCardDescription(CardData card)
    {
        switch (card.type)
        {
            case CardData.CardType.IncreasePlayerSpeed:
                return $"+{card.speedIncrease} Speed player";

            case CardData.CardType.DecreaseNodeRecharge:
                return $"-{card.nodeRechargeDecrease}s Apple tree";

            case CardData.CardType.DecreaseEnemySpeed:
                return $"-{card.enemySpeedDecrease} Enemy speed";

            case CardData.CardType.HealPlayer:
                return $"+{card.healAmount} Health";

            default:
                return "Unknown Card";
        }
    }

    void OnCardSelected(CardData selectedCard)
    {
        if (SoundManager_SilkyWoods.Instance != null)
            SoundManager_SilkyWoods.Instance.PlayVFXSound(1);

        Debug.Log($"Card selected: {selectedCard.type}");

        // Apply card effect
        if (GameStatsManager_SilkyWoods.Instance != null)
        {
            GameStatsManager_SilkyWoods.Instance.ApplyCardEffect(selectedCard);
        }

        // Resume game
        Debug.Log("Resuming game (Time.timeScale = 1)...");
        Time.timeScale = 1f;

        // ★ Close UI Update bằng CloseDirectly (không dùng EnableUpdate)
        Debug.Log("Closing UI Update...");
        if (UIManager_SilkyWoods.Instance != null)
        {
            UIManager_SilkyWoods.Instance.EnableUpdate( false);
        }

        Debug.Log("UI Update closed. Wave system will continue automatically.");
    }

    public override void CloseDirectly()
    {
        Debug.Log("UIUpdate_Full CloseDirectly called - Resuming game...");

        // Resume game nếu đang pause
        Time.timeScale = 1f;

        // Clear buttons
        foreach (var cardUI in cardUISlots)
        {
            if (cardUI.cardButton != null)
            {
                cardUI.cardButton.onClick.RemoveAllListeners();
            }
        }

        base.CloseDirectly();
    }
}