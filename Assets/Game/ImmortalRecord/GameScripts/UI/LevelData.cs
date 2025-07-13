// �ļ���: LevelData.cs
using UnityEngine;

// [CreateAssetMenu] ���������ǿ��Դ� Assets/Create �˵�����������͵���Դ�ļ�
[CreateAssetMenu(fileName = "NewLevelData", menuName = "MyGame/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("�ؿ�������Ϣ")]
    public int levelNumber;         // �ؿ���ţ����磺1, 2, 3
    public string levelName;        // �ؿ����ƣ����磺���������
    public Sprite levelImage;       // ��ѡ�������ʾ�Ĺؿ���ͼ

    [Header("ս��������Ϣ")]
    public string sceneNameToLoad;  // �����ս��������Ҫ���صĳ����ļ���
                                    // ��Ҫ��������ֱ�������� Build Settings ����ӵĳ���������ȫһ�£�
}