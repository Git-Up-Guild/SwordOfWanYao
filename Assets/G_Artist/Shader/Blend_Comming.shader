

Shader "Blend_Comming"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Texture Color",Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {

                Blend SrcAlpha OneMinusSrcAlpha
                ZTest LEqual
                ZWrite Off


                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag


    #define USING_DIRECTIONAL_LIGHT 1

            //启用gpu instancing
            #pragma multi_compile_instancing 


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            //自定义数组,保存每个实例的颜色
            //StructuredBuffer<float4> _instancing_color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
               // half3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {

                //float3 positionWS               : TEXCOORD2;
                //float4 positionCS               : SV_POSITION;


                float4 pos : SV_POSITION;
                //float3 worldPos : TEXCOORD0;
                //half3 worldNormal : TEXCOORD1;
                //half3 viewDir : TEXCOORD2;
                float2 uv : TEXCOORD0;
                //half3 sh : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
            CBUFFER_END



            v2f vert(appdata v)
            {
                v2f o;
                //给unity_InstanceID赋值,使urp的内部函数会自动调用unity_Builtins0数组中的属性
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
               // VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal.xyz);

                o.pos = vertexInput.positionCS;
                //o.worldPos = vertexInput.positionWS;
               // o.worldNormal = normalInput.normalWS;
                //o.sh = SampleSH(o.worldNormal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //o.viewDir = GetCameraPositionWS() - vertexInput.positionWS;



                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);


            float4 color =  tex2D(_MainTex, i.uv)* _Color;

 /*
            if (color.a < 0.1f)
            {
                color.a = 0.0f;
             }
*/

            return color;

            /*
#ifdef USING_DIRECTIONAL_LIGHT
            half nl = max(0, dot(i.worldNormal, _MainLightPosition.xyz));
#else
            half nl = max(0, dot(i.worldNormal, normalize(_MainLightPosition.xyz - i.worldPos)));
#endif
            half3 albedo = tex2D(_MainTex, i.uv);

            float coff = nl * 0.5 + 0.5;



            half3 viewDir = normalize(i.viewDir);
            half3 normal = normalize(i.worldNormal);
            half3 halfDir = normalize(viewDir + normal);

            half3 ambient = _Ambient.xyz * _Ambient.a;// ShadeSHPerPixel(i.worldNormal, i.sh, i.worldPos);

            //half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

            //return half4(ambient, 1);
            //return half4(i.sh, 1);
            half3 lightIntensity = _MainLightColor.rgb * coff + ambient;

            half3 spec = _SpecColor * pow(saturate(dot(halfDir, normal)), _Gloss);
            half3 c = albedo.rgb * _Color * (lightIntensity + spec);
            //UNITY_APPLY_FOG(i.fogCoord, c);

            half4 outputColor = half4(c, 1);


            return outputColor;
            */

            }
            ENDHLSL
        }

        Pass //产生阴影
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
          

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

                // -------------------------------------
                // Material Keywords
                #pragma shader_feature_local_fragment _ALPHATEST_ON
                #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

                //--------------------------------------
                // GPU Instancing
                #pragma multi_compile_instancing
               // #pragma multi_compile _ DOTS_INSTANCING_ON

                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
                ENDHLSL
            }

            Pass //写入深度
            {
                Name "DepthOnly"
                Tags{"LightMode" = "DepthOnly"}

                ZWrite On
                ColorMask 0
    

                HLSLPROGRAM
                #pragma exclude_renderers gles gles3 glcore
                #pragma target 4.5

                #pragma vertex DepthOnlyVertex
                #pragma fragment DepthOnlyFragment

                // -------------------------------------
                // Material Keywords
                #pragma shader_feature_local_fragment _ALPHATEST_ON
                #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

                //--------------------------------------
                // GPU Instancing
                #pragma multi_compile_instancing
                #pragma multi_compile _ DOTS_INSTANCING_ON

                #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
                ENDHLSL
            }
        }
}