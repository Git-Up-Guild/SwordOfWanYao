// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASE_Flipbook"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Tex_Columns("Tex_Columns", Float) = 1
		_Tex_Rows("Tex_Rows", Float) = 1
		_TexStartFrame("TexStartFrame", Float) = 0
		_TexSpeed("TexSpeed", Float) = 0.1
		_Tex_Time("Tex_Time", Float) = 1
		[ASEEnd]_UVSpeedControl("_UVSpeedControl", Float) = 1

	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			Name "Unlit"
			

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
			CBUFFER_START( UnityPerMaterial )
			float4 _MainColor;
			float _Tex_Columns;
			float _Tex_Rows;
			float _TexSpeed;
			float _TexStartFrame;
			float _Tex_Time;
			float _UVSpeedControl;
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

				float2 texCoord41 = IN.texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				float mulTime42 = _TimeParameters.x * _Tex_Time;
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles24 = _Tex_Columns * _Tex_Rows;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset24 = 1.0f / _Tex_Columns;
				float fbrowsoffset24 = 1.0f / _Tex_Rows;
				// Speed of animation
				float fbspeed24 = ( mulTime42 * _UVSpeedControl ) * _TexSpeed;
				// UV Tiling (col and row offset)
				float2 fbtiling24 = float2(fbcolsoffset24, fbrowsoffset24);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex24 = round( fmod( fbspeed24 + _TexStartFrame, fbtotaltiles24) );
				fbcurrenttileindex24 += ( fbcurrenttileindex24 < 0) ? fbtotaltiles24 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox24 = round ( fmod ( fbcurrenttileindex24, _Tex_Columns ) );
				// Multiply Offset X by coloffset
				float fboffsetx24 = fblinearindextox24 * fbcolsoffset24;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy24 = round( fmod( ( fbcurrenttileindex24 - fblinearindextox24 ) / _Tex_Columns, _Tex_Rows ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy24 = (int)(_Tex_Rows-1) - fblinearindextoy24;
				// Multiply Offset Y by rowoffset
				float fboffsety24 = fblinearindextoy24 * fbrowsoffset24;
				// UV Offset
				float2 fboffset24 = float2(fboffsetx24, fboffsety24);
				// Flipbook UV
				half2 fbuv24 = texCoord41 * fbtiling24 + fboffset24;
				// *** END Flipbook UV Animation vars ***
				
				float4 Color = ( _MainColor * ( IN.color * tex2D( _MainTex, fbuv24 ) ) );

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
279;263;1765;556;2379.066;239.8372;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;28;-1876.064,-66.35369;Inherit;False;Property;_Tex_Time;Tex_Time;6;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;42;-1648.795,-66.95081;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1658.266,44.94247;Inherit;False;Property;_UVSpeedControl;_UVSpeedControl;7;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1419.266,-65.05752;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1539.564,-324.6537;Inherit;False;Property;_Tex_Rows;Tex_Rows;3;0;Create;True;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;41;-1552.576,-583.6775;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-1566.564,-158.6537;Inherit;False;Property;_TexStartFrame;TexStartFrame;4;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1510.564,-244.6537;Inherit;False;Property;_TexSpeed;TexSpeed;5;0;Create;True;0;0;False;0;False;0.1;24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-1532.564,-417.6537;Inherit;False;Property;_Tex_Columns;Tex_Columns;2;0;Create;True;0;0;False;0;False;1;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;24;-1249.564,-316.6537;Inherit;True;0;0;6;0;FLOAT2;1,1;False;1;FLOAT;4;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.VertexColorNode;37;-795.9138,-457.1813;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-897.6001,-282.6;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;False;-1;None;58c76175061c0f1489814e9c21e85bce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-566.9138,-327.1813;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-816.9138,-663.1813;Inherit;False;Property;_MainColor;MainColor;0;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-382.9138,-420.1813;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-132.4,-307.9;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;15;ASE_Flipbook;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;42;0;28;0
WireConnection;35;0;42;0
WireConnection;35;1;34;0
WireConnection;24;0;41;0
WireConnection;24;1;32;0
WireConnection;24;2;33;0
WireConnection;24;3;30;0
WireConnection;24;4;29;0
WireConnection;24;5;35;0
WireConnection;1;1;24;0
WireConnection;38;0;37;0
WireConnection;38;1;1;0
WireConnection;40;0;39;0
WireConnection;40;1;38;0
WireConnection;0;1;40;0
ASEEND*/
//CHKSM=60424170EF6288FC07C0E5671C462F85A8927361