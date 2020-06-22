Shader "Custom/BlinkShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DarkFactor("DarkFactor", Range(0,5)) = 0.5
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				float4 _MainTex_TexelSize;
				float _DarkFactor;

				float4 mask(sampler2D tex, float2 uv, float4 size) {
					
					float4 color = tex2D(tex, uv + float2(0, 0));
						return color * _DarkFactor;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = mask(_MainTex, i.uv, _MainTex_TexelSize);

					return col;
				}
				ENDCG
			}
		}
}
