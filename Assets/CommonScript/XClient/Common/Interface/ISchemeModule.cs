/*******************************************************************
** 文件名:	ISchemeCenter.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	李涛
** 日  期:	2016/1/148
** 版  本:	1.0
** 描  述:	配置中心,服务器端差不多所有脚本都在这里载入或获取数据
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace XClient.Common
{
    // 物品配置基类
    // tolua_begin
    public class SGoodsSchemeInfo
    {
        public int lGoodsID;						// 物品ID,所有
        public int lGoodsGroupID;					// 物品GroupID
        public string szName; 						// 名字 32 => 48 for utf-8 version

        public int lGoodsClass;					    // 物品类（1：装备，2：药品）
        public int lGoodsSubClass;					// 物品子类（装备和药品各不相同）

        public int lAllowNation;					// 使用国籍
        public int[] lAllowVocation;				// 使用职业
        public int lAllowLevel;                     // 使用等级
        public int lAllowVipLevel;					// 使用至尊等级

        public int lPileQty;						// 叠放数量

        public int lBuyCost;						// 买入基础价格
        public int lSaleCost;						// 卖出基础价络

        public int lBaseLevel;						// 基础物品档次
        public int lPickupBcastFlag;				// 拾取后广播标志
        public int lHoldMaxQty;					    // 拥有最多个数
        public int lBindFlag;						// 绑定标志位	
        public int lUseTerm;						// 使用期限
        public int lDropFixedProbability;			// 死亡掉装固定概率

        public string lIconID;						// 物品栏图片

        public int lBoxResID;						// 盒子外观资源ID	
        public int lBoxRayResID;					// 盒子闪烁资源ID

        public int lIsTaskGoods;					// 是否任务物品（0：非任务物品，1：是）

        public string szDesc;				        // 描述  256 => 384 for utf-8 version
        public bool bForcePick;                     // 掉落时强制拾取
    };

    // Leechdom.csv
    // tolua_begin
    public class SLeechdomSchemeInfo : SGoodsSchemeInfo
    {
        public int lUseCount;						// 使用次数
        public int lOnID;							// 作用ID
        //public string szCue;						// 浮动提示
        public int lDiceFlag;						// 是否需要掷骰子(0：不需要,1：需要)
        public int lSyncExtendData;					// 是否同步扩展数据(0：不同步,1：同步)
        public int lDropSoundID;                    // 怪物掉装的音效ID

        // 以上是3端共用属性

        public int nIsPopGuide;                     // 弹出使用物品引导的ID(<=0 不使用) 
        public bool bIsUse;                         // 提示完成后是否默认使用
        public bool bValuable;                      // 珍贵物品
        public List<int> lTipsButton_Skep;          // 包裹Tips显示的按钮
        public List<int> lTipsButton_Dress;         // 穿戴Tips显示的按钮
        public List<int> lTipsButton_Show;          // 查看Tips显示的按钮
        public int nBornFlashLevel;                 // 出生光效播放等级（大于等于时在包裹中播放光效）  
        //public List<int>[] lGetType;                  // 获取方式（获取方式的类型，参数）
        //public string[][] lGetType;                  // 获取方式（获取方式的类型，参数）
        public int[] lGetTypeID;                 // 获取方式(ID)
        public string[][] lGetTypeParam;         // 获取方式（参数）
        public int nGuideDuration;                  // 引导使用物品界面显示时间
        public int nShowRank;                       // 展示阶数(奖励展示专用)
        public int nShowStar;                       // 展示星级(奖励展示专用)
        public bool bDisableAni;					// 是否禁用获得物品时的飘入动画
    };

    public class AnimationSoundSchemeInfo
    {
        public struct SoundFrameInfo
        {
            public int frame;
            public int audioAssetId;
        }

        public string modelName;
        public string animName;
        public List<SoundFrameInfo> soundFrameInfo = new List<SoundFrameInfo>();

        public int animNameHash;  // not serialized Animator.StringToHash(animName);

        public AnimationSoundSchemeInfo clone
        {
            get
            {
                AnimationSoundSchemeInfo info = new AnimationSoundSchemeInfo();
                info.modelName = this.modelName;
                info.animName = this.animName;
                info.soundFrameInfo.AddRange(this.soundFrameInfo);

                info.animNameHash = this.animNameHash;
                return info;
            }
        }
    }


    public interface ISchemeModule : IModule
    {
        /// <summary>
        /// 获取表接口
        /// </summary>
        /// <returns></returns>
        Igamescheme GetCgamescheme();

        /// <summary>
        /// 表格是否加载完成
        /// </summary>
        /// <returns></returns>
        bool IsLoadFinish();
    };

}