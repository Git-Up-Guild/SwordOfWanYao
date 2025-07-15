// �ļ���: EntrySceneUI.cs
using GameScripts.IR.UI.Login;
using UnityEngine;
using UnityEngine.SceneManagement; // �������볡�����������ռ�
using UnityEngine.UI;
using XGame.UI.Framework;

public class EntrySceneUI : MonoBehaviour
{
    // ��Inspector�а���Ҫ����İ�ť�ϵ�����
    public Button startButton;

    // ��Inspector��������Ҫ��ת�ĳ�����
    public string sceneToLoad;

    void Start()
    {
        // Ϊ��ť��̬���ӵ���¼�����
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadNextScene);
        }
    }

    public void LoadNextScene()
    {
        // ��鳡�����Ƿ�Ϊ��
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Ҫ���صĳ�����Ϊ�գ�����Inspector�����á�");
            return;
        }

        Debug.Log($"���ڼ��س���: {sceneToLoad}");


        UIWindowManager.Instance.CloseWindow<UILogin>();


        // ʹ��SceneManager����ָ���ĳ���
        SceneManager.LoadScene(sceneToLoad);

    }
}