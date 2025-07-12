using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Config")]
public class CardConfig : ScriptableObject
{
    public int ID;
    public Sprite ICON;
    public string Name;
    [TextArea] public string Description;
    public EffectBase Effect;
}