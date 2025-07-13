using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // 在Unity编辑器中，把你的Panel拖到这里
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        // 确保游戏开始时，主菜单是显示的，设置菜单是隐藏的
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // 这个函数将会被 “设置” 按钮调用
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // 这个函数将会被 “返回” 按钮调用
    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // 这个函数将会被 “开始游戏” 按钮调用
    public void StartGame()
    {
        // 在这里写加载游戏场景的代码
        // 例如: UnityEngine.SceneManagement.SceneManager.LoadScene("GameSceneName");
        Debug.Log("开始游戏!");
    }

    // 这个函数将会被 “退出游戏” 按钮调用
    public void QuitGame()
    {
        Debug.Log("退出游戏!");
        Application.Quit();
    }
}
