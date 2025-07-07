/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: IHeroCardModule.cs 
Module: HeroCard
Author: 许德纪
Date: 2025.05.09
Description: 影响卡模块
***************************************************************************/

using System.Collections.Generic;
using XClient.Common;

namespace GameScripts.CardDemo.HeroCard
{


    public class HeroCardData
    {
        public string name;
        public string state;
        public int power;
    }


    public interface IHeroCardModule : IModule
    {
        //获取当前所有的英雄卡

         List<HeroCardData> GetHeroCardData();
    }
}