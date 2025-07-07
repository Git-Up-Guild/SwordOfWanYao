using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using XGame.Effect;
using XGame.Anim.Tween;

[CustomEditor(typeof(Effect_TweenSequence))]
public class Editor_TweenSequence :  Editor {


    public string[] tweenTypeNames = new string[]
    {
        "Common/Tween Position","Common/Tween PositionX","Common/Tween PositionY","Common/Tween PositionZ","Common/Tween Rotation","Common/Tween RotationX","Common/Tween RotationY","Common/Tween RotationZ","Common/Tween Scale","Common/Tween Size",
        "Render/Tween Alpha","Render/Tween Color","Render/Tween UV","Render/Tween Material Property",
        "UI/Tween AnchorPosition","UI/Tween AnchorPositionX","UI/Tween AnchorPositionY","UI/Tween Image Alpha","UI/Tween ImageColor","UI/Tween RawImage UV","UI/Tween Text Alpha","UI/Tween TextColor",
    };
    public Type[] tweenTypes = new Type[]
    {
        typeof(TweenPosition),typeof(TweenPositionX),typeof(TweenPositionY),typeof(TweenPositionZ),typeof(TweenRotation),typeof(TweenRotationX),typeof(TweenRotationY),typeof(TweenRotationZ),typeof(TweenScale),typeof(TweenSize),
        typeof(TweenAlpha),typeof(TweenColor),typeof(TweenUV),typeof(TweenMaterialPropertyFloat),
        typeof(TweenAnchorPosition), typeof(TweenAnchorPositionX), typeof(TweenAnchorPositionY), typeof(TweenImageAlpha), typeof(TweenImageColor), typeof(TweenRawImageUV), typeof(TweenTextAlpha), typeof(TweenTextColor),
    };

    private int m_SelectedTweenTypeIndex = 0;
    private const float DefaultIntervalValue = 0.5f;

    private List<int> m_NeedRemoveNodeIndexes = new List<int>();
    public Dictionary<int,List<int>> m_NeedRemoveTweenIndexes = new Dictionary<int, List<int>>();

    public override void OnInspectorGUI ()
    {
        Effect_TweenSequence effSeq = target as Effect_TweenSequence;
        TweenSequence seq = effSeq.tweenSequence; 

        seq.playWhenStart = EditorGUILayout.Toggle("Play When Start",seq.playWhenStart);
        seq.loop = EditorGUILayout.Toggle("Loop",seq.loop);
        
        var seqProperty = serializedObject.FindProperty("tweenSequence");
        //回调事件View绘制 
        EditorGUI.BeginChangeCheck();
        SerializedProperty onFinishedSP = seqProperty.FindPropertyRelative("onFinished");
        EditorGUILayout.PropertyField(onFinishedSP);
        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
        }

        for (int i = 0; i < seq.nodes.Count; i++) {
            if (seq.nodes[i].delay > 0) {
                DrawIntervalView(ref seq.nodes[i].delay);
                EditorGUILayout.Separator();
            }

            EditorGUILayout.BeginVertical();
            if (seq.nodes[i].tweens.Count > 0) {
                DrawNodeItemUI(seq, seq.nodes[i],i);

            } else {
                AddNodeIndexToRemoveList(i);               
            }           

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        if (seq.lastDelay > 0) {
            DrawIntervalView(ref seq.lastDelay);
        }

        EditorGUILayout.BeginHorizontal("Box");
        m_SelectedTweenTypeIndex = EditorGUILayout.Popup(m_SelectedTweenTypeIndex,tweenTypeNames);

        if (GUILayout.Button("Append Tween")) {
            AppendTween(seq,tweenTypes[m_SelectedTweenTypeIndex]);
        }
        if (GUILayout.Button("Append Interval")) {
            AppendInterval(seq, DefaultIntervalValue);
        }

        if (GUILayout.Button("Join Tween")) {
            JoinTween(seq,tweenTypes[m_SelectedTweenTypeIndex]);
        }
        EditorGUILayout.EndHorizontal();

        if (m_NeedRemoveTweenIndexes.Count > 0) {
            EditorApplication.delayCall += RemoveTweenFromNode;
        }

        if (m_NeedRemoveNodeIndexes.Count > 0) {
            EditorApplication.delayCall += RemoveSeqNode;
        }
    }

