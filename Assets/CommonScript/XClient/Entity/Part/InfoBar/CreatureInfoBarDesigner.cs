using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Attr;
using XGame.MonoState;

namespace XClient.Entity.Part
{
    public class CreatureInfoBarDesigner : MonoBehaviour
    {
        [Label("血条")]
        public Image hpBar;

        [Label("血量值")]
        public TextMeshProUGUI tmpHpValue;

        [Label("名称")]
        public TextMeshProUGUI tmpName;
    }
}
