Shader "Character/ActorDieShader"
{
    Properties
    {
        _Color("Texture Color",Color) = (1,1,1,1)
    }
     SubShader
    {
         Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            //Blend One OneMinusSrcAlpha
            Blend One Zero
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


#define USING_DIRECTIONAL_LIGHT 1

            //启用gpu instancing
            #pragma multi_compile_instancing 


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

         
            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
               
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

          CBUFFER_START(UnityPerMaterial)
            float4 _Color;
          CBUFFER_END

            /*
            UNITY_INSTANCING_BUFFER_START(_InstancingProp)
                UNITY_DEFINE_INSTANCED_PROP(half,_Value)
            UNITY_INSTANCING_BUFFER_END(_InstancingProp)
            */

            v2f vert(appdata v)
            {
                v2f o;
                //给unity_InstanceID赋值,使urp的内部函数会自动调用unity_Builtins0数组中的属性
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.pos = vertexInput.positionCS;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                half a = 1.0f;// UNITY_ACCESS_INSTANCED_PROP(_InstancingProp, _Value);
                return half4(_Color.rgb,a);

            }
            ENDHLSL
        }


    }
}