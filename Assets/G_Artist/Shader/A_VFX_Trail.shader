// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "A_VFX/ProjectilesFX/Trail"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Int) = 10
		_EmissiveMultiply("Emissive Multiply", Float) = 1
		_OpacityMultiply("Opacity Multiply", Float) = 1
		_MainTiling("Main Tiling", Vector) = (1,1,1,1)
		_MainTexturePower("Main Texture Power", Float) = 1
		[KeywordEnum(None,Add,Lerp)] _Blend("Blend", Float) = 0
		_TimeScale1("Time Scale 1", Float) = 1
		_TimeScale2("Time Scale 2", Float) = 1
		[Toggle]_UseTextureMaskAlpha("Use Texture Mask Alpha", Float) = 1
		_TextureMaskAlpha("Texture Mask Alpha", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend [_SrcBlend] [_DstBlend]
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma multi_compile_local _BLEND_NONE _BLEND_ADD _BLEND_LERP


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform int _DstBlend;
				uniform int _SrcBlend;
				uniform float _EmissiveMultiply;
				uniform float _TimeScale1;
				uniform float4 _MainTiling;
				uniform float _MainTexturePower;
				uniform float _TimeScale2;
				uniform float _UseTextureMaskAlpha;
				uniform sampler2D _TextureMaskAlpha;
				uniform float4 _TextureMaskAlpha_ST;
				uniform float _OpacityMultiply;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float mulTime57 = _Time.y * _TimeScale1;
					float2 texCoord122 = i.texcoord.xy * (_MainTiling).xy + float2( 0,0 );
					float2 panner66 = ( mulTime57 * float2( -1,0 ) + texCoord122);
					float4 temp_cast_0 = (_MainTexturePower).xxxx;
					float4 temp_output_244_0 = pow( tex2D( _MainTex, panner66 ) , temp_cast_0 );
					float mulTime257 = _Time.y * _TimeScale2;
					float2 texCoord252 = i.texcoord.xy * (_MainTiling).zw + float2( 0,0 );
					float2 panner255 = ( mulTime257 * float2( -1,0 ) + texCoord252);
					float4 temp_cast_1 = (_MainTexturePower).xxxx;
					float4 temp_output_266_0 = pow( tex2D( _MainTex, panner255 ) , temp_cast_1 );
					float4 lerpResult304 = lerp( temp_output_244_0 , temp_output_266_0 , 0.5);
					#if defined(_BLEND_NONE)
					float4 staticSwitch263 = temp_output_244_0;
					#elif defined(_BLEND_ADD)
					float4 staticSwitch263 = ( temp_output_244_0 + temp_output_266_0 );
					#elif defined(_BLEND_LERP)
					float4 staticSwitch263 = lerpResult304;
					#else
					float4 staticSwitch263 = temp_output_244_0;
					#endif
					float2 uv_TextureMaskAlpha = i.texcoord.xy * _TextureMaskAlpha_ST.xy + _TextureMaskAlpha_ST.zw;
					float4 temp_output_86_0 = ( staticSwitch263 * i.color * _TintColor * (( _UseTextureMaskAlpha )?( tex2D( _TextureMaskAlpha, uv_TextureMaskAlpha ).r ):( 1.0 )) );
					float4 appendResult187 = (float4(( _EmissiveMultiply * (temp_output_86_0).rgb ) , saturate( ( (temp_output_86_0).a * _OpacityMultiply ) )));
					

					fixed4 col = appendResult187;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;318;554.994,-46.65884;Inherit;False;222.385;257.1656;Custom Blending;2;320;319;;1,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;2617.409,515.2094;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;87;2370.874,540.2277;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;200;2388.997,712.5887;Inherit;False;0;0;_TintColor;Shader;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;217;2292.309,888.3658;Inherit;False;Property;_UseTextureMaskAlpha;Use Texture Mask Alpha;9;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;218;2138.362,888.037;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;213;1980.107,973.518;Inherit;True;Property;_TextureMaskAlpha;Texture Mask Alpha;10;0;Create;True;0;0;0;False;0;False;-1;None;990426196cc6d264784eac08d855a648;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;263;2354.49,352.8054;Inherit;False;Property;_Blend;Blend;6;0;Create;True;0;0;0;False;0;False;1;0;0;True;;KeywordEnum;3;None;Add;Lerp;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;322;2546.008,497.0154;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;304;2125.573,406.099;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;261;2153.72,299.3553;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;244;1917.576,256.2718;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;266;1918.502,367.3341;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;305;1920.274,482.5576;Inherit;False;Constant;_Float2;Float 2;12;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;1643.63,650.9747;Inherit;False;Property;_MainTexturePower;Main Texture Power;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;253;1590.103,250.9433;Inherit;True;Property;_TextureSample0;Texture Sample 0;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;254;1591.142,440.3817;Inherit;True;Property;_TextureSample1;Texture Sample 1;19;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;255;1361.86,429.2816;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;189;1321.429,611.2278;Inherit;True;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;66;1359.694,280.0378;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.IntNode;320;604.994,95.10673;Inherit;False;Property;_DstBlend;DstBlend;1;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;1;False;0;1;INT;0
Node;AmplifyShaderEditor.IntNode;319;606.5789,5.341187;Inherit;False;Property;_SrcBlend;SrcBlend;0;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;969.071,624.3716;Inherit;False;Property;_TimeScale1;Time Scale 1;7;0;Create;True;0;0;0;False;0;False;1;1.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;256;967.2536,715.3779;Inherit;False;Property;_TimeScale2;Time Scale 2;8;0;Create;True;0;0;0;False;0;False;1;2.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;257;1134.138,720.6058;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;57;1133.955,629.6004;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;122;1091.202,279.7147;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;252;1092.804,429.0412;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;221;872.8622,299.0078;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;222;874.8622,448.0078;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;220;691.0123,298.8525;Inherit;False;Property;_MainTiling;Main Tiling;4;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.6,1,0.8,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;107;2865.3,602.4728;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;2875.172,689.7707;Inherit;False;Property;_OpacityMultiply;Opacity Multiply;3;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;108;2865.108,511.6703;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;127;2866.228,425.395;Inherit;False;Property;_EmissiveMultiply;Emissive Multiply;2;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;3070.249,670.6725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;3206.344,494.8725;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;187;3350.719,495.0323;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;317;3502.705,495.8432;Float;False;True;-1;2;ASEMaterialInspector;0;11;A_VFX/ProjectilesFX/Trail;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;True;_SrcBlend;10;True;_DstBlend;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.SaturateNode;117;3205.493,670.5457;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
WireConnection;86;0;322;0
WireConnection;86;1;87;0
WireConnection;86;2;200;0
WireConnection;86;3;217;0
WireConnection;217;0;218;0
WireConnection;217;1;213;1
WireConnection;263;1;244;0
WireConnection;263;0;261;0
WireConnection;263;2;304;0
WireConnection;322;0;263;0
WireConnection;304;0;244;0
WireConnection;304;1;266;0
WireConnection;304;2;305;0
WireConnection;261;0;244;0
WireConnection;261;1;266;0
WireConnection;244;0;253;0
WireConnection;244;1;245;0
WireConnection;266;0;254;0
WireConnection;266;1;245;0
WireConnection;253;0;189;0
WireConnection;253;1;66;0
WireConnection;254;0;189;0
WireConnection;254;1;255;0
WireConnection;255;0;252;0
WireConnection;255;1;257;0
WireConnection;66;0;122;0
WireConnection;66;1;57;0
WireConnection;257;0;256;0
WireConnection;57;0;55;0
WireConnection;122;0;221;0
WireConnection;252;0;222;0
WireConnection;221;0;220;0
WireConnection;222;0;220;0
WireConnection;107;0;86;0
WireConnection;108;0;86;0
WireConnection;248;0;107;0
WireConnection;248;1;249;0
WireConnection;310;0;127;0
WireConnection;310;1;108;0
WireConnection;187;0;310;0
WireConnection;187;3;117;0
WireConnection;317;0;187;0
WireConnection;117;0;248;0
ASEEND*/
//CHKSM=06A1AE434487D03043A3B709F6112D5AA21EBA2E