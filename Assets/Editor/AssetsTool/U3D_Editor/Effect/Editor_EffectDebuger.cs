using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XGame.Effect;

[CustomEditor(typeof(EffectDebuger))]
public class Editor_EffectDebuger : Editor {


     
    public override void OnInspectorGUI()
    {
        EffectDebuger effectDebuger = (EffectDebuger)target;
        if (GUILayout.Button("Get ParticleSystem List"))
        {
            InitParticleSystemsList(effectDebuger);
        }
        //DrawDefaultInspector(); 

        if ( effectDebuger.effects != null)
        {
            for (int i = 0; i < effectDebuger.effects.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                effectDebuger.effects[i].enable = EditorGUILayout.Toggle(effectDebuger.effects[i].enable,GUILayout.Width(30));
                EditorGUILayout.ObjectField(effectDebuger.effects[i].particleSystem,typeof(ParticleSystem),true);
                if (GUILayout.Button("Play")) {
                    PlayParticleSystem(effectDebuger.effects[i].particleSystem);
                } 
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.BeginHorizontal();       
        if (GUILayout.Button("Select All")) {
            SetEnableForAll(effectDebuger,true);
        } 

        if (GUILayout.Button("UnSelect All")) {
            SetEnableForAll(effectDebuger,false);
        } 
        EditorGUILayout.EndVertical();



        if (GUILayout.Button("Play Effect"))
        {
            PlayEffect(effectDebuger);
        }

        if (GUILayout.Button("Apply(Hidden Unenable)"))
        {
            ApplyChange(effectDebuger);
        }

        var warnStyle = new GUIStyle(GUI.skin.button);
        warnStyle.normal.textColor = Color.red;
        if (GUILayout.Button("Apply(Delete Unenable)", warnStyle))
        {
            ApplyChange(effectDebuger,true);
        }

        if (GUILayout.Button("Apply To Prefab", warnStyle))
        {
            ApplyToPrefab(effectDebuger);
        }

        EditorGUILayout.Separator();
        if (GUILayout.Button("Optimize Effect Light Setting"))
        {
            OptimizeEffectLightSetting(effectDebuger);
        }

        if (GUILayout.Button("Auto Replace Shaders With Mobile Version"))
        {
            AutoReplaceEffectShader(effectDebuger);
        }



        if (GUI.changed)
        {
            EditorUtility.SetDirty(effectDebuger);
        }
    }

    private void InitParticleSystemsList(EffectDebuger effectDebuger)
    {
        ParticleSystem[] pss = effectDebuger.GetComponentsInChildren<ParticleSystem>(true);
        effectDebuger.effects = new EffectDebuger.EffectData[pss.Length];

        for (int i = 0; i < pss.Length; i++)
        {
            effectDebuger.effects[i] = new EffectDebuger.EffectData();
            effectDebuger.effects[i].particleSystem = pss[i];
            effectDebuger.effects[i].enable = pss[i].gameObject.activeInHierarchy;
        }
    }


    private void PlayEffect(EffectDebuger effectDebuger)
    {
        foreach (var effect in effectDebuger.effects)
        {
            if (effect.enable) {
                effect.particleSystem.Play(false);
            }
        }
    }

    private void PlayParticleSystem(ParticleSystem ps)
    {
        ps.Play(false);  
    }

    private void OptimizeEffectLightSetting(EffectDebuger effectDebuger)
    {
        ParticleSystemRenderer psr;
        foreach (var effect in effectDebuger.effects)
        {
            psr = effect.particleSystem.GetComponent<ParticleSystemRenderer>();
            psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            psr.receiveShadows = false;
            psr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
        Debug.Log("OptimizeEffectLightSetting successed!");
    }

    private void AutoReplaceEffectShader(EffectDebuger effectDebuger)
    {
        ParticleSystemRenderer psr;
        string tmpShaderName;
        Shader tmpShader;
        foreach (var effect in effectDebuger.effects)
        {
            psr = effect.particleSystem.GetComponent<ParticleSystemRenderer>();
//            psr.sharedMaterial.shader = Shader.Find
            tmpShaderName = GetMobileVersionShaderName(psr.sharedMaterial.shader.name);
            tmpShader = Shader.Find(tmpShaderName);
            if (tmpShader != null) {
                if (tmpShader != psr.sharedMaterial.shader) {
                    Debug.Log("Replace shader from " + psr.sharedMaterial.shader.name + " to " + tmpShader.name, psr);
                    psr.sharedMaterial.shader = tmpShader;
                }
            } else {
                Debug.LogWarning("Can't find mobile version of the shader:" + psr.sharedMaterial.shader.name, psr); 
            }
        }
        Debug.Log("AutoReplaceEffectShader successed!");
        
    }

    private void SetEnableForAll(EffectDebuger effectDebuger,bool enable)
    {
        foreach (var effect in effectDebuger.effects)
        {
            effect.enable = enable;
        }
    }

    private void ApplyChange(EffectDebuger effectDebuger, bool del = false)
    {
        foreach (var effect in effectDebuger.effects)
        {
            if (del) {
                if (!effect.enable) {
                    //如果包含子节点，则只删除ParticleSystem组件，否则删除节点
                    if (effect.particleSystem.transform.childCount > 0) {
                        Debug.LogWarning("The particlesystem disabled contain some children,Just remove the particlesystem compoment.",effect.particleSystem.gameObject);
                        ParticleSystemRenderer psr = effect.particleSystem.GetComponent<ParticleSystemRenderer>();
                        if (psr != null) {
                            DestroyImmediate(psr);
                        }
                        DestroyImmediate(effect.particleSystem);
                    } else {
                        DestroyImmediate(effect.particleSystem.gameObject);
                    }
                }
            } else {
                if (!effect.enable && effect.particleSystem.transform.childCount > 0) {
                    Debug.LogWarning("The particlesystem disabled contain some children,please make sure all of the children are not enabled.",effect.particleSystem.gameObject);
                } 
                effect.particleSystem.gameObject.BetterSetActive(effect.enable);
            }
        }

        if (del)
        {
            InitParticleSystemsList(effectDebuger);
        }
        Debug.Log("Apply successed!");
    }

    private void ApplyToPrefab(EffectDebuger effectDebuger)
    {
        Object prefab = PrefabUtility.GetPrefabParent(effectDebuger.gameObject);
        if (prefab != null)
        {
            GameObject newPrefab = PrefabUtility.ReplacePrefab(effectDebuger.gameObject,prefab);
            EffectDebuger ed = newPrefab.GetComponent<EffectDebuger>();
            DestroyImmediate(ed,true);
        }
        Debug.Log("Apply to prefab successed!");
    }


    private string GetMobileVersionShaderName(string name)
    {
        string lastName = name.Substring(name.LastIndexOf("/") + 1);
        return "Mobile/Particles/" + lastName;
    }


}
