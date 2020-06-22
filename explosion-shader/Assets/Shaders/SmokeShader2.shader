Shader "Custom/SmokeShader2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		[PerRendererData]_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_RimTex("Rim Texture", 2D) = "white" {}
		_RimPower("Rim Width", Range(0,10)) = 2.15
		_DissolveFactor("Dissolve Factor", Range(0,10)) = 0.4

		_DissolveTex("Dissolve Texture", 2D) = "black" {}
		_DissolveAmount("Dissolve Amount", Range(0, 1)) = 0.0
		_TimeCreated("Time Created", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull back
        LOD 200
        CGPROGRAM

		#pragma surface surf Lambert alpha

        #pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float2 uv_RimTex;
			float2 uv_DissolveTex;
			float3 viewDir;
		};

		sampler2D _DissolveTex;
		float _DissolveAmount;
		float _DissolveFactor;
		float4 _Color;
		sampler2D _MainTex;
		float _TimeCreated;
		sampler2D _RimTex;
		float _RimPower;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
			float dissolve = tex2D(_DissolveTex, IN.uv_DissolveTex).r;
			dissolve = dissolve * 0.95;
			float isVisible = dissolve - _DissolveAmount;
			clip(isVisible);

			// calculate the rim
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
			float rim = saturate(dot(normalize(IN.viewDir), o.Normal));
			half rimComp = 1.0 - rim;
			// - (_Time * _DissolveFactor)
			if (rimComp > 0.45f) {
				//o.Alpha = 0;
			}
			else {
				o.Alpha = -1 * (rimComp + tex2D(_RimTex, IN.uv_RimTex).rgb) + (rim * _RimPower) - (_TimeCreated * _DissolveFactor);
			}

			if (o.Alpha < 0) {
				o.Alpha = 0;
			}
			if (o.Alpha > 1) {
				o.Alpha = 1;
			}
			
        }
        ENDCG
    }
    FallBack "Diffuse"
}
