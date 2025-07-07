// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32828,y:32343,varname:node_3138,prsc:2|emission-3178-OUT,alpha-3808-OUT;n:type:ShaderForge.SFN_Tex2d,id:5620,x:31799,y:32416,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:_node_3755_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5696-OUT;n:type:ShaderForge.SFN_Color,id:7519,x:31861,y:32223,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7519,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3808,x:32290,y:32558,varname:node_3808,prsc:2|A-7519-A,B-5620-A,C-1856-OUT;n:type:ShaderForge.SFN_Multiply,id:3178,x:32437,y:32354,varname:node_3178,prsc:2|A-7519-RGB,B-5620-RGB,C-2265-RGB;n:type:ShaderForge.SFN_TexCoord,id:1124,x:30536,y:32140,varname:node_1124,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:1397,x:30519,y:32388,varname:node_1397,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:5343,x:30781,y:32140,varname:node_5343,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1124-U;n:type:ShaderForge.SFN_Append,id:5696,x:31245,y:32192,varname:node_5696,prsc:2|A-4922-OUT,B-2730-OUT;n:type:ShaderForge.SFN_Add,id:4922,x:31004,y:32140,varname:node_4922,prsc:2|A-5343-OUT,B-6620-OUT;n:type:ShaderForge.SFN_Multiply,id:6620,x:31004,y:32304,varname:node_6620,prsc:2|A-1397-TSL,B-2708-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2708,x:30749,y:32479,ptovrint:False,ptlb:U,ptin:_U,varname:node_2708,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:8697,x:31039,y:32475,varname:node_8697,prsc:2|A-1397-TSL,B-9083-OUT;n:type:ShaderForge.SFN_Add,id:2730,x:31217,y:32423,varname:node_2730,prsc:2|A-2554-OUT,B-8697-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9083,x:30832,y:32583,ptovrint:False,ptlb:V,ptin:_V,varname:_U_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ComponentMask,id:2554,x:30781,y:32315,varname:node_2554,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1124-V;n:type:ShaderForge.SFN_Smoothstep,id:1856,x:32083,y:32817,varname:node_1856,prsc:2|A-6502-OUT,B-1916-OUT,V-3789-OUT;n:type:ShaderForge.SFN_OneMinus,id:6502,x:31723,y:32753,varname:node_6502,prsc:2|IN-1916-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1916,x:31459,y:32827,ptovrint:False,ptlb:Smooth,ptin:_Smooth,varname:node_1916,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_Tex2d,id:5048,x:31232,y:32887,ptovrint:False,ptlb:Mask_Texture,ptin:_Mask_Texture,varname:node_5048,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Add,id:5268,x:31680,y:32990,varname:node_5268,prsc:2|A-5048-R,B-377-OUT,C-7367-OUT,D-2265-A;n:type:ShaderForge.SFN_Vector1,id:377,x:31400,y:33010,varname:node_377,prsc:2,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:4423,x:31325,y:33110,ptovrint:False,ptlb:Out,ptin:_Out,varname:node_4423,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Clamp01,id:3789,x:31932,y:32990,varname:node_3789,prsc:2|IN-5268-OUT;n:type:ShaderForge.SFN_OneMinus,id:7367,x:31524,y:33066,varname:node_7367,prsc:2|IN-4423-OUT;n:type:ShaderForge.SFN_VertexColor,id:2265,x:31232,y:32704,varname:node_2265,prsc:2;proporder:7519-5620-5048-1916-4423-2708-9083;pass:END;sub:END;*/

Shader "A_VFX/Dissolution_Blend3" {
    Properties {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _Texture ("Texture", 2D) = "white" {}
        _Mask_Texture ("Mask_Texture", 2D) = "white" {}
        _Smooth ("Smooth", Float ) = 0.5
        _Out ("Out", Float ) = 0
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            //Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            //#pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float4 _Color;
            uniform float _U;
            uniform float _V;
            uniform float _Smooth;
            uniform sampler2D _Mask_Texture; uniform float4 _Mask_Texture_ST;
            uniform float _Out;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                half4 node_1397 = _Time;
                half2 node_5696 = float2((i.uv0.r.r+(node_1397.r*_U)),(i.uv0.g.r+(node_1397.r*_V)));
                fixed4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_5696, _Texture));
                fixed3 emissive = (_Color.rgb*_Texture_var.rgb*i.vertexColor.rgb);
                fixed3 finalColor = emissive;
                fixed4 _Mask_Texture_var = tex2D(_Mask_Texture,TRANSFORM_TEX(i.uv0, _Mask_Texture));
                return fixed4(finalColor,(_Color.a*_Texture_var.a*smoothstep( (1.0 - _Smooth), _Smooth, saturate((_Mask_Texture_var.r+1.0+(1.0 - _Out)+i.vertexColor.a)) )));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            //Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            //#pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
