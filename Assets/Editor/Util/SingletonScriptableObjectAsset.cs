/*****************************************************
** 文 件 名：SingletonScriptableObjectAsset
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 17:04:20
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using UnityEngine;
using XGame.Utils;
using XGameEditor.Config;

namespace XGameEditor
{
    public class SingletonScriptableObjectAsset<T> where T : ScriptableObject
    {
        public static  T Instance(bool autoCreate = true, string path = null)
        {
            return ScriptableObjectSingletonManager.Create<T>(autoCreate, path);
        }
    }
}
