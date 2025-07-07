// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 16:03

using DG.Tweening;
using UnityEditor;
#if true // UI_MARKER
#endif
#if false // TEXTMESHPRO_MARKER
    using TMPro;
#endif

namespace DG.DOTweenEditor
{
    [CustomEditor(typeof(DOTweenAnimationEx))]
    public class DOTweenAnimationExInspector : DOTweenAnimationInspector
    {
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override bool HasVisualManager()
        {
            return (target as DOTweenAnimation).GetComponent<DOTweenVisualManagerEx>() != null;
        }

        protected override void AddVisualManager()
        {
            (target as DOTweenAnimation).gameObject.AddComponent<DOTweenVisualManagerEx>();
        }
    }
}
