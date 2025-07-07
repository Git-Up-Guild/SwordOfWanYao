// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2758,x:33573,y:32660,varname:node_2758,prsc:2|emission-4528-OUT,alpha-540-OUT;n:type:ShaderForge.SFN_Tex2d,id:4222,x:32533,y:32588,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_4222,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8292-OUT;n:type:ShaderForge.SFN_Tex2d,id:972,x:32419,y:33302,ptovrint:False,ptlb:Dissolve,ptin:_Dissolve,varname:_node_4222_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:8d2b4ecf056cd6648972451d78ac6c4f,ntxv:0,isnm:False|UVIN-9098-OUT;n:type:ShaderForge.SFN_Multiply,id:4528,x:33194,y:32698,varname:node_4528,prsc:2|A-4222-RGB,B-9686-RGB,C-9686-A,D-4949-RGB;n:type:ShaderForge.SFN_Time,id:4841,x:31472,y:32498,varname:node_4841,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:6298,x:31505,y:32322,varname:node_6298,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:8292,x:32088,y:32347,varname:node_8292,prsc:2|A-4630-OUT,B-4530-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4002,x:31710,y:32286,varname:node_4002,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-6298-U;n:type:ShaderForge.SFN_Add,id:4630,x:31917,y:32347,varname:node_4630,prsc:2|A-4002-OUT,B-2054-OUT;n:type:ShaderForge.SFN_Multiply,id:2054,x:31707,y:32451,varname:node_2054,prsc:2|A-4841-TSL,B-860-OUT;n:type:ShaderForge.SFN_ValueProperty,id:860,x:31504,y:32688,ptovrint:False,ptlb:U,ptin:_U,varname:node_860,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ComponentMask,id:4482,x:31707,y:32587,varname:node_4482,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-6298-V;n:type:ShaderForge.SFN_Add,id:4530,x:31917,y:32494,varname:node_4530,prsc:2|A-4482-OUT,B-8428-OUT;n:type:ShaderForge.SFN_Multiply,id:8428,x:31796,y:32690,varname:node_8428,prsc:2|A-4841-TSL,B-2956-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2956,x:31389,y:32785,ptovrint:False,ptlb:V,ptin:_V,varname:_node_860_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_VertexColor,id:9686,x:32533,y:32976,varname:node_9686,prsc:2;n:type:ShaderForge.SFN_Color,id:4949,x:32627,y:32776,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_4949,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:540,x:33228,y:32947,varname:node_540,prsc:2|A-4222-A,B-4949-A,C-5848-OUT,D-9686-A;n:type:ShaderForge.SFN_TexCoord,id:8381,x:31023,y:33213,varname:node_8381,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRange,id:7348,x:31238,y:33206,varname:node_7348,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-8381-UVOUT;n:type:ShaderForge.SFN_Length,id:4570,x:31449,y:33140,varname:node_4570,prsc:2|IN-7348-OUT;n:type:ShaderForge.SFN_Append,id:9098,x:31937,y:33156,varname:node_9098,prsc:2|A-5135-OUT,B-7355-OUT;n:type:ShaderForge.SFN_ComponentMask,id:6567,x:31394,y:33417,varname:node_6567,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-7348-OUT;n:type:ShaderForge.SFN_ArcTan2,id:7355,x:31640,y:33399,varname:node_7355,prsc:2,attp:2|A-6567-R,B-6567-G;n:type:ShaderForge.SFN_Multiply,id:4352,x:32074,y:33467,varname:node_4352,prsc:2|A-6154-TSL,B-4550-OUT;n:type:ShaderForge.SFN_Time,id:6154,x:31862,y:33409,varname:node_6154,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:4550,x:31886,y:33594,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:node_4550,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_Add,id:5135,x:31701,y:33140,varname:node_5135,prsc:2|A-4570-OUT,B-4352-OUT;n:type:ShaderForge.SFN_Smoothstep,id:5848,x:33035,y:33181,varname:node_5848,prsc:2|A-6757-OUT,B-2568-OUT,V-921-OUT;n:type:ShaderForge.SFN_OneMinus,id:6757,x:32818,y:33092,varname:node_6757,prsc:2|IN-2568-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2568,x:32639,y:33315,ptovrint:False,ptlb:Smooth,ptin:_Smooth,varname:_Smooth_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_Add,id:1376,x:32775,y:33466,varname:node_1376,prsc:2|A-972-R,B-314-OUT,C-4123-OUT,D-9686-A;n:type:ShaderForge.SFN_Vector1,id:314,x:32404,y:33531,varname:node_314,prsc:2,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:4131,x:32386,y:33678,ptovrint:False,ptlb:Out,ptin:_Out,varname:_Out_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_OneMinus,id:4123,x:32620,y:33634,varname:node_4123,prsc:2|IN-4131-OUT;n:type:ShaderForge.SFN_Clamp01,id:921,x:33017,y:33466,varname:node_921,prsc:2|IN-1376-OUT;proporder:4949-4222-972-860-2956-4550-2568-4131;pass:END;sub:END;*/

Shader "A_VFX/Dissolution_Blend_JZB" {
    Properties {
        [HDR]_Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Texture ("Texture", 2D) = "white" {}
        _Dissolve ("Dissolve", 2D) = "white" {}
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        _Speed ("Speed", Float ) = 0.5
        _Smooth ("Smooth", Float ) = 0.5
        _Out ("Out", Float ) = 0
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
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform sampler2D _Dissolve; uniform float4 _Dissolve_ST;
            uniform float _U;
            uniform float _V;
            uniform float4 _Color;
            uniform float _Speed;
            uniform float _Smooth;
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
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_4841 = _Time;
                float2 node_8292 = float2((i.uv0.r.r+(node_4841.r*_U)),(i.uv0.g.r+(node_4841.r*_V)));
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_8292, _Texture));
                float3 emissive = (_Texture_var.rgb*i.vertexColor.rgb*i.vertexColor.a*_Color.rgb);
                float3 finalColor = emissive;
                float2 node_7348 = (i.uv0*2.0+-1.0);
                float4 node_6154 = _Time;
                float2 node_6567 = node_7348.rg;
                float2 node_9098 = float2((length(node_7348)+(node_6154.r*_Speed)),((atan2(node_6567.r,node_6567.g)/6.28318530718)+0.5));
                float4 _Dissolve_var = tex2D(_Dissolve,TRANSFORM_TEX(node_9098, _Dissolve));
                fixed4 finalRGBA = fixed4(finalColor,(_Texture_var.a*_Color.a*smoothstep( (1.0 - _Smooth), _Smooth, saturate((_Dissolve_var.r+1.0+(1.0 - _Out)+i.vertexColor.a)) )*i.vertexColor.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
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
