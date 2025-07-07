
using UnityEngine;
using XGame.Entity.Part;
using XGame.Utils;

namespace XClient.Entity
{
    public class RoleDataPart : BasePart
    {
        /// <summary>
        /// 金币数量
        /// </summary>
        public ListenableValue<int> goldNum;

        /// <summary>
        /// 钻石数量
        /// </summary>
        public ListenableValue<int> diamondNum;

        /// <summary>
        /// 钻石数量（充值获得）
        /// </summary>
        public ListenableValue<int> diamondNum2;

        /// <summary>
        /// 等级
        /// </summary>
        public ListenableValue<int> level;

        /// <summary>
        /// 经验
        /// </summary>
        public ListenableValue<int> exp;

        /// <summary>
        /// 位置
        /// </summary>
        public ListenableValue<Vector3> position;

        protected override void OnInit(object context)
        {
            base.OnInit(context);

            goldNum = ListenableValue<int>.Get();
            diamondNum = ListenableValue<int>.Get();
            diamondNum2 = ListenableValue<int>.Get();
            level = ListenableValue<int>.Get();
            exp = ListenableValue<int>.Get();
            position = ListenableValue<Vector3>.Get();
        }

        protected override void OnReset()
        {
            base.OnReset();

            goldNum?.Release();
            goldNum = null;

            diamondNum?.Release();
            diamondNum = null;

            diamondNum2?.Release();
            diamondNum2 = null;

            level?.Release();
            level = null;

            exp?.Release();
            exp = null;

            position?.Release();
            position = null;
        }
    }
}
