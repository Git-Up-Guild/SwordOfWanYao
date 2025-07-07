// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9690,x:33237,y:32654,varname:node_9690,prsc:2|emission-2808-OUT,alpha-6798-OUT;n:type:ShaderForge.SFN_Tex2d,id:5886,x:32534,y:32723,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_5886,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7083-OUT;n:type:ShaderForge.SFN_Tex2d,id:5966,x:32534,y:32937,ptovrint:False,ptlb:Mask,ptin:_Mask,varname:node_5966,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5809-OUT;n:type:ShaderForge.SFN_Multiply,id:2808,x:32868,y:32765,varname:node_2808,prsc:2|A-3755-RGB,B-5886-RGB,C-5966-RGB,D-6623-RGB;n:type:ShaderForge.SFN_Color,id:3755,x:32534,y:32526,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_3755,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:6623,x:32518,y:33140,varname:node_6623,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6798,x:32899,y:33048,varname:node_6798,prsc:2|A-3755-A,B-5886-A,C-5966-A,D-6623-A;n:type:ShaderForge.SFN_TexCoord,id:3735,x:31525,y:32656,varname:node_3735,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:751,x:31746,y:32650,varname:node_751,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-3735-U;n:type:ShaderForge.SFN_Add,id:8957,x:31939,y:32617,varname:node_8957,prsc:2|A-751-OUT,B-6985-OUT;n:type:ShaderForge.SFN_Time,id:5548,x:31442,y:32824,varname:node_5548,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6985,x:31871,y:32766,varname:node_6985,prsc:2|A-5548-TSL,B-9260-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9260,x:31605,y:32836,ptovrint:False,ptlb:U,ptin:_U,varname:node_9260,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:7083,x:32130,y:32639,varname:node_7083,prsc:2|A-8957-OUT,B-9702-OUT;n:type:ShaderForge.SFN_ComponentMask,id:7753,x:31782,y:32894,varname:node_7753,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-3735-V;n:type:ShaderForge.SFN_Add,id:9702,x:32003,y:32844,varname:node_9702,prsc:2|A-7753-OUT,B-1247-OUT;n:type:ShaderForge.SFN_Multiply,id:1247,x:31820,y:33072,varname:node_1247,prsc:2|A-5548-TSL,B-6918-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6918,x:31543,y:33045,ptovrint:False,ptlb:V,ptin:_V,varname:_U_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:3404,x:31546,y:33221,varname:node_3404,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:8093,x:31767,y:33215,varname:node_8093,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-3404-U;n:type:ShaderForge.SFN_Add,id:5250,x:31960,y:33182,varname:node_5250,prsc:2|A-8093-OUT,B-7789-OUT;n:type:ShaderForge.SFN_Time,id:6369,x:31463,y:33389,varname:node_6369,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7789,x:31892,y:33331,varname:node_7789,prsc:2|A-6369-TSL,B-5015-OUT;n:type:ShaderForge.SFN_Append,id:5809,x:32151,y:33204,varname:node_5809,prsc:2|A-5250-OUT,B-329-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4842,x:31814,y:33474,varname:node_4842,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-3404-V;n:type:ShaderForge.SFN_Add,id:329,x:32024,y:33409,varname:node_329,prsc:2|A-4842-OUT,B-3582-OUT;n:type:ShaderForge.SFN_Multiply,id:3582,x:31841,y:33637,varname:node_3582,prsc:2|A-6369-TSL,B-8197-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8197,x:31564,y:33610,ptovrint:False,ptlb:V_mask,ptin:_V_mask,varname:_V_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:5015,x:31643,y:33489,ptovrint:False,ptlb:U_mask,ptin:_U_mask,varname:node_5015,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:3755-5886-5966-9260-6918-8197-5015;pass:END;sub:END;*/

Shader "A_VFX/Blended_Mask" {
    Properties {
        [HDR]_Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Texture ("Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        _V_mask ("V_mask", Float ) = 0
        _U_mask ("U_mask", Float ) = 0
        //[Toggle(_UNITY_HALF_TEXEL_OFFSET)] _UseHalfTexelOffset("Use Half Texel Offset?", Float) = 1
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
            #pragma shader_feature _UNITY_HALF_TEXEL_OFFSET
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float4 _Color;
            uniform float _U;
            uniform float _V;
            uniform float _V_mask;
            uniform float _U_mask;
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
            //�Զ��� UnityPixelSnap����ʹ��Ĭ�Ϻ���
            inline float4 _UnityPixelSnap(float4 pos)
            {
                float2 hpc = _ScreenParams.xy * 0.5f;
#ifdef _UNITY_HALF_TEXEL_OFFSET
                float2 hpcO = float2(-0.5f, 0.5f);
#else
                float2 hpcO = float2(0, 0);
#endif
                float2 pixelPos = round((pos.xy / pos.w) * hpc);
                pos.xy = (pixelPos + hpcO) / hpc * pos.w;
                return pos;
            }

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.pos = _UnityPixelSnap(o.pos);

                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
////// Lighting:
////// Emissive:
                half4 node_5548 = _Time;
                half2 node_7083 = float2((i.uv0.r.r+(node_5548.r*_U)),(i.uv0.g.r+(node_5548.r*_V)));
                fixed4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_7083, _Texture));
                half4 node_6369 = _Time;
                half2 node_5809 = float2((i.uv0.r.r+(node_6369.r*_U_mask)),(i.uv0.g.r+(node_6369.r*_V_mask)));
                fixed4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(node_5809, _Mask));
                fixed3 emissive = (_Color.rgb*_Texture_var.rgb*_Mask_var.rgb*i.vertexColor.rgb);
                fixed3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(_Color.a*_Texture_var.a*_Mask_var.a*i.vertexColor.a));
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
