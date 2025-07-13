using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectionManager : MonoBehaviour
{
    public static CardSelectionManager Instance;
    
    [Header("UI References")]
    public GameObject CardPanel;
    public CardUI[] CardUISlots;
    public Button confirmButton;
    public ToggleGroup toggleGroup;
    public GameObject CollectionPanel;
    public GameObject MenuButton;

    [Header("Card Pool")]
    public List<CardConfig> CardPool = new List<CardConfig>();

    private CardConfig selectedCard;
    private System.Action<CardConfig> onConfirmed;

    public Dictionary<CardConfig, int> cardInventory = new Dictionary<CardConfig, int>();

    public GameObject DeathPanel;

    private void Awake()
    {
        Instance = this;
        InitializeInventory();
    }


    void Start()
    {
        CardPanel.SetActive(false);
        MenuButton.SetActive(false);
        CollectionPanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        confirmButton.interactable = false;
    }
    private void Update()
    {

        
    }
    private void InitializeInventory()
    {
        foreach (var card in CardPool)
        {
            if (!cardInventory.ContainsKey(card))
            {
                cardInventory.Add(card, 0);
            }
        }
    }

    // 唯一保留的 ShowCards 方法
    public void ShowCards(System.Action<CardConfig> onConfirmed)
    {
        this.onConfirmed = onConfirmed;
        selectedCard = null;
        confirmButton.interactable = false;

        // 清除旧监听器
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        List<CardConfig> availableCards = GetAvailableCards();
        List<CardConfig> randomCards = GetRandomCards(3, availableCards);
        for (int i = 0; i < CardUISlots.Length; i++)
        {
            if (i < randomCards.Count)
            {
                CardUISlots[i].Initialize(
                    randomCards[i],
                    toggleGroup,
                    (card, isOn) => OnCardToggled(card, isOn)
                );
                CardUISlots[i].UpdateCount(GetCardCount(randomCards[i]) + "/" + randomCards[i].MaxOwnable);
            }
            else
            {
                CardUISlots[i].gameObject.SetActive(false);
            }
        }

        CardPanel.SetActive(true);
        Time.timeScale = 0f; 
    }
    
    private List<CardConfig> GetAvailableCards()
    {
        List<CardConfig> available = new List<CardConfig>();
        foreach (var card in CardPool)
        {
            if (cardInventory[card] < card.MaxOwnable)
            {
                available.Add(card);
            }
        }
        return available;
    }
    public int GetCardCount(CardConfig card)
    {
        return cardInventory.ContainsKey(card) ? cardInventory[card] : 0;
    }
    private void OnCardToggled(CardConfig card, bool isOn)
    {
        if (isOn)
        {
            selectedCard = card;
            confirmButton.interactable = true;
        }
    }
    private void OnConfirmClicked()
    {
        if (selectedCard != null)
        {
            if (cardInventory.ContainsKey(selectedCard))
            {
                cardInventory[selectedCard]++;
            }
            CardPanel.SetActive(false);
            Time.timeScale = 1f; // 游戏暂停逻辑
            toggleGroup.SetAllTogglesOff();
            onConfirmed?.Invoke(selectedCard);
        }
    }
    private List<CardConfig> GetRandomCards(int count, List<CardConfig> availableCards)
    {
        List<CardConfig> shuffled = new List<CardConfig>(availableCards);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }

        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
        List<CardConfig> result = new List<CardConfig>();
        HashSet<CardConfig> added = new HashSet<CardConfig>();
        foreach (var card in shuffled)
        {
            if (!added.Contains(card))
            {
                result.Add(card);
                added.Add(card);
                if (result.Count >= count) break;
            }
        }

        // 补全默认卡牌
        while (result.Count < count)
        {
            CardConfig defaultCard = ScriptableObject.CreateInstance<CardConfig>();
            defaultCard.Name = "默认卡牌";
            result.Add(defaultCard);
        }

        return result;
    }
    
}