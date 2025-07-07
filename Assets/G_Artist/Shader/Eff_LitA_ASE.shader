// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Eff_LitA_ASE"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin][HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_MainTiling("MainTiling", Vector) = (1,1,0,0)
		_MainOffset("MainOffset", Vector) = (1,1,0,0)
		_U_Speed_Main("U_Speed_Main", Float) = 0
		_V_Speed_Main("V_Speed_Main", Float) = 0
		_MaskTex("MaskTex", 2D) = "white" {}
		_MaskTiling("MaskTiling", Vector) = (1,1,0,0)
		_MaskOffset("MaskOffset", Vector) = (1,1,0,0)
		_U_Speed("U_Speed", Float) = 0
		[ASEEnd]_V_Speed("V_Speed", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2  //声明外部控制开关

	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull [_Cull]
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			Name "Unlit"
			

			Blend One One, SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define ASE_SRP_VERSION 70403

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS_SPRITEUNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#define ASE_NEEDS_FRAG_COLOR


			sampler2D _MainTex;
			SAMPLER(sampler_MainTex);
			sampler2D _MaskTex;
			SAMPLER(sampler_MaskTex);
			CBUFFER_START( UnityPerMaterial )
			float4 _MainColor;
			float2 _MainTiling;
			float2 _MainOffset;
			float2 _MaskTiling;
			float2 _MaskOffset;
			float _U_Speed_Main;
			float _V_Speed_Main;
			float _U_Speed;
			float _V_Speed;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.normal = v.normal;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.vertex.xyz );

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.clipPos = vertexInput.positionCS;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float mulTime58 = _TimeParameters.x * _U_Speed_Main;
				float2 appendResult72 = (float2(_MainOffset.x , mulTime58));
				float mulTime59 = _TimeParameters.x * _V_Speed_Main;
				float2 appendResult73 = (float2(mulTime59 , _MainOffset.y));
				float2 texCoord57 = IN.texCoord0.xy * _MainTiling + ( appendResult72 + appendResult73 );
				float4 tex2DNode45 = tex2D( _MainTex, texCoord57 );
				float mulTime38 = _TimeParameters.x * _U_Speed;
				float2 appendResult67 = (float2(_MaskOffset.x , mulTime38));
				float mulTime39 = _TimeParameters.x * _V_Speed;
				float2 appendResult70 = (float2(mulTime39 , _MaskOffset.y));
				float2 texCoord44 = IN.texCoord0.xy * _MaskTiling + ( appendResult67 + appendResult70 );
				float4 tex2DNode46 = tex2D( _MaskTex, texCoord44 );
				
				float4 Color = ( ( ( IN.color * _MainColor ) * ( IN.color.a * _MainColor.a ) ) * ( ( tex2DNode45 * tex2DNode45.a ) * ( tex2DNode46 * tex2DNode46.a ) ) );

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif

				Color *= IN.color;

				return Color;
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18500
-1920;0;1920;1019;2479.353;961.7;1;False;True
Node;AmplifyShaderEditor.RangedFloatNode;65;-3130.608,-629.312;Inherit;False;Property;_U_Speed_Main;U_Speed_Main;4;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-3030.509,-535.3121;Inherit;False;Property;_V_Speed_Main;V_Speed_Main;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-2736.043,-127.747;Inherit;False;Property;_V_Speed;V_Speed;10;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2788.677,-237.4137;Inherit;False;Property;_U_Speed;U_Speed;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;58;-2951.677,-633.0422;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;59;-2782.343,-512.3755;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;38;-2642.377,-214.4137;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;37;-2797.349,-388.3224;Inherit;False;Property;_MaskOffset;MaskOffset;8;0;Create;True;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;39;-2559.043,-103.747;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;64;-2752.208,-762.012;Inherit;False;Property;_MainOffset;MainOffset;3;0;Create;True;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;70;-2357.92,-152.2712;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;67;-2367.92,-257.2712;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;-2500.586,-633.9381;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;73;-2460.586,-516.938;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;-2285.586,-566.938;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-2200.92,-238.2712;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;42;-2231.628,-411.7673;Inherit;False;Property;_MaskTiling;MaskTiling;7;0;Create;True;0;0;False;0;False;1,1;0.5,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;63;-2323.614,-758.7109;Inherit;False;Property;_MainTiling;MainTiling;2;0;Create;True;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;57;-2085.407,-674.2122;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;44;-2026.754,-353.905;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;52;-1671.055,-845.6837;Inherit;False;Property;_MainColor;MainColor;0;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;128,128,128,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;121;-1638.807,-1054.193;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-1850.952,-669.6332;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;45;None;87b66ed01d0a21f47ac1292bdc4d68b3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;46;-1794.478,-405.6332;Inherit;True;Property;_MaskTex;MaskTex;6;0;Create;True;0;0;False;0;False;46;None;a44a6aae89318a047bac10294d837082;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;-1386.807,-877.1931;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-1360.807,-718.1931;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-1469.564,-359.0943;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;-1484.764,-573.7946;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-1302.797,-478.1524;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-1216.807,-728.1931;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1085.832,-614.9335;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;118;-912.467,-609.3563;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;15;Eff_LitA_ASE;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;4;1;False;-1;1;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;58;0;65;0
WireConnection;59;0;66;0
WireConnection;38;0;36;0
WireConnection;39;0;35;0
WireConnection;70;0;39;0
WireConnection;70;1;37;2
WireConnection;67;0;37;1
WireConnection;67;1;38;0
WireConnection;72;0;64;1
WireConnection;72;1;58;0
WireConnection;73;0;59;0
WireConnection;73;1;64;2
WireConnection;74;0;72;0
WireConnection;74;1;73;0
WireConnection;71;0;67;0
WireConnection;71;1;70;0
WireConnection;57;0;63;0
WireConnection;57;1;74;0
WireConnection;44;0;42;0
WireConnection;44;1;71;0
WireConnection;45;1;57;0
WireConnection;46;1;44;0
WireConnection;122;0;121;0
WireConnection;122;1;52;0
WireConnection;123;0;121;4
WireConnection;123;1;52;4
WireConnection;120;0;46;0
WireConnection;120;1;46;4
WireConnection;119;0;45;0
WireConnection;119;1;45;4
WireConnection;104;0;119;0
WireConnection;104;1;120;0
WireConnection;124;0;122;0
WireConnection;124;1;123;0
WireConnection;55;0;124;0
WireConnection;55;1;104;0
WireConnection;118;1;55;0
ASEEND*/
//CHKSM=478D9B01986549A2B570D8B0F63B37B056ED8AC6