// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2758,x:33275,y:32658,varname:node_2758,prsc:2|emission-4528-OUT,alpha-540-OUT;n:type:ShaderForge.SFN_Tex2d,id:4222,x:32550,y:32724,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_4222,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-8292-OUT;n:type:ShaderForge.SFN_Tex2d,id:972,x:32518,y:32913,ptovrint:False,ptlb:Mask,ptin:_Mask,varname:_node_4222_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7552-OUT;n:type:ShaderForge.SFN_Multiply,id:4528,x:32940,y:32697,varname:node_4528,prsc:2|A-4222-RGB,B-972-RGB,C-9686-RGB,D-9686-A,E-4949-RGB;n:type:ShaderForge.SFN_Time,id:4841,x:31663,y:32781,varname:node_4841,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:6298,x:31696,y:32605,varname:node_6298,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:8292,x:32279,y:32630,varname:node_8292,prsc:2|A-4630-OUT,B-4530-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4002,x:31901,y:32569,varname:node_4002,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-6298-U;n:type:ShaderForge.SFN_Add,id:4630,x:32108,y:32630,varname:node_4630,prsc:2|A-4002-OUT,B-2054-OUT;n:type:ShaderForge.SFN_Multiply,id:2054,x:31898,y:32734,varname:node_2054,prsc:2|A-4841-TSL,B-860-OUT;n:type:ShaderForge.SFN_ValueProperty,id:860,x:31695,y:32971,ptovrint:False,ptlb:U,ptin:_U,varname:node_860,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ComponentMask,id:4482,x:31898,y:32870,varname:node_4482,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-6298-V;n:type:ShaderForge.SFN_Add,id:4530,x:32108,y:32777,varname:node_4530,prsc:2|A-4482-OUT,B-8428-OUT;n:type:ShaderForge.SFN_Multiply,id:8428,x:31898,y:33034,varname:node_8428,prsc:2|A-4841-TSL,B-2956-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2956,x:31685,y:33080,ptovrint:False,ptlb:V,ptin:_V,varname:_node_860_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_VertexColor,id:9686,x:32604,y:33069,varname:node_9686,prsc:2;n:type:ShaderForge.SFN_Color,id:4949,x:32631,y:32538,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_4949,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:540,x:32921,y:32907,varname:node_540,prsc:2|A-4949-A,B-4222-A,C-972-A,D-9686-A;n:type:ShaderForge.SFN_Time,id:2229,x:31653,y:33280,varname:node_2229,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:1942,x:31686,y:33104,varname:node_1942,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Append,id:7552,x:32269,y:33129,varname:node_7552,prsc:2|A-2187-OUT,B-9802-OUT;n:type:ShaderForge.SFN_ComponentMask,id:6929,x:31891,y:33068,varname:node_6929,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1942-U;n:type:ShaderForge.SFN_Add,id:2187,x:32098,y:33129,varname:node_2187,prsc:2|A-6929-OUT,B-5311-OUT;n:type:ShaderForge.SFN_Multiply,id:5311,x:31888,y:33233,varname:node_5311,prsc:2|A-2229-TSL,B-4662-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4662,x:31685,y:33470,ptovrint:False,ptlb:U_mask,ptin:_U_mask,varname:_U_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ComponentMask,id:4246,x:31888,y:33369,varname:node_4246,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-1942-V;n:type:ShaderForge.SFN_Add,id:9802,x:32098,y:33276,varname:node_9802,prsc:2|A-4246-OUT,B-3392-OUT;n:type:ShaderForge.SFN_Multiply,id:3392,x:31888,y:33533,varname:node_3392,prsc:2|A-2229-TSL,B-6952-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6952,x:31675,y:33579,ptovrint:False,ptlb:V_mask,ptin:_V_mask,varname:_V_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;proporder:4949-4222-972-860-2956-4662-6952;pass:END;sub:END;*/

Shader "AB_VFX/Additive_Mask" {
    Properties {
        [HDR]_Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Texture ("Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        _U_mask ("U_mask", Float ) = 0
        _V_mask ("V_mask", Float ) = 0
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
            Blend SrcAlpha One
            //Cull Off
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
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _U;
            uniform float _V;
            uniform float4 _Color;
            uniform float _U_mask;
            uniform float _V_mask;
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
                half4 node_4841 = _Time;
                half2 node_8292 = float2((i.uv0.r.r+(node_4841.r*_U)),(i.uv0.g.r+(node_4841.r*_V)));
                fixed4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_8292, _Texture));
                half4 node_2229 = _Time;
                half2 node_7552 = float2((i.uv0.r.r+(node_2229.r*_U_mask)),(i.uv0.g.r+(node_2229.r*_V_mask)));
                fixed4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(node_7552, _Mask));
                fixed3 emissive = (_Texture_var.rgb*_Mask_var.rgb*i.vertexColor.rgb*i.vertexColor.a*_Color.rgb);
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
            //Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
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
