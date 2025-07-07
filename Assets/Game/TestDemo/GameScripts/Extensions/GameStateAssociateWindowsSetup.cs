/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.06.28
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.TestDemo
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.TestDemo.UI.ZCLogin.UIZCLogin>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.TestDemo.UI.ZCLogin.UIZCLogin>(1);
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.TestDemo.UI.Main.UIMain>(2);

        }
    }
}
