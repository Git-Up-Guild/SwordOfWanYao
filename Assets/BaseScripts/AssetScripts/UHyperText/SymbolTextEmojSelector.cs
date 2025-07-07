/**************************************************************************    
文　　件：SymbolTextEmojSelector
作　　者：郑秀程
创建时间：2022/6/11 12:53:53
描　　述：表情选择面板
***************************************************************************/

using WXB;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace XGame.AssetScript.UHyperText
{
    /// <summary>
    /// 表情选择器
    /// </summary>
    [ExecuteInEditMode]
    public class SymbolTextEmojSelector : MonoBehaviour
    {
        public class OnSelectEvent : UnityEvent<string> { }

        //选择事件
        public OnSelectEvent onSelectEvent = new OnSelectEvent();

        //显示原型
        [SerializeField]
        private GameObject m_EmojItemProto;

        //输入框
        public InputField input;

        //输入框的光标位置
        private int m_InputCaretPosition = 0;

#if UNITY_EDITOR
        private int m_EmojItemProtoInstanceID = 0;
#endif

        private bool m_IsEmojItemCreated;

        private List<Cartoon> m_cartoons = new List<Cartoon>();

        private void OnDisable()
        {
            m_InputCaretPosition = 0;
        }

        private void OnEnable()
        {

        }

        private void Awake()
        {
            if(input != null)
            {
                input.onEndEdit.AddListener((t) => {
                    m_InputCaretPosition = input.caretPosition;
                    Debug.Log("CaretPosition is " + input.caretPosition);
                });
            }
        }

        private void Start()
        {
            CreateEmojItems();
        }

        public bool IsEmojExist(string name)
        {
            var cartoon = SymbolTextSettings.GetCartoon(name);
            return cartoon != null;
        }

        private void CreateEmojItems()
        {
            if (m_EmojItemProto == null || m_IsEmojItemCreated)
                return;

#if UNITY_EDITOR
            m_EmojItemProtoInstanceID = m_EmojItemProto.GetInstanceID();
#endif

            m_IsEmojItemCreated = true;
            var emojItemCom = m_EmojItemProto.GetComponent<SymbolTextEmojItem>();
            if (emojItemCom == null)
                m_EmojItemProto.AddComponent<SymbolTextEmojItem>();

            m_EmojItemProto.BetterSetActive(false);
            
            SymbolTextSettings.GetCartoons(m_cartoons);
            foreach (var c in m_cartoons)
            {
                var item = Instantiate(m_EmojItemProto, transform);
                item.hideFlags = HideFlags.DontSave;
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                item.BetterSetActive(true);
                var itemCom = item.GetComponent<SymbolTextEmojItem>();
                itemCom.SetEmojName(c.name, OnSelectEmoj);
            }
        }

        private void DestroyEmojItems()
        {
            m_IsEmojItemCreated = false;

            var itemComps = GetComponentsInChildren<SymbolTextEmojItem>();
            foreach (var i in itemComps)
            {
                if (i != m_EmojItemProto)
                {
                    DestroyImmediate(i, true);
                }
            }
        }

#if UNITY_EDITOR
        //private void OnValidate()
        //{
        //    if (Application.isPlaying)
        //        return;

        //    bool isProtoItemDirty = false;
        //    if (m_EmojItemProto == null)
        //    {
        //        isProtoItemDirty = true;
        //    }
        //    else
        //    {
        //        var instID = m_EmojItemProto.GetInstanceID();
        //        if (instID != m_EmojItemProtoInstanceID)
        //        {
        //            isProtoItemDirty = true;
        //        }
        //    }

        //    if(isProtoItemDirty)
        //        Invoke("OnEmojProtoItemDirty", 0.05f);
        //}

        //private void OnEmojProtoItemDirty()
        //{
        //    if (m_EmojItemProto == null)
        //    {
        //        DestroyEmojItems();
        //    }
        //    else
        //    {
        //        var instID = m_EmojItemProto.GetInstanceID();
        //        if (instID != m_EmojItemProtoInstanceID)
        //        {
        //            DestroyEmojItems();
        //            CreateEmojItems();
        //        }
        //    }
        //}

#endif

        private void OnSelectEmoj(string name)
        {
            onSelectEvent?.Invoke(name);

            if (input != null)
            {
                if (m_InputCaretPosition > input.text.Length)
                    m_InputCaretPosition = input.text.Length;

                //插入表情代码
                string prefixStr = input.text.Substring(0, m_InputCaretPosition);
                string suffixStr = input.text.Substring(m_InputCaretPosition);

                input.text = $"{prefixStr}[{name}]{suffixStr}";

                //移动光标的位置
                m_InputCaretPosition += name.Length + 2;

                //修正，防止越界
                if (m_InputCaretPosition > input.text.Length)
                    m_InputCaretPosition = input.text.Length;

                input.caretPosition = m_InputCaretPosition;
            }
        }
    }
}