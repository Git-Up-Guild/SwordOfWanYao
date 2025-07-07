Shader "Unlit/ScoreShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Score("Score", Vector)  = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;


            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Score;
            CBUFFER_END

           

            v2f vert (appdata v)
            {
                v2f o;

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(positionWS);

                //o.vertex = UnityObjectToClipPos(v.vertex);

                // 第一为，描述是否是符号
                if (_Score.x > 0)
                {

                    o.uv = v.uv * 0.25 + float2(0.25* _Score.y, 0.75);

                    /*
                    if (_Score.y > 0)
                    {
                        o.uv = v.uv * 0.25 + float2(0.25, 0.75);
                    }
                    else
                    {
                        o.uv = v.uv * 0.25 + float2(0, 0.75);
                    }
                    */
                }
                else
                {
                    float row = floor(_Score.y / 4);
                    float col = ceil(_Score.y-row*4);
                    o.uv = v.uv * 0.25 + float2(col,row)*0.25;

                }

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
