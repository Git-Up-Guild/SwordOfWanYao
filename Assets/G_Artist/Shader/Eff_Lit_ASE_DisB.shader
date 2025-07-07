// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Eff_Lit_ASE_DisB"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_MainTex("MainTex", 2D) = "white" {}
		_Float3("Float 3", Float) = 1
		_ClipRect("ClipRect", Vector) = (0,0,0,0)
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_NoisesUSpeed("NoisesUSpeed", Float) = 0.1
		_NoisesVSpeed("NoisesVSpeed", Float) = 0.1
		_NoisePower("NoisePower", Range( 0 , 0.1)) = 0.05
		_UVPower("UVPower", Vector) = (0.03,0,-0.03,0)
		[HDR]_DissolveColor("DissolveColor", Color) = (1,1,1,1)
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_DissolveEdge("DissolveEdge", Float) = 0.05
		_DissolvePower("DissolvePower", Range( -0.1 , 1.1)) = 1
		_UVSpeedControl("UVSpeedControl", Float) = 1
		[ASEEnd][Toggle(_ENABLECLIPRECT_ON)] _EnableClipRect("EnableClipRect", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		
		Pass
		{
			Name "Sprite Lit"
			Tags { "LightMode"="Universal2D" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One Zero
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			Stencil
			{
				Ref 0
				Comp LEqual
				Pass Keep
				Fail Keep
				ZFail Keep
			}

			HLSLPROGRAM
			#define ASE_SRP_VERSION 70403

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
			#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
			#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
			#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS_SPRITELIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
			
			#if USE_SHAPE_LIGHT_TYPE_0
			SHAPE_LIGHT(0)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_1
			SHAPE_LIGHT(1)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_2
			SHAPE_LIGHT(2)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_3
			SHAPE_LIGHT(3)
			#endif

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

			#pragma shader_feature_local _ENABLECLIPRECT_ON


			sampler2D _MainTex;
			sampler2D _NoiseTex;
			SAMPLER(sampler_NoiseTex);
			sampler2D _DissolveTex;
			SAMPLER(sampler_DissolveTex);
			CBUFFER_START( UnityPerMaterial )
			float4 _NoiseTex_ST;
			float4 _UVPower;
			float4 _DissolveTex_ST;
			float4 _DissolveColor;
			float4 _ClipRect;
			float _NoisesUSpeed;
			float _NoisesVSpeed;
			float _NoisePower;
			float _DissolvePower;
			float _DissolveEdge;
			float _UVSpeedControl;
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
				float4 screenPosition : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D(_AlphaTex); SAMPLER(sampler_AlphaTex);
				float _EnableAlphaTexture;
			#endif

			
			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				o.ase_texcoord3.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.normal = v.normal;
				v.tangent.xyz = v.tangent.xyz;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.clipPos = vertexInput.positionCS;
				o.screenPosition = ComputeScreenPos( o.clipPos, _ProjectionParams.x );
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 texCoord125 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult85 = (float2(_NoisesUSpeed , _NoisesVSpeed));
				float2 uv_NoiseTex = IN.texCoord0.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 panner84 = ( 1.0 * _Time.y * appendResult85 + uv_NoiseTex);
				float2 appendResult127 = (float2(( texCoord125.x + ( tex2D( _NoiseTex, panner84 ).r * _NoisePower ) ) , texCoord125.y));
				float4 break134 = ( (0.0 + (_NoisePower - 0.0) * (1.0 - 0.0) / (0.1 - 0.0)) * _UVPower );
				float2 appendResult136 = (float2(break134.x , break134.y));
				float4 tex2DNode122 = tex2D( _MainTex, appendResult127 );
				float2 appendResult137 = (float2(break134.z , break134.w));
				float4 appendResult140 = (float4(tex2D( _MainTex, ( appendResult127 + appendResult136 ) ).r , tex2DNode122.g , tex2D( _MainTex, ( appendResult127 + appendResult137 ) ).b , tex2DNode122.a));
				float2 uv_DissolveTex = IN.texCoord0.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float4 tex2DNode97 = tex2D( _DissolveTex, uv_DissolveTex );
				float temp_output_100_0 = step( tex2DNode97.r , ( _DissolvePower - _DissolveEdge ) );
				float3 ase_worldPos = IN.ase_texcoord3.xyz;
				float ifLocalVar60 = 0;
				if( ase_worldPos.x > _ClipRect.x )
				ifLocalVar60 = _Float3;
				float ifLocalVar63 = 0;
				if( ase_worldPos.x < _ClipRect.y )
				ifLocalVar63 = _Float3;
				float ifLocalVar62 = 0;
				if( ase_worldPos.y > _ClipRect.z )
				ifLocalVar62 = _Float3;
				float ifLocalVar61 = 0;
				if( ase_worldPos.y < _ClipRect.w )
				ifLocalVar61 = _Float3;
				#ifdef _ENABLECLIPRECT_ON
				float staticSwitch70 = ( ( ( ifLocalVar60 + ifLocalVar63 ) + ( ifLocalVar62 + ifLocalVar61 ) ) > 3.8 ? _Float3 : 0.0 );
				#else
				float staticSwitch70 = 1.0;
				#endif
				
				float4 Color = ( ( ( appendResult140 * temp_output_100_0 ) + ( _DissolveColor * ( step( tex2DNode97.r , ( _UVSpeedControl * _DissolvePower ) ) - temp_output_100_0 ) ) ) * staticSwitch70 );
				float Mask = 1;
				float3 Normal = float3( 0, 0, 1 );

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, IN.texCoord0.xy);
					Color.a = lerp ( Color.a, alpha.r, _EnableAlphaTexture);
				#endif
				
				Color *= IN.color;

				return CombinedShapeLightShared( Color, Mask, IN.screenPosition.xy / IN.screenPosition.w );
			}

			ENDHLSL
		}

	
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18500
607;249;1934;1101;5982.53;2478.444;3.688067;True;True
Node;AmplifyShaderEditor.RangedFloatNode;86;-5274.191,-1331.63;Inherit;False;Property;_NoisesUSpeed;NoisesUSpeed;4;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-5252.507,-1189.755;Inherit;False;Property;_NoisesVSpeed;NoisesVSpeed;5;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;83;-5030.374,-1522.343;Inherit;False;0;78;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;85;-4955.563,-1282.351;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;84;-4754.712,-1398.866;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-4995.75,-950.7071;Inherit;False;Property;_NoisePower;NoisePower;6;0;Create;True;0;0;False;0;False;0.05;0.01;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;132;-4566.297,-627.7468;Inherit;False;Property;_UVPower;UVPower;7;0;Create;True;0;0;False;0;False;0.03,0,-0.03,0;0.03,0,-0.03,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;78;-4497.899,-1382.324;Inherit;True;Property;_NoiseTex;NoiseTex;3;0;Create;True;0;0;False;0;False;-1;28c7aad1372ff114b90d330f8a2dd938;3258c5adb31d7fa46a55905dfcc25d2f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;130;-4598.67,-877.72;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-4045.171,-1313.172;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;125;-4087.44,-1547.463;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-4298.308,-795.7112;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;134;-4115.943,-857.124;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-3637.994,-1463.038;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-3269.429,69.0386;Inherit;False;Property;_DissolveEdge;DissolveEdge;10;0;Create;True;0;0;False;0;False;0.05;0.003;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;59;-3657.905,414.0735;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;127;-3384.301,-1220.442;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;137;-3857.01,-756.1045;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;163;-3831.032,-55.41943;Inherit;False;Property;_DissolvePower;DissolvePower;11;0;Create;True;0;0;False;0;False;1;1;-0.1;1.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;136;-3855.446,-855.0242;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-3566.298,914.5914;Inherit;False;Property;_Float3;Float 3;1;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;58;-3655.194,673.6461;Inherit;False;Property;_ClipRect;ClipRect;2;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;94;-3510.43,-261.9614;Inherit;False;Property;_UVSpeedControl;UVSpeedControl;12;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;63;-3096.443,634.0903;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;61;-3097.809,1063.14;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;97;-3045.429,-362.9614;Inherit;True;Property;_DissolveTex;DissolveTex;9;0;Create;True;0;0;False;0;False;-1;None;e402653ac65716748a85ec6c1f46ef56;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;62;-3098.556,852.321;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;60;-3097.056,416.7275;Inherit;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-3173.429,-154.9614;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;123;-3372.072,-1722.899;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;None;9d36ed88a04b7294e86c9d4af2bf5560;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;139;-3142.405,-938.1405;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;-3136.728,-1178.437;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;98;-2997.429,5.038534;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-2778.344,444.0261;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-2745.272,725.7617;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;122;-2871.069,-1446.045;Inherit;True;Property;_TextureSample2;Texture Sample 2;11;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;100;-2597.428,53.03854;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;121;-2903.444,-1688.047;Inherit;True;Property;_TextureSample0;Texture Sample 0;10;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;101;-2645.428,-170.9614;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;124;-2785.517,-1201.281;Inherit;True;Property;_TextureSample3;Texture Sample 3;11;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;90;-2225.231,57.70789;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2510.812,502.7755;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;89;-2230.897,-127.9141;Inherit;False;Property;_DissolveColor;DissolveColor;8;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;0.687696,1.885618,2.828427,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;140;-2391.678,-1415.517;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-2450.01,769.8251;Inherit;False;Constant;_Float0;Float 0;12;0;Create;True;0;0;False;0;False;3.8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-1980.848,-441.9568;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2063.206,465.8127;Inherit;False;Constant;_Float1;Float 1;16;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-1973.256,-99.51327;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;68;-2059.954,557.0533;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;93;-1681.643,-310.2791;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;70;-1838.119,512.8318;Inherit;False;Property;_EnableClipRect;EnableClipRect;13;0;Create;True;0;0;False;0;False;0;0;0;True;EnableClipRect;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-890.4113,-129.9809;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;14;New Amplify Shader;199187dac283dbe4a8cb1ea611d70c58;True;Sprite Forward;0;2;Sprite Forward;0;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;True;0;False;-1;255;False;-1;255;False;-1;3;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;14;New Amplify Shader;199187dac283dbe4a8cb1ea611d70c58;True;Sprite Normal;0;1;Sprite Normal;0;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;True;0;False;-1;255;False;-1;255;False;-1;3;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=NormalsRendering;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-466.3565,-51.31411;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;14;Eff_Lit_ASE_DisB;199187dac283dbe4a8cb1ea611d70c58;True;Sprite Lit;0;0;Sprite Lit;6;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;True;2;5;False;-1;10;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;True;0;False;-1;255;False;-1;255;False;-1;4;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;3;True;False;False;False;;False;0
WireConnection;85;0;86;0
WireConnection;85;1;87;0
WireConnection;84;0;83;0
WireConnection;84;2;85;0
WireConnection;78;1;84;0
WireConnection;130;0;129;0
WireConnection;128;0;78;1
WireConnection;128;1;129;0
WireConnection;131;0;130;0
WireConnection;131;1;132;0
WireConnection;134;0;131;0
WireConnection;126;0;125;1
WireConnection;126;1;128;0
WireConnection;127;0;126;0
WireConnection;127;1;125;2
WireConnection;137;0;134;2
WireConnection;137;1;134;3
WireConnection;136;0;134;0
WireConnection;136;1;134;1
WireConnection;63;0;59;1
WireConnection;63;1;58;2
WireConnection;63;4;57;0
WireConnection;61;0;59;2
WireConnection;61;1;58;4
WireConnection;61;4;57;0
WireConnection;62;0;59;2
WireConnection;62;1;58;3
WireConnection;62;2;57;0
WireConnection;60;0;59;1
WireConnection;60;1;58;1
WireConnection;60;2;57;0
WireConnection;99;0;94;0
WireConnection;99;1;163;0
WireConnection;139;0;127;0
WireConnection;139;1;137;0
WireConnection;138;0;127;0
WireConnection;138;1;136;0
WireConnection;98;0;163;0
WireConnection;98;1;95;0
WireConnection;65;0;60;0
WireConnection;65;1;63;0
WireConnection;64;0;62;0
WireConnection;64;1;61;0
WireConnection;122;0;123;0
WireConnection;122;1;127;0
WireConnection;100;0;97;1
WireConnection;100;1;98;0
WireConnection;121;0;123;0
WireConnection;121;1;138;0
WireConnection;101;0;97;1
WireConnection;101;1;99;0
WireConnection;124;0;123;0
WireConnection;124;1;139;0
WireConnection;90;0;101;0
WireConnection;90;1;100;0
WireConnection;67;0;65;0
WireConnection;67;1;64;0
WireConnection;140;0;121;1
WireConnection;140;1;122;2
WireConnection;140;2;124;3
WireConnection;140;3;122;4
WireConnection;91;0;140;0
WireConnection;91;1;100;0
WireConnection;92;0;89;0
WireConnection;92;1;90;0
WireConnection;68;0;67;0
WireConnection;68;1;66;0
WireConnection;68;2;57;0
WireConnection;93;0;91;0
WireConnection;93;1;92;0
WireConnection;70;1;69;0
WireConnection;70;0;68;0
WireConnection;71;0;93;0
WireConnection;71;1;70;0
WireConnection;0;1;71;0
ASEEND*/
//CHKSM=F1EBBEA003295706C8526B3038EFD75A7581EEE9