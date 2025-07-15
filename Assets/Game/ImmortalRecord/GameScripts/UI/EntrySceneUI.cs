// 文件名: EntrySceneUI.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间
using UnityEngine.UI;

public class EntrySceneUI : MonoBehaviour
{
    // 在Inspector中把你要点击的按钮拖到这里
    public Button startButton;

    // 在Inspector中输入你要跳转的场景名
    public string sceneToLoad;

    void Start()
    {
        // 为按钮动态添加点击事件监听
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadNextScene);
        }
    }

    public void LoadNextScene()
    {
        // 检查场景名是否为空
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("要加载的场景名为空！请在Inspector中设置。");
            return;
        }

        Debug.Log($"正在加载场景: {sceneToLoad}");

        // 使用SceneManager加载指定的场景
        SceneManager.LoadScene(sceneToLoad);
    }
}