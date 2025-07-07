using UnityEngine;
using System.Collections;

/// <summary>
/// 性能统计信息
/// </summary>
public class PrefamceInfo : MonoBehaviour {

#if !UNITY_EDITOR
    bool bShow = false;
    float deltaTime = 0.0f;
    float fixedDelatTime = 0.0f;
    GUIStyle style = new GUIStyle();
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F11))
        {
            bShow = !bShow;
        }
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void FixedUpdate()
    {
        fixedDelatTime += (Time.fixedDeltaTime - fixedDelatTime) * 0.1f;
    }
    void OnGUI()
    {
        if (!bShow)
        {
            return;
        }
        int w = Screen.width, h = Screen.height;

        Rect rect = new Rect(10, 10, w, h * 2 * 0.01f);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 16;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        float fixedmsec = fixedDelatTime * 1000.0f;
        float FixedFps = 1.0f / fixedDelatTime;
        string text = string.Format("主线程:{0:0.0} ms ({1:0.} fps)\n物理线程:{2:0.0} ms" +
            "({3:0.} fps)\n系统:{4}\n显存:{5:0.}MB\n内存:{6:.0}MB\n" +
            "显卡:{7}\nCPU:{8}", msec, fps, fixedmsec, FixedFps,
            SystemInfo.operatingSystem, SystemInfo.graphicsMemorySize, SystemInfo.systemMemorySize, SystemInfo.graphicsDeviceName, SystemInfo.processorType);

        GUI.Label(rect, text, style);
    }
#endif


}
