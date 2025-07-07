// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
//Shader "VFX/PandaShaderSampleV1.0"
Shader "VFX/PandaShaderNew"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_Scr("Scr", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Int) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Int) = 0
		
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle]_MainTexAR("MainTexAR", Int) = 0
		[HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainAlpha("MainAlpha", Range( 0 , 10)) = 1
		_MainTexUSpeed("MainTexUSpeed", Float) = 0
		_MainTexVSpeed("MainTexVSpeed", Float) = 0
		
		[Toggle]_CustomMainTex("CustomMainTex", Int) = 0

        //实际没用，用来做容错的
        [HideDisabled]_ColorMask("ColorMask", Color) = (0,0,0,0)
		
		//遮罩功能
		[Toggle(_FMASKTEX_ON)] _FMaskTex("FMaskTex", Int) = 0
		[HideDisabled(_FMASKTEX_ON)]_MaskTex("MaskTex", 2D) = "white" {}
		[HideDisabled(_FMASKTEX_ON)][Toggle]_MaskTexAR("MaskTexAR", Int) = 1
		[HideDisabled(_FMASKTEX_ON)]_MaskTexUSpeed("MaskTexUSpeed", Float) = 0
		[HideDisabled(_FMASKTEX_ON)]_MaskTexVSpeed("MaskTexVSpeed", Float) = 0
		
		//扭曲功能
		[Toggle(_FDISTORTTEX_ON)] _FDistortTex("FDistortTex", Int) = 0
		[HideDisabled(_FDISTORTTEX_ON)]_DistortTex("DistortTex", 2D) = "white" {}
		[HideDisabled(_FDISTORTTEX_ON)][Toggle]_DistortTexAR("DistortTexAR", Int) = 1
		[HideDisabled(_FDISTORTTEX_ON)]_DistortFactor("DistortFactor", Range( 0 , 1)) = 0
		[HideDisabled(_FDISTORTTEX_ON)]_DistortTexUSpeed("DistortTexUSpeed", Float) = 0
		[HideDisabled(_FDISTORTTEX_ON)]_DistortTexVSpeed("DistortTexVSpeed", Float) = 0
		[HideDisabled(_FDISTORTTEX_ON)][Toggle]_DistortMainTex("DistortMainTex", Int) = 0
		[HideDisabled(_FDISTORTTEX_ON)][Toggle]_DistortMaskTex("DistortMaskTex", Int) = 0
		[HideDisabled(_FDISTORTTEX_ON)][Toggle]_DistortDissolveTex("DistortDissolveTex", Int) = 0
		
		//溶解功能
		[Toggle(_FDISSOLVETEX_ON)] _FDissolveTex("FDissolveTex", Int) = 0
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveTex("DissolveTex", 2D) = "white" {}
		[HideDisabled(_FDISSOLVETEX_ON)][Toggle]_DissolveTexAR("DissolveTexAR", Int) = 1
		[HideDisabled(_FDISSOLVETEX_ON)][HDR]_DissolveColor("DissolveColor", Color) = (1,1,1,1)
		[HideDisabled(_FDISSOLVETEX_ON)][Toggle]_CustomDissolve("CustomDissolve", Int) = 0
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveFactor("DissolveFactor", Range( 0 , 1)) = 0
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveSoft("DissolveSoft", Range( 0 , 1)) = 0.1
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveWide("DissolveWide", Range( 0 , 1)) = 0.05
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveTexUSpeed("DissolveTexUSpeed", Float) = 0
		[HideDisabled(_FDISSOLVETEX_ON)]_DissolveTexVSpeed("DissolveTexVSpeed", Float) = 0
        [HideDisabled(_FDISSOLVETEX_ON)]_PackInfo("PackInfo", Vector) = (0,0,1,1)
		
		//菲尼尔功能
		[Toggle(_FFNL_ON)] _FFnl("FFnl", Int) = 0
		[HideDisabled(_FFNL_ON)][HDR]_FnlColor("FnlColor", Color) = (1,1,1,1)
		[HideDisabled(_FFNL_ON)]_FnlScale("FnlScale", Range( 0 , 2)) = 0
		[HideDisabled(_FFNL_ON)]_FnlPower("FnlPower", Range( 1 , 10)) = 1
		[HideDisabled(_FFNL_ON)][Toggle]_ReFnl("ReFnl", Int) = 0
		
		//软粒子
		[Toggle(_FDEPTH_ON)] _FDepth("FDepth", Int) = 0
		[HideDisabled(_FDEPTH_ON)]_DepthFade("DepthFade", Range( 0 , 10)) = 1
		[Enum(Alpha,0,Add,1)]_BlendMode("BlendMode", Float) = 0
		//[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		//[HideInInspector] _texcoord( "", 2D ) = "white" {}
		//[HideInInspector] __dirty( "", Int ) = 1


        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0"  }
		
		Pass{
            Cull [_CullMode]
            ZWrite Off
            ZTest LEqual
            Blend [_Scr] [_Dst]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityShaderVariables.cginc"
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ _FDISSOLVETEX_ON
            //#pragma multi_compile_local _ USE_CUSTOM_MAINTEX
            #pragma multi_compile_local _ _FDISTORTTEX_ON
            #pragma multi_compile_local _ _FFNL_ON
            #pragma multi_compile_local _ _FMASKTEX_ON
            #pragma multi_compile_local _ _FDEPTH_ON
            
            half _Dst;
            half _CullMode;
            half _Scr;
            half _BlendMode;
            
            float4 _MainColor;
            float4 _ColorMask;

            sampler2D _MainTex;
            float _MainTexUSpeed;
            float _MainTexVSpeed;
            half _CustomMainTex;
            float4 _MainTex_ST;
            half _MainAlpha;
            
            half _DistortMainTex;
            half _DistortTexAR;
            sampler2D _DistortTex;
            float _DistortTexUSpeed;
            float _DistortTexVSpeed;
            float4 _DistortTex_ST;
            half _DistortFactor;
            
            float4 _DissolveColor;
            half _CustomDissolve;
            half _DissolveFactor;
            half _DissolveWide;
            half _DissolveSoft;
            half _DissolveTexAR;
            sampler2D _DissolveTex;
            float _DissolveTexUSpeed;
            float _DissolveTexVSpeed;
            float4 _DissolveTex_ST;
            half _DistortDissolveTex;

            //打包图集的信息
            float4 _PackInfo;
            
            half _ReFnl;
            half4 _FnlColor;
            half _FnlScale;
            half _FnlPower;
            half _MainTexAR;
            half _MaskTexAR;
            sampler2D _MaskTex;
            float _MaskTexUSpeed;
            float _MaskTexVSpeed;
            float4 _MaskTex_ST;
            half _DistortMaskTex;
            UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            
            half _DepthFade;
            
            //float4 _texcoord;
            //float4 _texcoord2;
            //float4 _texcoord_ST;
            //float4 _texcoord2_ST;
            
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct VertexOutput {
                UNITY_POSITION(pos);
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };



            float2 getNormalizedUV(float2 uv) {
                //归一化的uv
                float2 normalizedUV;
                // (uv.x - 图片在图集的初始uv位置)/(图片在图集的宽的占比)
                normalizedUV.x = (uv.x - _PackInfo.x) / _PackInfo.z;
                // (uv.y - 图片在图集的初始uv位置)/(图片在图集的高的占比)
                normalizedUV.y = (uv.y - _PackInfo.y) / _PackInfo.w;

                return normalizedUV;
            }

            
            VertexOutput vert (VertexInput v) {
                UNITY_SETUP_INSTANCE_ID(v);
                VertexOutput o = (VertexOutput)0;
                UNITY_TRANSFER_INSTANCE_ID(v,o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.uv0 = v.texcoord;
                o.uv1 = v.texcoord1;
                //o.uv0 = float4(v.texcoord.xy * _texcoord_ST.xy + _texcoord.zw, _texcoord.z, _texcoord.w);
                //o.uv1 = float4(v.texcoord1.xy * _texcoord2_ST.xy + _texcoord2.zw, _texcoord2.z, _texcoord2.w);
                
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal( v.normal );
                o.screenPos = ComputeScreenPos (o.pos);
                return o;
            }
            
            half4 frag(VertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                
                //i.vertexColor.x = 1;
                //i.uv0.x = 1;
                //i.uv1.x = 1;
                //i.worldPos.x = 1;
                //i.worldNormal.x = 1;
                //i.screenPos.x = 1;
            
                float2 uv_MainTex = TRANSFORM_TEX(i.uv0,_MainTex);
                float2 curUVMainTex = uv_MainTex;
                float  curXOffset = 0;
                float  curYOffset = 0;
     
                #if defined (_FDISTORTTEX_ON) //扭曲效果
                    float2 uv_DistortTex = TRANSFORM_TEX(i.uv0,_DistortTex);
                    float4 texDisort = tex2D(_DistortTex, float2( ((_Time.y )* _DistortTexUSpeed + uv_DistortTex.x), ((_Time.y) * _DistortTexVSpeed + uv_DistortTex.y)));
                    half distortUse = (( _DistortTexAR == 0 ? texDisort.a : texDisort.r ) * _DistortFactor );
                    curUVMainTex = ( _DistortMainTex == 0 ? uv_MainTex : ( uv_MainTex + distortUse ) );
                    //curXOffset = curXOffset + distortUse;
                    //curYOffset = curYOffset + distortUse;

                   
                #endif
                
                curUVMainTex.x = (_CustomMainTex ) == 1 ? (curUVMainTex.x  + i.uv1.x): (curUVMainTex.x) ;
                curUVMainTex.y = (_CustomMainTex ) == 1 ? (curUVMainTex.y  + i.uv1.y): (curUVMainTex.y) ;

                //#if defined (USE_CUSTOM_MAINTEX)	//主贴图自定义，用了UV2
                    //curUVMainTex.x += i.uv1.x;
                    //curUVMainTex.y += i.uv1.y;
                //#endif
    
                float4 mainTexSam = tex2D( _MainTex, float2( ((_Time.y )* _MainTexUSpeed + curUVMainTex.x ),  ((_Time.y) * _MainTexVSpeed + curUVMainTex.y )) );
                half4 mainTexColor = _MainColor * mainTexSam;
                half mainTexAlpha = ( _MainColor.a * ( _MainTexAR == 0 ? mainTexSam.a : mainTexSam.r ) );
                mainTexAlpha = mainTexAlpha * i.vertexColor.a * _MainAlpha;
    
                #if defined (_FDISSOLVETEX_ON) //溶解效果
                    half dissolve_Temp1 = (-_DissolveWide + (_CustomDissolve == 1 ? i.uv1.z : _DissolveFactor))* (1.0 + _DissolveWide);
                    half dissolve_Temp2 = ( _DissolveSoft + 0.0001 );
                    half dissolve_Temp3 = (-dissolve_Temp2 + (( dissolve_Temp1 + _DissolveWide )) * (1.0 + dissolve_Temp2));
                    float2 dissolve_uv = getNormalizedUV(i.uv0);
                    float2 uv_DissolveTex = TRANSFORM_TEX(dissolve_uv,_DissolveTex);

                    

                    #if defined (_FDISTORTTEX_ON)
                        //uv_DissolveTex.x = uv_DissolveTex.x + distortUse;
                        //uv_DissolveTex.y = uv_DissolveTex.y + distortUse;
                        curXOffset = (_DistortDissolveTex == 0 ? 0 : distortUse) ;
                        curYOffset = (_DistortDissolveTex == 0 ? 0 : distortUse) ;

                        
                    #endif      
                    float4 texDissolve = tex2D( _DissolveTex, float2( ((_Time.y) * _DissolveTexUSpeed + uv_DissolveTex.x + curXOffset), ((_Time.y) * _DissolveTexVSpeed + uv_DissolveTex.y + curYOffset)));
                    half dissolveUse = ( _DissolveTexAR == 0 ? texDissolve.a : texDissolve.r );   
                    half dissolve_Temp4 = smoothstep( dissolve_Temp3 , ( dissolve_Temp3 + dissolve_Temp2 ) , dissolveUse); 
                    mainTexColor = lerp( mainTexColor , _DissolveColor , ( _DissolveColor.a * ( 1.0 - dissolve_Temp4 ) * _MainAlpha ));
                    half dissolve_Temp5 = (-dissolve_Temp2 + dissolve_Temp1  * (1.0 + dissolve_Temp2));
                    half dissolve_Temp6 = smoothstep( dissolve_Temp5 , ( dissolve_Temp5 + dissolve_Temp2 ) , dissolveUse);          
                    mainTexAlpha = mainTexAlpha * dissolve_Temp6;
                    //curXOffset = curXOffset + dissolveUse;
                    //curYOffset = curYOffset + dissolveUse;
                #endif
                
                mainTexColor = i.vertexColor *  mainTexColor;
    
                #if defined (_FFNL_ON) //菲尼尔效果

                
                    half finierNdot = dot( i.worldNormal, normalize( UnityWorldSpaceViewDir( i.worldPos)));
                    half finierTemp1 = saturate( _FnlScale * pow( 1.0 - finierNdot, _FnlPower ) );
                    half4 finierMainColor = _FnlColor * finierTemp1 * _FnlColor.a ;
                    finierMainColor = (_ReFnl == 0 ? finierMainColor : half4(0,0,0,0) );
                    half4 finierTemp2 = (_ReFnl == 0 ? 1 : (1.0 - finierTemp1));
                    mainTexColor = mainTexColor + i.vertexColor * finierMainColor;
                    mainTexAlpha = mainTexAlpha * finierTemp2;
                #endif	
    
                #if defined (_FMASKTEX_ON) //遮罩效果

                    

                    float2 uv_MaskTex = TRANSFORM_TEX(i.uv0,_MaskTex);
                    #if defined (_FDISTORTTEX_ON)

                   
                        //uv_MaskTex.x = uv_MaskTex.x + distortUse;
                        //uv_MaskTex.y = uv_MaskTex.y + distortUse;
                        curXOffset = ( _DistortMaskTex == 0 ? 0 : ( distortUse ) );
                        curYOffset = ( _DistortMaskTex == 0 ? 0 : ( distortUse ) );
                    #endif
                    float4 texMask = tex2D( _MaskTex, float2( ((_Time.y)* _MaskTexUSpeed + uv_MaskTex.x + curXOffset) , ((_Time.y) * _MaskTexVSpeed + uv_MaskTex.y + curYOffset)));
                    half maskUse = ( _MaskTexAR == 0 ? texMask.a : texMask.r );
                    mainTexAlpha = mainTexAlpha * maskUse;
                #endif
            
                #if defined (_FDEPTH_ON) //软粒子效果

                   
                    float4 screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
                    half4 screenPosNorm = screenPos / screenPos.w;
                    screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE > 0 ) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
                    float screenDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, screenPosNorm.xy ));
                    half distanceDepth = saturate(abs( ( screenDepth - LinearEyeDepth( screenPosNorm.z ) ) / ( _DepthFade ) ));
                    mainTexAlpha = mainTexAlpha * distanceDepth;
                #endif
                
                   // return half4(1, 0, 0, 1);
                mainTexAlpha = saturate(mainTexAlpha);
                half4 finalColor = (0,0,0,0);
                finalColor = ( _Scr == 5 ? mainTexColor : ( mainTexColor * mainTexAlpha ));
                finalColor.a = mainTexAlpha;
                
                return finalColor;
                
            }
            ENDCG
		}
	}
	FallBack  Off
    CustomEditor "SampleGUI"
}
