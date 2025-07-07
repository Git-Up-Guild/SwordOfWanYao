// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Eff_LitB_ASE_UI"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin][HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_MainOffset("MainOffset", Vector) = (1,1,0,0)
		_U_Speed_Main("U_Speed_Main", Float) = 0
		_V_Speed_Main("V_Speed_Main", Float) = 0
		[Toggle(_ENABLECLIPRECT_ON)] _EnableClipRect("EnableClipRect", Float) = 0
		_MaskTex("MaskTex", 2D) = "white" {}
		_MaskOffset("MaskOffset", Vector) = (1,1,0,0)
		_U_Speed("U_Speed", Float) = 0
		_V_Speed("V_Speed", Float) = 0
		_Float3("Float 3", Float) = 1
		[ASEEnd]_ClipRect("ClipRect", Vector) = (0,0,0,0)
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
			

			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
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
			#pragma shader_feature_local _ENABLECLIPRECT_ON


			sampler2D _MainTex;
			SAMPLER(sampler_MainTex);
			sampler2D _MaskTex;
			SAMPLER(sampler_MaskTex);
			CBUFFER_START( UnityPerMaterial )
			float4 _MainColor;
			float4 _ClipRect;
			float2 _MainOffset;
			float2 _MaskOffset;
			float _U_Speed_Main;
			float _V_Speed_Main;
			float _U_Speed;
			float _V_Speed;
			float _Float3;
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
				float4 ase_texcoord2 : TEXCOORD2;
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

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				o.ase_texcoord2.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
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

				float mulTime117 = _TimeParameters.x * _U_Speed_Main;
				float2 appendResult125 = (float2(_MainOffset.x , mulTime117));
				float mulTime118 = _TimeParameters.x * _V_Speed_Main;
				float2 appendResult126 = (float2(mulTime118 , _MainOffset.y));
				float2 texCoord131 = IN.texCoord0.xy * float2( 1,1 ) + ( appendResult125 + appendResult126 );
				float4 tex2DNode135 = tex2D( _MainTex, texCoord131 );
				float mulTime119 = _TimeParameters.x * _U_Speed;
				float2 appendResult124 = (float2(_MaskOffset.x , mulTime119));
				float mulTime121 = _TimeParameters.x * _V_Speed;
				float2 appendResult123 = (float2(mulTime121 , _MaskOffset.y));
				float2 texCoord132 = IN.texCoord0.xy * float2( 1,1 ) + ( appendResult124 + appendResult123 );
				float4 tex2DNode136 = tex2D( _MaskTex, texCoord132 );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float ifLocalVar79 = 0;
				if( ase_worldPos.x > _ClipRect.x )
				ifLocalVar79 = _Float3;
				float ifLocalVar96 = 0;
				if( ase_worldPos.x < _ClipRect.y )
				ifLocalVar96 = _Float3;
				float ifLocalVar88 = 0;
				if( ase_worldPos.y > _ClipRect.z )
				ifLocalVar88 = _Float3;
				float ifLocalVar82 = 0;
				if( ase_worldPos.y < _ClipRect.w )
				ifLocalVar82 = _Float3;
				#ifdef _ENABLECLIPRECT_ON
				float staticSwitch144 = ( ( ( ifLocalVar79 + ifLocalVar96 ) + ( ifLocalVar88 + ifLocalVar82 ) ) > 3.8 ? _Float3 : 0.0 );
				#else
				float staticSwitch144 = 1.0;
				#endif
				
				float4 Color = ( ( ( ( IN.color * _MainColor ) * ( IN.color.a * _MainColor.a ) ) * ( ( tex2DNode135 * tex2DNode135.a ) * ( tex2DNode136 * tex2DNode136.a ) ) ) * staticSwitch144 );

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
690;215;1566;1032;2431.376;896.3082;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;97;-2673.668,336.494;Inherit;False;Property;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;95;-2718.36,149.5756;Inherit;False;Property;_ClipRect;ClipRect;4;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;77;-2772.641,-43.69137;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ConditionalIfNode;79;-2314.936,-176.1043;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;82;-2315.689,470.3083;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;88;-2316.436,259.4891;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;96;-2314.323,41.25851;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;164;-2047.382,-418.9773;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;160;None;c5aeb86c148be094fbd311ce9ed8fe28;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;165;-1940.993,-628.8172;Inherit;False;Property;_MainColor;MainColor;0;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-1963.153,132.9299;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;89;-1996.225,-148.8058;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-1681.646,153.2305;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;False;0;False;3.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;-1619.871,-552.5709;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-1728.693,-90.05635;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;101;-1277.835,-35.77847;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;181;-1262.327,-124.5178;Inherit;False;Constant;_Float1;Float 1;16;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;-1429.348,-502.8203;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;160;-1480.336,-815.8565;Inherit;True;Property;_MaskTex;MaskTex;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;-1103.564,-541.741;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;180;-1056,-80;Inherit;False;Property;_EnableClipRect;EnableClipRect;5;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-783.1307,-240.392;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;-617.5964,-254.0815;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;15;Eff_LitB_ASE_UI;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;79;0;77;1
WireConnection;79;1;95;1
WireConnection;79;2;97;0
WireConnection;82;0;77;2
WireConnection;82;1;95;4
WireConnection;82;4;97;0
WireConnection;88;0;77;2
WireConnection;88;1;95;3
WireConnection;88;2;97;0
WireConnection;96;0;77;1
WireConnection;96;1;95;2
WireConnection;96;4;97;0
WireConnection;90;0;88;0
WireConnection;90;1;82;0
WireConnection;89;0;79;0
WireConnection;89;1;96;0
WireConnection;173;0;165;0
WireConnection;173;1;164;0
WireConnection;98;0;89;0
WireConnection;98;1;90;0
WireConnection;101;0;98;0
WireConnection;101;1;102;0
WireConnection;101;2;97;0
WireConnection;174;0;173;0
WireConnection;174;1;164;4
WireConnection;182;0;160;4
WireConnection;182;1;174;0
WireConnection;180;1;181;0
WireConnection;180;0;101;0
WireConnection;84;0;182;0
WireConnection;84;1;180;0
WireConnection;112;1;84;0
ASEEND*/
//CHKSM=0CC77663D57C50510337E2226C45D598FD885616