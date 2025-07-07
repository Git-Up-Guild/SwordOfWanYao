using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal.Internal
{
    [Serializable, VolumeComponentMenu("Post-processing/Color Multiply")]
    class LevelMask : VolumeComponent, IPostProcessComponent
    {
       //渲染mask的材质
        public MaterialParameter m_mat = new MaterialParameter(null,true);

        //已经攻克了的关卡 layer (透明高亮)
        public IntParameter m_conquerLayerMask = new IntParameter(0);

        //当前攻打的关卡 layer (透明红色)
        public IntParameter m_curLayerMask = new IntParameter(0);


        public ColorParameter color = new ColorParameter(Color.white, false);


        public bool IsActive()
        {
            return m_mat.value != null;
        }
        public bool IsTileCompatible()
        {
            return false;
        }

    }

   

}
