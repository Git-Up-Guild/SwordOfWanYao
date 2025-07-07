// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:33156,y:32880,varname:node_9361,prsc:2|emission-7836-OUT,alpha-7276-OUT;n:type:ShaderForge.SFN_Tex2d,id:5737,x:32454,y:33086,varname:node_5737,prsc:2,ntxv:0,isnm:False|UVIN-7088-OUT,TEX-5091-TEX;n:type:ShaderForge.SFN_Color,id:7212,x:32429,y:32807,ptovrint:False,ptlb:color,ptin:_color,varname:node_7212,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:1509,x:32257,y:32928,varname:node_1509,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7836,x:32743,y:32990,varname:node_7836,prsc:2|A-5737-RGB,B-1509-RGB,C-7212-RGB,D-1509-A,E-1788-OUT;n:type:ShaderForge.SFN_RemapRange,id:4719,x:32338,y:33253,varname:node_4719,prsc:2,frmn:0.3,frmx:1,tomn:-1,tomx:1|IN-8333-RGB;n:type:ShaderForge.SFN_Tex2d,id:8333,x:32081,y:33353,varname:node_8333,prsc:2,ntxv:0,isnm:False|UVIN-7088-OUT,TEX-5091-TEX;n:type:ShaderForge.SFN_Multiply,id:1788,x:32626,y:33360,varname:node_1788,prsc:2|A-6183-OUT,B-2451-RGB,C-8333-A;n:type:ShaderForge.SFN_Color,id:2451,x:32520,y:33565,ptovrint:False,ptlb:glow,ptin:_glow,varname:node_2451,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2dAsset,id:5091,x:31901,y:33231,ptovrint:False,ptlb:Main Tex,ptin:_MainTex,varname:node_5091,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Add,id:9860,x:32920,y:33269,varname:node_9860,prsc:2|A-7836-OUT,B-1788-OUT;n:type:ShaderForge.SFN_Multiply,id:7276,x:32931,y:33124,varname:node_7276,prsc:2|A-5737-A,B-8333-A;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:6183,x:32338,y:33490,varname:node_6183,prsc:2|IN-8333-RGB,IMIN-3941-OUT,IMAX-73-OUT,OMIN-1860-OUT,OMAX-73-OUT;n:type:ShaderForge.SFN_Vector1,id:3941,x:32081,y:33504,varname:node_3941,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:73,x:32081,y:33590,varname:node_73,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:1860,x:32081,y:33705,ptovrint:False,ptlb:Out,ptin:_Out,varname:node_1860,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_TexCoord,id:9436,x:31529,y:32775,varname:node_9436,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:6468,x:31817,y:32755,varname:node_6468,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-9436-U;n:type:ShaderForge.SFN_Time,id:5971,x:31483,y:33041,varname:node_5971,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:3462,x:31608,y:32947,ptovrint:False,ptlb:U,ptin:_U,varname:node_3462,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:9766,x:31848,y:32913,varname:node_9766,prsc:2|A-3462-OUT,B-5971-TSL;n:type:ShaderForge.SFN_Add,id:6991,x:32021,y:32803,varname:node_6991,prsc:2|A-6468-OUT,B-9766-OUT;n:type:ShaderForge.SFN_Append,id:7088,x:32185,y:32711,varname:node_7088,prsc:2|A-6991-OUT,B-9435-OUT;n:type:ShaderForge.SFN_ComponentMask,id:4795,x:31755,y:33026,varname:node_4795,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-9436-V;n:type:ShaderForge.SFN_ValueProperty,id:8299,x:31632,y:33202,ptovrint:False,ptlb:V,ptin:_V,varname:_node_3462_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:1008,x:31943,y:33047,varname:node_1008,prsc:2|A-8299-OUT,B-5971-TSL;n:type:ShaderForge.SFN_Add,id:9435,x:32094,y:32992,varname:node_9435,prsc:2|A-4795-OUT,B-1008-OUT;proporder:7212-2451-5091-1860-3462-8299;pass:END;sub:END;*/

Shader "A_VFX/lizi_katong" {
    Properties {
        [HDR]_color ("color", Color) = (0.5,0.5,0.5,1)
        [HDR]_glow ("glow", Color) = (0.5,0.5,0.5,1)
        _MainTex ("Main Tex", 2D) = "white" {}
        _Out ("Out", Range(0, 1)) = 1
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
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _color;
            uniform float4 _glow;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Out;
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
                half4 node_5971 = _Time;
                float2 node_7088 = float2((i.uv0.r.r+(_U*node_5971.r)),(i.uv0.g.r+(_V*node_5971.r)));
                fixed4 node_5737 = tex2D(_MainTex,TRANSFORM_TEX(node_7088, _MainTex));
                fixed4 node_8333 = tex2D(_MainTex,TRANSFORM_TEX(node_7088, _MainTex));
                fixed node_3941 = 0.5;
                fixed node_73 = 1.0;
                fixed3 node_1788 = ((_Out + ( (node_8333.rgb - node_3941) * (node_73 - _Out) ) / (node_73 - node_3941))*_glow.rgb*node_8333.a);
                fixed3 node_7836 = (node_5737.rgb*i.vertexColor.rgb*_color.rgb*i.vertexColor.a*node_1788);
                fixed3 emissive = node_7836;
                fixed3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(node_5737.a*node_8333.a));
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
