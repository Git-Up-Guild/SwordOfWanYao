using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // ��Unity�༭���У������Panel�ϵ�����
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        // ȷ����Ϸ��ʼʱ�����˵�����ʾ�ģ����ò˵������ص�
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // ����������ᱻ �����á� ��ť����
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // ����������ᱻ �����ء� ��ť����
    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // ����������ᱻ ����ʼ��Ϸ�� ��ť����
    public void StartGame()
    {
        // ������д������Ϸ�����Ĵ���
        // ����: UnityEngine.SceneManagement.SceneManager.LoadScene("GameSceneName");
        Debug.Log("��ʼ��Ϸ!");
    }

    // ����������ᱻ ���˳���Ϸ�� ��ť����
    public void QuitGame()
    {
        Debug.Log("�˳���Ϸ!");
        Application.Quit();
    }
}
