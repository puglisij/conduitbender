Shader "Custom/ConduitHighlight" {
	Properties {
		_Highlight ("Highlight Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows //vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Highlight;

		static const fixed4 EPSILON = fixed4(0.005, 0.005, 0.005, 1);
		//void vert(inout appdata_full v) {
		//	v.vertex.xyz += v.normal;
		//}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			// A little hacky here. Basically if the interpolated vertice color 
			// is >= Highlight (assume conduit color channels < _Highlight) then use full Highlight color. 
			float4 c = step(_Highlight, IN.color + EPSILON) * _Highlight;

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
