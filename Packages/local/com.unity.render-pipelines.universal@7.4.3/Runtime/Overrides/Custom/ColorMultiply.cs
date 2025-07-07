using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal.Internal
{
    [Serializable, VolumeComponentMenu("Post-processing/Color Multiply")]
    class ColorMultiply : VolumeComponent, IPostProcessComponent
    {
       
        public MaterialParameter m_mat = new MaterialParameter(null,true);

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

    /*
    [Serializable]
    public sealed class MaterialParameter : VolumeParameter<Material>
    {
        public MaterialParameter(Material value, bool overrideState = false)
            : base(value, overrideState) { }
    }
    */

}
