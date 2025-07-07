using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal.Internal
{
    [Serializable, VolumeComponentMenu("Post-processing/CaptureBlur")]
    public class CaptureBlur : VolumeComponent, IPostProcessComponent
    {
        //模糊采样的材质
        static public Material s_MatCupture;

        //模糊采样的RT
        static public RenderTexture s_RT;

        //模糊系数
        static public float s_Blur;

        //降采样系数
        static public int s_Downsample;

        //模糊迭代次数
        static public int s_Loop;

        //颜色
        static public Color s_BlurColor;

        //是否启用
        static public bool s_Enable;

        //最大模糊迭代次数
        public const int BLUR_MIP_MAX_LEVL = 8;

        public MaterialParameter _Mat = new MaterialParameter(null,true);
        public RTParameter _RT = new RTParameter(null, true);
        public ClampedFloatParameter _Blur = new ClampedFloatParameter(0.7f, 0f, 20f);
        public ClampedIntParameter _BlurDownSample = new ClampedIntParameter(2, 1, 8);
        public ClampedIntParameter _BlurLoop = new ClampedIntParameter(2, 1, BLUR_MIP_MAX_LEVL);
        public ColorParameter _BlurColor = new ColorParameter(Color.white);

        public void Setup()
        {
            _Mat.value = s_MatCupture;
            _RT.value = s_RT;
            _Blur.value = s_Blur;
            _BlurLoop.value = s_Loop;
            _BlurDownSample.value = s_Downsample;
            _BlurColor.value = s_BlurColor;
        }

        public bool IsActive()
        {
            return s_Enable && _Mat.value != null && _RT.value != null;
        }

        public bool IsTileCompatible()
        {
            return false;
        }

    }

    [Serializable]
    public sealed class MaterialParameter : VolumeParameter<Material>
    {
        public MaterialParameter(Material value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable]
    public sealed class RTParameter : VolumeParameter<RenderTexture>
    {
        public RTParameter(RenderTexture value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}
