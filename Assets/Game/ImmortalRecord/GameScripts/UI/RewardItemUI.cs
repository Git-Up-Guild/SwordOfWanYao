// 文件名: RewardItemUI.cs
// 挂载对象: RewardItem_Prefab 预制件的根物体

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    // --- 在预制件的Inspector中链接这些UI元素 ---
    [Header("UI组件引用")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;

    /// <summary>
    /// 使用奖励数据来设置并初始化这个UI元素。
    /// </summary>
    /// <param name="rewardData">包含图标和数量的奖励数据</param>
    public void Setup(RewardItemData rewardData)
    {
        // 安全检查，防止传入空数据时报错
        if (rewardData == null)
        {
            // 如果数据为空，可以选择隐藏整个UI或者显示一个“空”状态
            gameObject.SetActive(false);
            return;
        }

        // --- 更新图标 ---
        if (itemIcon != null)
        {
            // 如果有图标资源，就显示它
            if (rewardData.itemIcon != null)
            {
                itemIcon.sprite = rewardData.itemIcon;
                itemIcon.enabled = true;
            }
            else
            {
                // 如果没有图标资源，就隐藏图标Image
                itemIcon.enabled = false;
            }
        }

        // --- 更新数量文本 ---
        if (quantityText != null)
        {
            // 如果数量大于1，就显示数量
            if (rewardData.quantity > 1)
            {
                quantityText.text = rewardData.quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                // 如果数量等于或小于1，则不显示数量文本
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    // 我们也可以提供一个更直接的Setup方法
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