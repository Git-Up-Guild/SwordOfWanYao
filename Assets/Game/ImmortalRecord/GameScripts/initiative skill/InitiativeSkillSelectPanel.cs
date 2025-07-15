using UnityEngine;
using UnityEngine.EventSystems;

public class InitiativeSkillSelectPanel : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!InitiativeSkillManager.Instance.IsSelecting()) return;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        InitiativeSkillManager.Instance.OnSelectPoint(worldPos);
    }
}
