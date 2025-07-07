// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Eff_LitA_ASE_UI"
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
607;249;1934;1101;3685.608;1098.577;1.75648;True;True
Node;AmplifyShaderEditor.RangedFloatNode;116;-2853.088,-158.974;Inherit;False;Property;_U_Speed;U_Speed;8;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-2990.019,-538.8722;Inherit;False;Property;_U_Speed_Main;U_Speed_Main;3;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-2915.92,-420.8723;Inherit;False;Property;_V_Speed_Main;V_Speed_Main;4;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-2833.454,-43.30728;Inherit;False;Property;_V_Speed;V_Speed;9;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;120;-2901.76,-307.8826;Inherit;False;Property;_MaskOffset;MaskOffset;7;0;Create;True;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;121;-2651.454,-34.30728;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;118;-2694.754,-408.9357;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;117;-2774.088,-525.6024;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;122;-2779.619,-677.5724;Inherit;False;Property;_MainOffset;MainOffset;2;0;Create;True;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;119;-2669.788,-129.9739;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;123;-2385.331,-67.83145;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;125;-2527.997,-549.4984;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;126;-2487.997,-432.4982;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;124;-2395.331,-172.8315;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;95;-2512.752,580.7154;Inherit;False;Property;_ClipRect;ClipRect;11;0;Create;True;0;0;False;0;False;0,0,0,0;-2.292994,2.292994,-4.113695,3.064013;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;77;-2581.033,385.4485;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;127;-2312.997,-482.4982;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;128;-2228.331,-153.8315;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-2482.061,765.6339;Inherit;False;Property;_Float3;Float 3;10;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;96;-2122.715,470.3984;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;79;-2123.328,253.0357;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;131;-2112.818,-589.7725;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;88;-2124.828,688.629;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;82;-2124.082,899.4482;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;132;-2054.165,-269.4652;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-1771.545,562.0699;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;136;-1821.889,-321.1935;Inherit;True;Property;_MaskTex;MaskTex;6;0;Create;True;0;0;False;0;False;136;None;3258c5adb31d7fa46a55905dfcc25d2f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;133;-1698.466,-761.2441;Inherit;False;Property;_MainColor;MainColor;0;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;2,2,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;135;-1878.363,-585.1935;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;135;None;32a2a0fff0c45634fb83d8d541ab67a1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;89;-1804.617,280.334;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;134;-1666.218,-969.7534;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-1537.086,339.0835;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;140;-1512.175,-489.3549;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-1388.218,-633.7534;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-1490.039,582.3703;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;False;0;False;3.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-1496.975,-274.6545;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;-1414.218,-792.7534;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;145;-1260.496,98.67007;Inherit;False;Constant;_Float1;Float 1;16;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;101;-1230.48,334.102;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;-1330.208,-393.7127;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;-1244.218,-643.7534;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1113.243,-530.4938;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;144;-1054.169,143.1879;Inherit;False;Property;_EnableClipRect;EnableClipRect;5;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-785.4134,-158.8746;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;-615.1847,-156.1672;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;15;Eff_LitA_ASE_UI;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;4;1;False;-1;1;False;-1;2;5;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;121;0;115;0
WireConnection;118;0;114;0
WireConnection;117;0;113;0
WireConnection;119;0;116;0
WireConnection;123;0;121;0
WireConnection;123;1;120;2
WireConnection;125;0;122;1
WireConnection;125;1;117;0
WireConnection;126;0;118;0
WireConnection;126;1;122;2
WireConnection;124;0;120;1
WireConnection;124;1;119;0
WireConnection;127;0;125;0
WireConnection;127;1;126;0
WireConnection;128;0;124;0
WireConnection;128;1;123;0
WireConnection;96;0;77;1
WireConnection;96;1;95;2
WireConnection;96;4;97;0
WireConnection;79;0;77;1
WireConnection;79;1;95;1
WireConnection;79;2;97;0
WireConnection;131;1;127;0
WireConnection;88;0;77;2
WireConnection;88;1;95;3
WireConnection;88;2;97;0
WireConnection;82;0;77;2
WireConnection;82;1;95;4
WireConnection;82;4;97;0
WireConnection;132;1;128;0
WireConnection;90;0;88;0
WireConnection;90;1;82;0
WireConnection;136;1;132;0
WireConnection;135;1;131;0
WireConnection;89;0;79;0
WireConnection;89;1;96;0
WireConnection;98;0;89;0
WireConnection;98;1;90;0
WireConnection;140;0;135;0
WireConnection;140;1;135;4
WireConnection;138;0;134;4
WireConnection;138;1;133;4
WireConnection;139;0;136;0
WireConnection;139;1;136;4
WireConnection;137;0;134;0
WireConnection;137;1;133;0
WireConnection;101;0;98;0
WireConnection;101;1;102;0
WireConnection;101;2;97;0
WireConnection;141;0;140;0
WireConnection;141;1;139;0
WireConnection;142;0;137;0
WireConnection;142;1;138;0
WireConnection;143;0;142;0
WireConnection;143;1;141;0
WireConnection;144;1;145;0
WireConnection;144;0;101;0
WireConnection;84;0;143;0
WireConnection;84;1;144;0
WireConnection;112;1;84;0
ASEEND*/
//CHKSM=956CC47FE8E9A75D1DE60B6B447AF2C99E1CF12C