using UnityEngine;

namespace SwordOfWanYao
{
    /// <summary>
    /// 卡牌效果基类
    /// </summary>
    public abstract class EffectBase : ScriptableObject
    {
        [Header("效果基础信息")]
        public string effectName;
        public string description;
        public Sprite icon;

        /// <summary>
        /// 应用效果到指定的士兵控制器
        /// </summary>
        /// <param name="soldierController">目标士兵控制器</param>
        public abstract void ApplyEffect(SoldierController soldierController);

        /// <summary>
        /// 移除效果（如果需要）
        /// </summary>
        /// <param name="soldierController">目标士兵控制器</param>
        public virtual void RemoveEffect(SoldierController soldierController)
        {
            // 默认实现为空，子类可以重写
        }
    }
} 