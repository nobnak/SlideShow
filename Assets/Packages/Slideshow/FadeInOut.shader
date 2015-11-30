Shader "Unlit/FadeInOut" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Fader ("Fader", Range(0.0, 1.0)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			static const float4x4 FULLSCREEN_MATRIX = {
				2, 0, 0, 0,
				0, 2, 0, 0,
				0, 0, 0, 0,
				0, 0, 0, 1
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Fader;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(FULLSCREEN_MATRIX, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				return col * _Fader;
			}
			ENDCG
		}
	}
}
