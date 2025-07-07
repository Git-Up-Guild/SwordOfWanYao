/*******************************************************************
** 文件名:	ReddotConfigData.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2024/11/3 17:09:57
** 版  本:	1.0
** 描  述:	红点系统
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using XGame.Reddot;

namespace XClient.Reddot
{
    internal class ReddotConfigData : ReddotConfigDataBase
    {
        private cfg_Reddot _config;
        private cfg_Reddot Config
        {
            get
            {
                if (_config == null)
                {
                    _config = OriData as cfg_Reddot;
                }
                   
                return _config;
            }
        }

        private List<StyleData> _lstStyleData = new List<StyleData>();

        public override int ID => Config.iID;

        public override bool IsUseSubID => Config.bUseSubID == 1;

        public override int ShowLimit => Config.enShowLimit;

        public override int FinishMode => Config.enFinishMode;

        public override string FinishModeParams => Config.szFinishParams;

        public override int Priority => Config.iPriority;

        public override int FunctionID => Config.iFunctionID;

        public override StyleData GetStyleParams(int appMode)
        {
            foreach(var data in _lstStyleData)
            {
                if (data.AppMode == appMode)
                    return data;
            }
            return null;
        }

        public override void OnParse(Dictionary<int, ReddotConfigDataBase> allConfigs)
        {
            //解析风格数据
            var len = Config.arrStyleParams.Length;
            var count = (int)(len / 4);
            for(var i = 0; i < count; i++)
            {
                var idx = i * 4;
                var styleData = new StyleData();
                styleData.AppMode = Config.arrStyleParams[idx++];
                styleData.ID = Config.arrStyleParams[idx++];
                styleData.XPos = Config.arrStyleParams[idx++];
                styleData.YPos = Config.arrStyleParams[idx++];

                _lstStyleData.Add(styleData);
            }

            //层次关系
            foreach(var c in Config.arrChildren)
            {
                if(allConfigs.TryGetValue(c, out var child))
                {
                    if(child != null)
                    {
                        if(!Children.Contains(child))
                            Children.Add(child);

                        if (!child.Parents.Contains(this))
                            child.Parents.Add(this);
                    }
                }
            }
        }
    }
}
