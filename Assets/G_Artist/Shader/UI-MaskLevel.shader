// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/UI-MaskLevel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OpenEdgeColor("OpenEdgeColor", Color) = (0,1,0,1)
        _AttackEdgeColor("AttackEdgeColor", Color) = (0,1,0,1)
        _OpenColor("OpenColor", Color) = (1,1,1,0.5)
        _AttackColor("AttackColor", Color) = (1,0,0,0.5)
        _BGColor("BGColor", Color) = (1,1,1,0.5)
        _CenterPos("Center Pos",Vector) = (0,0,0,0)
        _AttackCenter("AttackCenter",Vector) = (0,0,0,0)
        _AttackSize("AttackSize",Vector) = (0,0,0,0)
        _Radius("Radius",Float) = 0.5
        _LineWidth("LineWidth",Float) = 8
        _ExpandWidth("ExpandWidth",Float) = 0.2
	

        _NoiseTex("Noise Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }



        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass Keep    //[_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest   [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        //Blend One One
        ColorMask [_ColorMask]


        CGINCLUDE

            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OpenEdgeColor;
            fixed4 _AttackEdgeColor;
            fixed4 _OpenColor;
            fixed4 _AttackColor;
            fixed4 _BGColor;

            float4 _CenterPos;
            float4 _AttackCenter;
            float4 _AttackSize;
            float _LineWidth;
            float _Radius;
            float _ExpandWidth;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

#define MAX_OFFSET 8

            /*
            float calcAlpha(float2 uv,float ew)
            {
                float d = 1000;
                float a = 0;
                float2 offset[8] = { {-1,1},{0,1},{1,1},{-1,0},{1,0},{-1,-1},{0,-1},{1,-1} };
                for (int i = 0; i < 8; ++i)
                {
                    float2 offsetuv = offset[i] * ew;
                    a = tex2D(_MainTex, uv + offsetuv* _MainTex_TexelSize.xy).a;
                    if (a < 0.1f)
                    {
                        float l = length(offsetuv);
                        if (l < d)
                        {
                            d = l;
                        }
                    }
                }

                return saturate(d/ew);
            }
            */

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = mul(unity_ObjectToWorld, v.vertex); //v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color;// *_Color;
                return OUT;
            }




            //已经攻打过的
            fixed4 frag(v2f IN) : SV_Target
            {
                 half4 color = tex2D(_MainTex,IN.texcoord);
                 clip(color.a - 0.1f);
                return _Color;
            }

            //当前攻打的
            fixed4 attackfrag(v2f IN) : SV_Target
            {
                 half4 color = tex2D(_MainTex,IN.texcoord);
                 clip(color.a - 0.1f);

                
                 float2 uv = abs(IN.worldPosition.xy - _CenterPos.xy) / _Radius;// +float2(0.5, 0.5);
                 uv = TRANSFORM_TEX(uv, _NoiseTex);
                 half a =  tex2D(_NoiseTex, uv).r;
                 a = clamp(0, 1, a+0.3 );
                 //a = 1;


                 

                float3 v = IN.worldPosition.xyz - _AttackCenter.xyz;
                float len = length(v);

                 //当前点离中心点的权重
                 float2 w = abs(v.xy) / (_AttackSize.xy + 0.001f);
                 //float coff = lerp(0, 1, pow(len2 / (_AttackCenter.w + 0.0001f), 5));
                 
                 float r = max(_AttackSize.x, _AttackSize.y);
                 float coff =  lerp(0, 1, pow(max(w.x,w.y),0.2) ); //,
                 //coff = lerp(0,coff, len/(r));
                 //float coff = lerp(0, 1, pow((len) / (len2+20), 1.5));

                 color = _Color;
                 r = _CenterPos.w * a;
                 v = IN.worldPosition.xyz - _CenterPos.xyz;
                 len = length(v);
                 if(len <= r) //_CenterPos.w  abs(len)-
                 {
                     color.r = 0.5f;  // r 通道 0，没开，b 已经开了
                     //color.b = 1;
                     float ew = clamp(0, _ExpandWidth, r - len);
                     color.g =  lerp(0.6, 1, ew / _ExpandWidth);
                     return color; // 0,1,1
                 }
                 
                 
                 //color.g *= clamp(len - _CenterPos.w * a, 0, 1); //clamp(0,1,len- r);
                 color.g *= coff;  //
                 //color.g = clamp(0.3, 0.5, color.g);

                 //color.g *= 1.0-calcAlpha(IN.texcoord,10);
                return color;
            }

            float getEdgeType(float4 color)
            {
                //return step(1, color.r * 2)*2 + step(1,color.b * 2);

                return floor(color.r*2.45);
            }


            //是否在边缘,像素类型==
            void calcColorInfo(fixed2 uv, float4 color,out float w,out float edgeType, out bool bIsEdge)
            {
                // r （0,0.5,1）代表类型 ，g 代表alpha
                edgeType = getEdgeType(color); // 0,1,2
                w = step(0.01f,color.g);  //alpha

                bIsEdge = false;
                float t1 = edgeType;
                float t2 = 0;
                float edgew = 0;
                float4 clr;
                float2 offset[MAX_OFFSET] = { {-1,1},{0,1},{1,1},{-1,0},{1,0},{-1,-1},{0,-1},{1,-1} };

                //float2 offset[MAX_OFFSET] = {{0,1},{-1,0},{1,0},{0,-1} };

                //for (int j = 1; j <= _LineWidth; ++j)
                {
                    for (int i = 0; i < MAX_OFFSET; ++i)
                    {
                        clr = tex2D(_MainTex, uv + offset[i] * _LineWidth * _MainTex_TexelSize.xy);

                        t2 = getEdgeType(clr);
                        w += step(0.01f, color.g);
                        edgew += clamp(0, 1, abs(t1 - t2));
                        if (abs(t2 - t1) > 0.01f)
                        {
                            bIsEdge = true;
                        }

                        //开放部分的边缘优先
                        if (t2 > 0 && (edgeType<0.01f || edgeType> t2))
                        {
                            edgeType = t2;
                        }

                    }
                }

               


                w /= (MAX_OFFSET+1);
                edgew /= MAX_OFFSET;
                if (bIsEdge)
                {
                    w = edgew;
                }

                w += 1 / (MAX_OFFSET + 1);

            }

           


            //全屏mask
            float4   fullScreenfrag(v2f IN) : SV_Target
            {
                // rg 格式  r,g
                float4 color = tex2D(_MainTex,IN.texcoord);
                float alpha = color.g;
                float4 finalColor = float4(color.r, 0, 0, alpha); 
                float4 edgeColor = float4(_OpenEdgeColor.xyz, 0);

                float clrType = getEdgeType(color);
                float edgeType = clrType;
                bool bIsEdge = false;
                calcColorInfo(IN.texcoord, color, alpha, edgeType, bIsEdge);
                if(bIsEdge)
                {
                    edgeColor.a = saturate(alpha*5);// alpha*5;
                    if (edgeType > 1.0f)
                    {
                        edgeColor.rgb = _AttackEdgeColor.rgb;
                        edgeColor.a *= _AttackEdgeColor.a;
                    }
                    else
                    {
                        edgeColor.a *= _OpenEdgeColor.a;
                    }

                    
                    alpha = 0;
                }
                else
                {
                    //处理非关卡外灰化
                    float a = saturate(1 - clrType);
                    finalColor.a += a * 0.6f;
                    if (clrType<0.3f)
                    {
                        finalColor = _BGColor;
                       
                    }
                    else if (clrType <1.2f)
                    {
                        
                        finalColor.rgb = _OpenColor.rgb;
                        finalColor.a =  _OpenColor.a + (1 - color.g);
                    }
                    else
                    {
                       // return float4(0, 0, 0, 0);
                        finalColor.a = _AttackColor.a* color.g;
                        finalColor.rgb = _AttackColor.rgb;
                       

                    }

                    edgeColor = finalColor;

                }
                
                finalColor = lerp(edgeColor, finalColor, alpha);
                return finalColor;
            }



            //模糊效果
            fixed4 Blurfrag(v2f IN) : SV_Target
            {
                 float4 color = tex2D(_MainTex,IN.texcoord);
                // return color;
                 clip(color.a - 0.001f);

                 float coff = 1.0 / 9.0;
                 color *= coff;
                 float2 offset[MAX_OFFSET] = { {-1,1},{0,1},{1,1},{-1,0},{1,0},{-1,-1},{0,-1},{1,-1} };

                 //float2 offset[MAX_OFFSET] = {{0,1},{-1,0},{1,0},{0,-1} };
                 for (int i = 0; i < MAX_OFFSET; ++i)
                 {
                     color += tex2D(_MainTex, IN.texcoord + offset[i] * _MainTex_TexelSize.xy) * coff;
                 }
                return color;
            }


        ENDCG


        Pass
        {
            Name "OpenMask"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        ENDCG
        }

        Pass
        {
            Name "AttackMask"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment attackfrag
        ENDCG
        }


         Pass
        {
            Name "FullScreenMask"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment fullScreenfrag
        ENDCG
        }

        Pass
        {
            Name "BlurMask"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment Blurfrag
        ENDCG
        }
    }
}
