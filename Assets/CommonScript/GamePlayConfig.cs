/*******************************************************************
** �ļ���:	GamePlayConfig.cs
** ��  Ȩ:	(C) �����������޹�˾
** ������:	���¼�
** ��  ��:	2024.11.7
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ��Ϸ�б�����

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;

namespace CommonScript
{

    [Serializable]
    public class GamePlayItem
    {
        //demo�����Դ·��
        public string gameResDir = "Game/ImmortalFamily/GameResources";

        //�����ű�Ŀ¼
        public string gameScriptDir = "Game/ImmortalFamily/GameScripts";

        //�ֱ��ʿ�
        public int resolutionWidth = 1080;

        //�ֱ��ʸ�
        public int resolutionHeight = 1920;

        //���ռ�
        public string strNamespace = "ImmortalFamily";

        //������������
        //public string setupScenePath = "";

        public ResourceRef setupScenePath;


    }

    public class GamePlayConfig : MonoBehaviour
    {

        //��Ϸ����ѡ��
        [Header("��Ϸ�б��������")]
        public List<GamePlayItem> gamePlayItems;

        //���õ�demo�±�
        public int setupGamePlayIndex;

        // Start is called before the first frame update
        void Start()
        {

            int Limit = QualitySettings.masterTextureLimit;

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


