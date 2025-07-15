// �ļ���: SpawnCardUI.cs
// ���ض���: SpawnCard_Prefab Ԥ�Ƽ��ĸ�����

using UnityEngine;
using UnityEngine.UI;
using TMPro;      // �����ʹ�� TextMeshPro
using System;      // Ϊ��ʹ�� Action �¼�

public class SpawnCardUI : MonoBehaviour
{
    // --- ��Ԥ�Ƽ���Inspector��������ЩUIԪ�� ---
    [Header("UI�������")]
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private Image cooldownOverlay;

    // --- ���������߼���ص�˽�б��� ---
    private float spawnInterval; // ������ֵ�������ʱ�� (���� 10��)
    private float cooldownTimer; // ��ǰ����ȴ����ʱ
    private bool isInitialized = false; // ����Ƿ��ѳ�ʼ������ֹ��Update�й���ִ��

    /// <summary>
    /// ����ȴ����ʱ����ʱ�������¼���
    /// �����ű�������BattleUIManager�����ԡ����ġ�����¼����Ա�����ȴ����ʱ�յ�֪ͨ��
    /// </summary>
    public Action OnCooldownComplete;

    void Update()
    {
        // ȷ���Ѿ���ʼ������ִ�е���ʱ�߼�
        if (!isInitialized)
        {
            return;
        }

        // �����ʱ�����ڵ���ʱ...
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime; // ÿ֡���ټ�ʱ��

            // ����UI����ת��ȴ���ֵ������
            if (cooldownOverlay != null && spawnInterval > 0)
            {
                cooldownOverlay.fillAmount = cooldownTimer / spawnInterval;
            }
        }
        else // �� cooldownTimer <= 0 ʱ����ζ����ȴ����
        {
            // 1. �����¼���֪ͨ�ⲿ�ű�����BattleUIManager������������λ�ˣ���
            // '?' �ǿ�ֵ��飬ȷ���ж�����������¼��ŵ��ã���ֹ����
            OnCooldownComplete?.Invoke();

            // 2. ���ü�ʱ�������̿�ʼ��һ�ֵ���ȴ����ʱ
            cooldownTimer = spawnInterval;
        }
    }

    /// <summary>
    /// ��ʼ�����ſ�Ƭ���� BattleUIManager ����Ϸ��ʼʱ���á�
    /// </summary>
    /// <param name="icon">Ҫ��ʾ�ı���ͼ��</param>
    /// <param name="interval">�ñ��ֵ������������λ���룩</param>
    public void Initialize(Sprite icon, float interval)
    {
        // ���ñ���ͼ��
        if (unitIcon != null)
        {
            unitIcon.sprite = icon;
        }

        // ������������
        this.spawnInterval = interval;
        this.cooldownTimer = interval; // ����������ȴ״̬��ʼ����ʱ

        // ȷ����ȴ���ֿɼ�������Ϊ�����
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // ���Ϊ�ѳ�ʼ����Update�е��߼����Կ�ʼִ����
        isInitialized = true;
    }

    /// <summary>
    // �����ڳ���λ��������ʾ���� BattleUIManager ����Ҫʱ���á�
    /// </summary>
    /// <param name="currentCount">��ǰս���ϸñ��ֵ�����</param>
    public void UpdatePopulation(int currentCount)
    {
        if (populationText != null)
        {
            populationText.text = currentCount.ToString();
        }
    }

    // �����Ҫ��ʾ "��ǰ����/�������" �ĸ�ʽ������ʹ��������ذ汾

    //public void UpdatePopulation(int currentCount, int maxCount)
    //{
    //    if (populationText != null)
    //    {
    //        populationText.text = $"{currentCount}/{maxCount}";
    //    }
    //}

}