/*****************************************************
** 文 件 名：PreloadEditor
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 17:04:20
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using UnityEditor;
using XGame.Preload;

namespace XGameEditor
{
    public class PreloadEditor : Editor
    {
        [MenuItem("XGame/预加载/创建配置")]
        public static void CreateConfig()
        {
            XGameEditorUtilityEx.CreateScriptableObject<PreloadConfig>();
        }
    }
}