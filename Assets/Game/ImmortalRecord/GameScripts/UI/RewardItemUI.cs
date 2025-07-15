// �ļ���: RewardItemUI.cs
// ���ض���: RewardItem_Prefab Ԥ�Ƽ��ĸ�����

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    // --- ��Ԥ�Ƽ���Inspector��������ЩUIԪ�� ---
    [Header("UI�������")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;

    /// <summary>
    /// ʹ�ý������������ò���ʼ�����UIԪ�ء�
    /// </summary>
    /// <param name="rewardData">����ͼ��������Ľ�������</param>
    public void Setup(RewardItemData rewardData)
    {
        // ��ȫ��飬��ֹ���������ʱ����
        if (rewardData == null)
        {
            // �������Ϊ�գ�����ѡ����������UI������ʾһ�����ա�״̬
            gameObject.SetActive(false);
            return;
        }

        // --- ����ͼ�� ---
        if (itemIcon != null)
        {
            // �����ͼ����Դ������ʾ��
            if (rewardData.itemIcon != null)
            {
                itemIcon.sprite = rewardData.itemIcon;
                itemIcon.enabled = true;
            }
            else
            {
                // ���û��ͼ����Դ��������ͼ��Image
                itemIcon.enabled = false;
            }
        }

        // --- ���������ı� ---
        if (quantityText != null)
        {
            // �����������1������ʾ����
            if (rewardData.quantity > 1)
            {
                quantityText.text = rewardData.quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                // ����������ڻ�С��1������ʾ�����ı�
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    // ����Ҳ�����ṩһ����ֱ�ӵ�Setup����
    public void Setup(Sprite icon, int quantity)
    {
        if (itemIcon != null)
        {
            if (icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.enabled = true;
            }
            else
            {
                itemIcon.enabled = false;
            }
        }

        if (quantityText != null)
        {
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }
}