using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image IconImage;
    public Text Name;
    public Text Description;
    public Text CountText;
    public Toggle SelectToggle;


    public void Initialize(CardConfig card, ToggleGroup toggleGroup, System.Action<CardConfig, bool> OnToggled)
    {
        IconImage.sprite = card.ICON;
        Name.text = card.Name;
        Description.text = card.Description;

        SelectToggle.group = toggleGroup;
        SelectToggle.onValueChanged.RemoveAllListeners();
        SelectToggle.onValueChanged.AddListener(isOn => { OnToggled?.Invoke(card, isOn); });
    }
    public void UpdateCount(string text)
    {
        CountText.text = text;
    }
}
