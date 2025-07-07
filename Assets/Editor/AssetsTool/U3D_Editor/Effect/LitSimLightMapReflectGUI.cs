///*******************************************************************
//** 文件名: LitSimLightMapReflectGUI.cs
//** 版  权:    (C) 深圳冰川网络技术有限公司 
//** 创建人:    彭朝政
//** 日  期:    2016/5/2
//** 版  本:    1.0
//** 描  述:    反射材质的编辑器脚本
//**            
//** 应  用:    辅助反射材质的矫正       
//**
//**************************** 修改记录 ******************************
//** 修改人:  
//** 日  期: 
//** 描  述: 
//********************************************************************/

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[System.Serializable]
//public class LitSimLightMapReflectGUI :MaterialEditor {
//    //new void Awake() {
//    //    base.Awake();
//    //}

//	//public override void  OnInspectorGUI(){
//	//	base.OnInspectorGUI ();
// //       EditorGUILayout.Separator();
// //       GUILayout.Label("Reflect BoundBox");
       
      
// //       EditorGUILayout.BeginVertical();
// //       mBBoxPosition =  EditorGUILayout.Vector3Field("BoundBoxPosition", mBBoxPosition);
// //       mBBoxScaleXYZ = EditorGUILayout.Vector3Field("BoundBoxScaleXYZ", mBBoxScaleXYZ);
// //       mEnivMapPos = EditorGUILayout.Vector3Field("EnvironmentPosition", mEnivMapPos);
// //       EditorGUILayout.EndVertical();

// //       Vector3 bMin = mBBoxPosition - mBBoxScaleXYZ / 2;
// //       Vector3 bMax = mBBoxPosition + mBBoxScaleXYZ / 2;

// //       targetMat.SetVector("_BBoxMin", bMin);
// //       targetMat.SetVector("_BBoxMax", bMax);
// //       targetMat.SetVector("_EnviCubeMapPos", mEnivMapPos);

// //       EditorGUI.BeginChangeCheck();

// //       if (EditorGUI.EndChangeCheck()) {
// //           serializedObject.ApplyModifiedProperties();
// //           EditorUtility.SetDirty(targetMat);
// //       }
// //   }


//    //public void OnSceneGUI(){
//    //    Material targetMat = target as Material;
//    //    Vector3 bPos = targetMat.GetVector("_BoundingBoxPos");
//    //    Vector3 bScale = targetMat.GetVector("_BoundingBoxScale");
//    //    Vector3 envCubePos = targetMat.GetVector("_EnviCubeMapPos");
//    //    Handles.color = Color.blue;
//    //    Handles.DrawWireCube(bPos, bScale);

//    //    Handles.color = Color.red;
//    //    Handles.SphereHandleCap(0, envCubePos, Quaternion.identity, 1, EventType.DragUpdated);
//    //}
   
//}
