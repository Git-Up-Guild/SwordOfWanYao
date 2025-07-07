/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: IZCCardModule.cs 
Module: ZCCard
Author: mcaswen
Date: 2025.06.28
Description: 卡牌模块
***************************************************************************/

using System.Collections.Generic;
using XClient.Common;

namespace GameScripts.TestDemo.ZCCard
{
    public interface IZCCardModule : IModule
    {

        List<Card> GetAllCards();
        List<Exam> GetAllExams();

    }

    public class Card
    {

        public int ID;
        public int Number;
        public string Name;

    }

    public class Exam
    {
        public string Time;
        public string TeacherName;
        public string Location;
        public string CourseName;

    }
    
    
}