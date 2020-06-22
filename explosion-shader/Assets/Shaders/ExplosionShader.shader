Shader "Custom/ExplosionShader"
{
    Properties
    {
		_Emission("Emission Value", Range(0.0, 0.5)) = 0.1
		_DispTex("Displacement Texture", 2D) = "black" {}
		_DispFactor("Displacement Factor", Range(0, 1.0)) = 0.395
		_NoiseMove("Noise Move", Vector) = (1,0,0)
		_ColorGradTex("Color Gradient", 2D) = "black" {}
		_Depth("Depth", Vector) = (0,0.5,0)
		_DepthSpeed("Depth Speed", float) = 1
		_DisapFactor("Disappearing Factor", Range(0, 1.0)) = 0.781
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		Cull off

        CGPROGRAM
		#pragma surface surf Lambert vertex:vert 
		#pragma target 3.0


        sampler2D _MainTex;
		half _Glossiness;
		float _Emission;
		half _Metallic;
		fixed4 _Color;
		float _DepthSpeed;
		sampler2D _ColorGradTex;
		sampler2D _DispTex;
		float _DispFactor;
		float3 _NoiseMove;
		float3 _Depth;
		float _DisapFactor;

		struct Input
		{
			float2 uv_GradTex;
			float2 uv_DispTex;
			float normalVal;
		};

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v)
		{
			// mix displacement and add it to normal
			float3 dispVal = tex2Dlod(_DispTex, float4(v.texcoord.xy, 0, 0));
			float dispX = dispVal.x * _NoiseMove.x;
			float dispY = dispVal.y * _NoiseMove.y;
			float dispZ = dispVal.z * _NoiseMove.z;
			float totalDisp = dispX + dispY + dispZ;
			v.vertex.xyz += v.normal * totalDisp * _DispFactor;
		}

        void surf (Input IN, inout SurfaceOutput o)
        {
			// create the displacement in color
			float3 dispColor = tex2D(_DispTex, IN.uv_DispTex);

			// dot product to get the mixed values
			float totalDisp = dot(dispColor, _NoiseMove);

			// use the depth difference to add noise
			totalDisp = _DepthSpeed * totalDisp * (-1.0 * (_Depth.x - _Depth.y));
			totalDisp += 1.05 * _Depth.x;

			// get the color from the gradient based on displacement
			float4 color = tex2D(_ColorGradTex, float2(totalDisp, 0.0));

			if (o.Alpha < 0) {
				o.Alpha = 0;
			}
			
			// add emission to eliminate the dark shadows in the fire
			o.Albedo = color.rgb;
			o.Emission = o.Albedo;
			

			float finalColor = -1.0 * (totalDisp - _DisapFactor);
			clip(finalColor);

        }
        ENDCG
    }
    FallBack "Diffuse"
}
