Shader "A_VFX/Niuqu"
{
	Properties
	{
		_Texture ("扭曲图", 2D) = "white" {}
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        _NiuquInt ("扭曲度", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Float) = 1
	}

	SubShader
	{
		Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
			GrabPass
		{
			Name "BASE"
			Tags{ "LightMode" = "ForwardBase" }
		}
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            //Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Texture;
            uniform float4 _Texture_ST;
            uniform float _NiuquInt;
            uniform float _U;
            uniform float _V;
struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                //o.projPos = ComputeScreenPos (o.pos);
                o.projPos = ComputeGrabScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                half2 uv = float2((i.uv0.r.r+(_Time.x * _U)),(i.uv0.g.r+(_Time .x * _V)));
                fixed4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(uv, _Texture));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (float2(_Texture_var.r,_Texture_var.g)*i.vertexColor.a*_NiuquInt*_Texture_var.a);
                fixed4 sceneColor = tex2D(_GrabTexture, sceneUVs);

                return fixed4(sceneColor.rgb, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}