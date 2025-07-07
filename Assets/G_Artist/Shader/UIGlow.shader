Shader "XGame/UI/Glow"
{
	Properties
	{
		//2D描边
		[PerRendererData] _MainTex("Main Texture", 2D) = "white"{}											//主纹理
		_Color("Tint", Color) = (1,1,1,1)

		//[Toggle(SHOW_OUTLINE)] _ShowOutline("Show Outline", Int) = 0										//开启外轮廓的Toggle
		[MaterialToggle] _ShowOutline("Show Outline", Int) = 0										//开启外轮廓的Toggle
		_EdgeWidth("Edge Width", Range(0, 30)) = 1																			//边界的宽度
		_EdgeAlphaThreshold("Edge Alpha Threshold", Range(0, 1)) = 1.0									//边界透明度的阈值
		_EdgeColor("Edge Color", Color) = (0,0,0,1)																			//边界的颜色
		_EdgeFadeStrength("EdgeFadeStrength", Range(0, 2)) = 1													//渐变的强化值
		_OriginAlphaThreshold("OriginAlphaThreshold", range(0, 1)) = 0.2									//原像素剔除的阈值
		_UEdgeFade("U Fade Edge", Range(0.001, 0.5)) = 0.015
		_VEdgeFade("V Fade Edge", Range(0.001, 0.5)) = 0.015
		_UIRenderRect("UIRenderRect", Vector) = (0,0,0,0)

			//2D内发光
			//[Toggle(SHOW_INNER_GLOW)] _ShowInnerGlow("Show Inner Glow", Int) = 0		//开启外轮廓的Toggle
			[MaterialToggle] _ShowInnerGlow("Show Inner Glow", Int) = 0		//开启外轮廓的Toggle
			_InnerGlowWidth("Inner Glow Width", Float) = 0.1												//内发光的宽度
			_InnerGlowColor("Inner Glow Color", Color) = (0,0,0,1)										//内发光的颜色
			_InnerGlowAccuracy("Inner Glow Accuracy", Int) = 2											//内发光的精度
			_InnerGlowAlphaSumThreshold("Inner Glow Alpha Sum Threshold", Float) = 0.5								//内发光的透明度和的阈值
			_InnerGlowLerpRate("Inner Glow Lerp Rate", range(0, 1)) = 0.8							//内发光颜色和原颜色的差值

			//UI支持属性
			_StencilComp("Stencil Comparison", Float) = 8
			_Stencil("Stencil ID", Float) = 0
			_StencilOp("Stencil Operation", Float) = 0
			_StencilWriteMask("Stencil Write Mask", Float) = 255
			_StencilReadMask("Stencil Read Mask", Float) = 255
			_ColorMask("Color Mask", Float) = 15

			[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
				#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

				//#pragma multi_compile_local _ SHOW_OUTLINE
				//#pragma multi_compile_local _ SHOW_INNER_GLOW

				sampler2D _MainTex;
				float4 _MainTex_ST;
				half4 _MainTex_TexelSize;
				float _EdgeWidth;
				fixed _EdgeAlphaThreshold;
				fixed4 _EdgeColor;
				float _UEdgeFade;
				float _VEdgeFade;
				float _EdgeFadeStrength;
				float _OriginAlphaThreshold;

				int _ShowOutline;
				int _ShowInnerGlow;

				float _InnerGlowWidth;
				fixed4 _InnerGlowColor;
				int _InnerGlowAccuracy;
				float _InnerGlowAlphaSumThreshold;
				float _InnerGlowLerpRate;

				float4 _ClipRect;
				float4 _UIRenderRect;	//xmin,ymin,xmax,ymax


#define MAX_SAMPLE 8
				struct v2f
				{
					float4 vertex : SV_POSITION;
					float4 worldPosition : TEXCOORD0;
					//float2 uv[9] : TEXCOORD1;
					float2 uv : TEXCOORD1;
				};

				//计算渲染边界Alpha值
				half CalculateRenderRectAlpha(float3 worldPos)
				{
					float width = _UIRenderRect.z - _UIRenderRect.x;
					float height = _UIRenderRect.w - _UIRenderRect.y;
					float ufade = _UEdgeFade * width;
					float vfade = _VEdgeFade * height;

					//return (worldPos.x - _UIRenderRect.x) / width;
					//return (worldPos.y - _UIRenderRect.y) / height;

					float alpha = 0;
					float count = 0;
					float valid = 0;

					//左
					if (worldPos.x >= _UIRenderRect.x && worldPos.x <= (_UIRenderRect.x + ufade))
					{
						float a = lerp(1, 0, (worldPos.x - _UIRenderRect.x) / ufade);
						if (a > alpha)
							alpha = a;
						valid = 1;
					}

					//右
					if (worldPos.x <= _UIRenderRect.z && worldPos.x >= (_UIRenderRect.z - ufade))
					{
						float a = lerp(1, 0, (_UIRenderRect.z - worldPos.x) / ufade);
						if (a > alpha)
							alpha = a;
						valid = 1;
					}

					//上
					if (worldPos.y <= _UIRenderRect.w && worldPos.y >= (_UIRenderRect.w - vfade))
					{
						float a = lerp(1, 0, (_UIRenderRect.w - worldPos.y) / vfade);
						if (a > alpha)
							alpha = a;
						valid = 1;
					}

					//下
					if (worldPos.y >= _UIRenderRect.y && worldPos.y <= (_UIRenderRect.w + vfade))
					{
						float a = lerp(1, 0, (worldPos.y - _UIRenderRect.y) / vfade);
						if (a > alpha)
							alpha = a;
						valid = 1;
					}

					return alpha * step(0.5, valid);
				}

				half CalculateAlphaSumAround(v2f i)
				{

					//float2 offset[8] = {  {-1,1},{0,1},{1,1},{-1,0},{1,0},{-1,-1},{0,-1},{1,-1} };
					float2 offset[4] = { {0,1},{-1,0},{1,0},{0,-1} };

					half texAlpha;
					half alphaSum = 0;
					for (int it = 0; it < 4; it++)
					{
						float2 uv = i.uv+ offset[it]* _MainTex_TexelSize.xy * _EdgeWidth;
						texAlpha = tex2D(_MainTex, uv).a;
						alphaSum += texAlpha;
					}
					return alphaSum;
				}

				float CalculateCircleSumAlpha(float2 orign, float radiu, int time)
				{
					//通过精度来划分一个圆，然后通过一个for循环来计算偏移，然后采样机上透明度。
					//我本来使用的精度的平方，但是这样unityshader会报一个错误，说迭代的次数太长，不能超过1024次，
					//但是实际上我并没有用那么多次，但是没法解决就手动输入精度
					float sum = 0;
					float perAngle = 360 / time;
					for (int i = 0; i < time; i++)
					{
						float2 newUV = orign + radiu * float2(cos(perAngle * i), sin(perAngle * i));
						sum += tex2D(_MainTex, newUV).a;
					}

					return sum;
				}

				v2f vert(appdata_img v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.worldPosition = mul(unity_ObjectToWorld, v.vertex); // v.vertex;

					o.uv = v.texcoord;

					/*
					half2 uv = v.texcoord;

					o.uv[0] = uv + _MainTex_TexelSize.xy * half2(-1, -1) * _EdgeWidth;
					o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, -1) * _EdgeWidth;
					o.uv[2] = uv + _MainTex_TexelSize.xy * half2(1, -1) * _EdgeWidth;
					o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-1, 0) * _EdgeWidth;
					o.uv[4] = uv;
					o.uv[5] = uv + _MainTex_TexelSize.xy * half2(1, 0) * _EdgeWidth;
					o.uv[6] = uv + _MainTex_TexelSize.xy * half2(-1, 1) * _EdgeWidth;
					o.uv[7] = uv + _MainTex_TexelSize.xy * half2(0, 1) * _EdgeWidth;
					o.uv[8] = uv + _MainTex_TexelSize.xy * half2(1, 1) * _EdgeWidth;
					*/

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
				
					fixed4 innerGlow = fixed4(0,0,0,0);
					fixed4 outline = fixed4(0,0,0,0);
					fixed4 orignColor = tex2D(_MainTex, i.uv);
					float isOutlineValid = 0;

					//2D图片外轮廓
					//#if defined(SHOW_OUTLINE)
					if (_ShowOutline == 1)
					{
						//边缘颜色
						half alphaSum = CalculateAlphaSumAround(i)+ orignColor.a;
						half edgeAlpha = 1 - saturate(alphaSum / 5);
						//half edgeAlpha = 1 - saturate(orignColor.a);
						//return fixed4(1, 1, 1, edgeAlpha);

						//渲染区域
						half renderRectAlpha = CalculateRenderRectAlpha(i.worldPosition);
						//return fixed4(1, 1, 1, renderRectAlpha);

						float outlineAlpha = 0;
						if (edgeAlpha > 0.005 || renderRectAlpha > 0.005)
						{
							float outlineAlpha = renderRectAlpha;
							if (edgeAlpha > outlineAlpha)
								outlineAlpha = edgeAlpha;

							if (outlineAlpha > _EdgeAlphaThreshold)
							{
								isOutlineValid = 1;
							}

							//重新进行Alpha映射
							outlineAlpha = (outlineAlpha - _EdgeAlphaThreshold);

							//渐变强度
							outlineAlpha = saturate(outlineAlpha * _EdgeFadeStrength);
							//return fixed4(1, 1, 1, outlineAlpha);

							outline = fixed4(_EdgeColor.rgb, outlineAlpha);
							//return outline;
						}
						else
						{
							outline = fixed4(_EdgeColor.rgb, 0);
						}
					}
					//#endif

					//2D图片的内发光
					//#if defined(SHOW_INNER_GLOW)
					if (_ShowInnerGlow == 1)
					{
						//计算透明度的和
						//float alphaCircleSum = CalculateCircleSumAlpha(i.uv, _InnerGlowWidth, _InnerGlowAccuracy) / _InnerGlowAccuracy;

						//做一个采样的容错
						float sampleTime = min(MAX_SAMPLE, _InnerGlowWidth);
						float alphaCircleSum = CalculateCircleSumAlpha(i.uv, _InnerGlowWidth, sampleTime) / sampleTime;
						float innerColorAlpha = 0;

						//这里获取到内发光的的透明度，并且做了渐变，让靠近边界的颜色更亮一些，原理的透明度的会越来越低
						innerColorAlpha = 1 - saturate(alphaCircleSum - _InnerGlowAlphaSumThreshold) / (1 - _InnerGlowAlphaSumThreshold);

						//剔除超出原图的像素的颜色。
						if (orignColor.a <= _OriginAlphaThreshold)
						{
							innerColorAlpha = 0;
						}

						fixed3 innerColor = _InnerGlowColor.rgb * innerColorAlpha;
						innerGlow = fixed4(innerColor.rgb, innerColorAlpha);
						//return innerGlow;
					}
					//#endif

					//最终输出颜色
					fixed4 finalColor = fixed4(1,1,1,1);

					//将外轮廓和内发光元颜色叠加输出。
					//#if defined(SHOW_OUTLINE)
					if (_ShowOutline == 1)
					{
						float outlineAlphaDiscard = orignColor.a > _OriginAlphaThreshold;
						orignColor = outlineAlphaDiscard * orignColor;

						//乘2是为了更加突出外发光
						fixed4 glowColor = lerp(orignColor, innerGlow, _InnerGlowLerpRate * innerGlow.a);

						//混合
						float blendAlpha = outline.a * isOutlineValid;
						glowColor.rgb = outline.rgb * blendAlpha + glowColor.rgb * (1 - blendAlpha);
						finalColor = glowColor;
					}
					else
					{
						//#else
						finalColor = lerp(orignColor, innerGlow * 2, _InnerGlowLerpRate * innerGlow.a);
					}
					//#endif

		#ifdef UNITY_UI_CLIP_RECT
						finalColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
		#endif

		#ifdef UNITY_UI_ALPHACLIP
						clip(finalColor.a - 0.0001);
		#endif

						return finalColor;

					}

					ENDCG
				}

		}

}