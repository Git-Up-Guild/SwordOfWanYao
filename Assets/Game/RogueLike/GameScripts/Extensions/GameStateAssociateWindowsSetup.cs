/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.06.03
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.RogueLike
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.RogueLike.UI.RogueLogin.UIRogueLogin>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.RogueLike.UI.RogueLogin.UIRogueLogin>(1);

        }
    }
}
