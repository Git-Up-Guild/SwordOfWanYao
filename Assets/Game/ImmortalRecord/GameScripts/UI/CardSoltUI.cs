using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class CardSlotUI : MonoBehaviour
{
    [Header("UI组件")]
    public Image iconImage;
    public Text nameText;
    public Text descriptionText;
    public Text countText;
    public Button selectButton;
    public GameObject selectedIndicator;
    public GameObject maxTag;
    
    private CardConfig cardConfig;
    private System.Action<CardConfig> onClickCallback;
    
    public void Initialize(CardConfig card, int currentCount, System.Action<CardConfig> callback)
    {
        cardConfig = card;
        onClickCallback = callback;
        
        iconImage.sprite = card.ICON;
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        countText.text = $"{currentCount}/{card.MaxOwnable}";
        
        maxTag.SetActive(currentCount >= card.MaxOwnable);
        selectButton.interactable = currentCount < card.MaxOwnable;
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClickCallback?.Invoke(card));
    }
    
    public void SetSelected(bool selected)
    {
        selectedIndicator.SetActive(selected);
    }
}