/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.07.12
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.UITsetDemo
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.UITsetDemo.UI.Main.UIMain>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.UITsetDemo.UI.Main.UIMain>(1);

        }
    }
}