    private void DrawIntervalView(ref float interval)
    {
        EditorGUILayout.BeginHorizontal("Box");
        interval = EditorGUILayout.FloatField("Interval",interval);
        if (GUILayout.Button("Del",GUILayout.Width(50))) {                  
            interval = 0;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawNodeItemUI(TweenSequence seq,TweenSequence.SequeueNode node, int nodeIndex)
    {
        EditorGUILayout.BeginVertical("Box");
        for (int i = 0; i < node.tweens.Count; i++) {
            DrawTweenItemUI(node,node.tweens[i],nodeIndex,i);          
        }

        //操作区
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Del")) {  
            for (int i = 0; i < node.tweens.Count; i++) {
                AddTweenIndexToRemoveList(nodeIndex,i);
            }
            AddNodeIndexToRemoveList(nodeIndex);  
            EditorGUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Append Tween")) {  
            AppendTweenAfterNodeIndex(seq,tweenTypes[m_SelectedTweenTypeIndex],nodeIndex);
        }

        if (GUILayout.Button("Append Interval")) {  
            AppendIntervalAfterNodeIndex(seq,DefaultIntervalValue,nodeIndex);
        }


        if (GUILayout.Button("Join Tween")) { 
            JoinTweenAtNodeIndex(seq,tweenTypes[m_SelectedTweenTypeIndex],nodeIndex);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();
//        EditorGUILayout.PropertyField(sp);
//        EditorGUILayout.PropertyField(
    }

    private void AppendTween(TweenSequence seq,Type tweenType)
    {
        TweenCore tween = CreateTween(seq, tweenType);

        if (tween != null) {
            seq.Append(tween);
        }
    }

    private void AppendInterval(TweenSequence seq,float interval)
    {
        seq.AppendInterval(interval);
    }

    private void JoinTween(TweenSequence seq,Type tweenType)
    {
        TweenCore tween = CreateTween(seq, tweenType);
        if (tween != null) {
            seq.Join(tween);
        }
    }

    private void AppendTweenAfterNodeIndex(TweenSequence seq,Type tweenType,int nodeIndex)
    {
        TweenCore tween = CreateTween(seq, tweenType);
        if (tween != null) {
            seq.AppendAfterNodeIndex(tween,nodeIndex);
        }
    }

    private void AppendIntervalAfterNodeIndex(TweenSequence seq,float interval, int nodeIndex)
    {
        seq.AppenIntervalAfterNodeIndex(interval,nodeIndex);
    }

    private void JoinTweenAtNodeIndex(TweenSequence seq,Type tweenType, int nodeIndex)
    {
        TweenCore tween = CreateTween(seq, tweenType);
        if (tween != null) {
            seq.JoinAtNodeIndex(tween,nodeIndex);
        }
    }

    private void AddNodeIndexToRemoveList(int index)
    {
        if (!m_NeedRemoveNodeIndexes.Contains(index)) {
            m_NeedRemoveNodeIndexes.Add(index);
        }   
    }

    private void AddTweenIndexToRemoveList(int nodeIndex,int tweenindex)
    {
        List<int> tweenIndexes;
        if (!m_NeedRemoveTweenIndexes.TryGetValue(nodeIndex,out tweenIndexes)) {
            tweenIndexes = new List<int>();
            m_NeedRemoveTweenIndexes.Add(nodeIndex,tweenIndexes);
        }
        if (!tweenIndexes.Contains(tweenindex)) {
            tweenIndexes.Add(tweenindex);
        }
    }

    private void RemoveSeqNode()
    {
        Effect_TweenSequence effSeq = target as Effect_TweenSequence;
        TweenSequence seq = effSeq.tweenSequence;
        for (int i = m_NeedRemoveNodeIndexes.Count-1; i >= 0; i--) {
            seq.nodes.RemoveAt(m_NeedRemoveNodeIndexes[i]);
        }
        m_NeedRemoveNodeIndexes.Clear();

    }

    private void RemoveTweenFromNode()
    {
        Effect_TweenSequence effSeq = target as Effect_TweenSequence;
        TweenSequence seq = effSeq.tweenSequence;
        TweenCore tmpTween;
        foreach (var item in m_NeedRemoveTweenIndexes) {
            for (int i = item.Value.Count-1; i >=0; i--) {
                tmpTween = seq.nodes[item.Key].tweens[i];
                seq.nodes[item.Key].tweens.RemoveAt(i);
                DestroyImmediate(tmpTween);
            }
        }
        m_NeedRemoveTweenIndexes.Clear();
    }

    private void DrawTweenItemUI(TweenSequence.SequeueNode node,TweenCore tween, int nodeIndex, int tweenIndex)
    {      
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.BeginVertical();
        EditorGUI.indentLevel++;       
        tween.needShowDetail = EditorGUILayout.Foldout(tween.needShowDetail,tween.GetType().ToString());
        if (tween.needShowDetail) { 
            EditorGUI.BeginChangeCheck();
            SerializedObject so = new SerializedObject(tween);
            SerializedProperty sp =so.GetIterator();
           //忽略第前两个属性（Script Reference,OnFinshed Event)
            sp.NextVisible(true); //获取第一个属性时EnterChildren必须设置为True,
            sp.NextVisible(false);
            while (sp.NextVisible(false)) {   
                EditorGUILayout.PropertyField(sp);
            }
            if (EditorGUI.EndChangeCheck()) {
                so.ApplyModifiedProperties();
                node.UpdateDuration();
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Del",GUILayout.Width(50))) {                  
            AddTweenIndexToRemoveList(nodeIndex,tweenIndex);
        }
        EditorGUILayout.EndHorizontal();
    }

    private TweenCore CreateTween(TweenSequence seq, Type tweenType)
    {
        Effect_TweenSequence effSeq = target as Effect_TweenSequence;
        TweenCore tween = effSeq.gameObject.AddComponent(tweenType) as TweenCore;
        tween.hideFlags = HideFlags.HideInInspector;
        //默认参数设置
        tween.isFromCurrentValue = true;
        tween.isStartRelative = true;
        tween.isEndRelative = true;
        tween.revertMode = RevertMode.Play;
        return tween;
    }
}
