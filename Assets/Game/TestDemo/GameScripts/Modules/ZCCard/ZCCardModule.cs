/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: ZCCardModule.cs 
Module: ZCCard
Author: mcaswen
Date: 2025.06.28
Description: 卡牌模块
***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using XClient.Common;
using XGame.Timer;
using UnityEngine;

namespace GameScripts.TestDemo.ZCCard
{

    public class ZCCardModule : IZCCardModule, ITimerHandler
    {
        public string ModuleName { get; set; }
        public ModuleState State { get; set; }
        public float Progress { get; set; }
        public int ID { get; set; }

        List<Card> m_ListCardData = new List<Card>();
        List<Exam> m_ListExamData = new List<Exam>();
        TeacherNameTable m_TeacherNames = new TeacherNameTable();
        LocationTable m_Locations = new LocationTable();
        CourseNameTable m_CourseNames = new CourseNameTable();

        private ZCCardModuleMessageHandler m_MessageHandler;

        public ZCCardModule()
        {
            ModuleName = "ZCCard";
            ID = DModuleIDEx.MODULE_ID_ZCCARD;
        }

        public bool Create(object context, object config = null)
        {
            m_MessageHandler = new ZCCardModuleMessageHandler();
            m_MessageHandler.Create(this);

            Progress = 1f;
            return true;
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
        }

        public void Release()
        {
            m_MessageHandler?.Release();
            m_MessageHandler = null;
        }

        public bool Start()
        {
            m_MessageHandler?.Start();
            //InitializeCards();
            XGame.XGameComs.Get<ITimerManager>().AddTimer(this, 1, 1000, "1");

            State = ModuleState.Success;
            return true;

        }

        public void Stop()
        {
            m_MessageHandler?.Stop();
        }

        public void Update()
        {
        }

        public void OnGameStateChange(int newStateID, int oldStateID)
        {
        }

        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后创建的数据,Create中创建的不要清理了
        public void Clear(int param)
        {
        }

        //玩家数据准备好后回调
        public void OnRoleDataReady()
        {
        }

        public void InitializeCards()
        {

            for (int i = 0; i < 100; i++)
            {

                Card card = new Card();
                card.ID = i;
                card.Number = i + 1;
                card.Name = "物品" + (i + 1);

                m_ListCardData.Add(card);

            }

        }

        public List<Card> GetAllCards()
        {

            return m_ListCardData;

        }

        public List<Exam> GetAllExams()
        {

            return m_ListExamData;

        }

        public void OnTimer(TimerInfo ti)
        {
            if (m_ListExamData.Count >= 20)
            {

                XGame.XGameComs.Get<ITimerManager>().RemoveTimer(this, 1);
                return;

            }

            Exam exam = new Exam();
            int count = m_ListExamData.Count;

            exam.Time = "2025-07-" + (count + 1).ToString("D2");

            exam.TeacherName = m_TeacherNames.GetRandomString();
            exam.Location = m_Locations.GetRandomString();
            exam.CourseName = m_CourseNames.GetRandomString();

            m_ListExamData.Add(exam);

            GameGlobal.EventEgnine.FireExecute(DGlobalEventEx.EVENT_CARD_DATA_UPDATE, 0, 0, null);

        }

    }

    public abstract class StringTable
    {
        protected List<string> strings;

        public StringTable(List<string> initStrings)
        {

            strings = initStrings;

        }
        public string GetRandomString()
        {
            if (strings.Count == 0) return "";
            return strings[Random.Range(0, strings.Count)];
        }

    }

    public class TeacherNameTable : StringTable
    {

        public TeacherNameTable() : base(new List<string>
        {

            "杜圆圆",
            "杨远帆",
            "王垚",
            "杨思博",
            "刘定奇",
            "桑拓",
            "柳杨",
            "高玉龙",
        })
        { }

    }

    public class LocationTable : StringTable
    {
        public LocationTable() : base(new List<string>
        {
            "C201",
            "C202",
            "C204",
            "C401",
            "C301",
            "C402",
        })
        { }

    }

    public class CourseNameTable : StringTable
    {
        public CourseNameTable() : base(new List<string>
        {
            "学术英语2",
            "计算思维概论",
            "设计工作坊",
            "游戏概念设计",
            "面向对象游戏程序设计",
            "计算媒介与艺术创作",
            "思想道德与法治",
            "游戏概论"
        })
        { }

    }



}
