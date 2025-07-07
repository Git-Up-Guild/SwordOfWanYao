


Shader "AB_VFX/distort_blend" {
	Properties {
		
		_Brightness("Brightness",float) = 1
  		_Contrast ("Contrast", Float ) = 1
		_MainColor ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainPannerX  ("Main Panner X", float) = 0
		_MainPannerY  ("Main Panner Y", float) = 0
		[Space(15)]
		_MaskTex("Mask Texture", 2D) = "white" {}
		[Space(15)]
		_TurbulenceTex ("Turbulence Texture", 2D) = "white" {}
		_DistortPower  ("Distort Power", float) = 0
		_PowerX  ("Power X", range (0,1)) = 0
		_PowerY  ("Power Y", range (0,1)) = 0

	}

	Category
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off

		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					half2 texcoord: TEXCOORD0;
					float4 vertexColor : COLOR;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 vertexColor : COLOR;
					half2 uvmain : TEXCOORD1;
					half2 uvnoise : TEXCOORD2;
					half2 uvMask : TEXCOORD3;
				};

				float _Brightness;
				float _Contrast;
				fixed4 _MainColor;
				fixed _PowerX;
				fixed _PowerY;
				fixed _DistortPower;
				half4 _MainTex_ST;
				half4 _TurbulenceTex_ST;
				half4 _MaskTex_ST;
				float _Type;
				float _MainPannerX;
      			float _MainPannerY;

				sampler2D _TurbulenceTex;
				sampler2D _MainTex;
				sampler2D _MaskTex;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.vertexColor = v.vertexColor;
					o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
					o.uvnoise = TRANSFORM_TEX( v.texcoord, _TurbulenceTex);
					o.uvMask = TRANSFORM_TEX(v.texcoord, _MaskTex);
					return o;
				}

				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 offsetColor1 = tex2D(_TurbulenceTex, i.uvnoise + fmod(_Time.xz*_DistortPower,1));
					fixed4 offsetColor2 = tex2D(_TurbulenceTex, i.uvnoise + fmod(_Time.yx*_DistortPower,1));

					fixed4 mask = tex2D(_MaskTex, i.uvMask);

					half2 oldUV = i.uvmain;

					i.uvmain.x += ((offsetColor1.r + offsetColor2.r) - 1) * _PowerX;
					i.uvmain.y += ((offsetColor1.r + offsetColor2.r) - 1) * _PowerY;

					half2 newUV = i.uvmain;
					half2 resUV = lerp(oldUV,newUV,mask.xy);
					resUV.x += fmod(_MainPannerX*_Time.y,1);
					resUV.y += fmod(_MainPannerY*_Time.y,1);

					fixed4 _MainTex_var = tex2D(_MainTex,resUV);
					half2 maskUV = i.uvMask;
					fixed4 _MaskTex_var = tex2D(_MaskTex,maskUV);
					fixed3 emissive = _MainColor.rgb*_Brightness*pow(_MainTex_var.rgb,_Contrast)*i.vertexColor.rgb;
					fixed finalAlpha = _MainColor.a*_MainTex_var.a*i.vertexColor.a*_MaskTex_var.r;
					return fixed4(emissive,finalAlpha);
				}
				ENDCG
			}
		}
	}
}
