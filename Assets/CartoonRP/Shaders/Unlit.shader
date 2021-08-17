Shader "CRP/Unlit/Color" {
	
	Properties {
		_Color("Color", Color) = (1, 1, 1, 1)	
	}
	
	SubShader {
		
		Pass {

			Tags { "LightMode" = "CRPUnlit" }
			
			HLSLPROGRAM
			#pragma vertex UnlitColorVertex
			#pragma fragment UnlitColorFragment

			#include "../ShaderLibrary/Common.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float4 _Color;
			CBUFFER_END
			
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS : NORMAL;
			};

			struct Pixels
			{
				float3 color : COLOR0;
				float3 normals : COLOR1;
			};

			
			Varyings UnlitColorVertex(Attributes a)
			{
				Varyings v;
				float3 positionWS = TransformObjectToWorld(a.positionOS.xyz);
				v.positionCS = TransformWorldToHClip(positionWS);
				v.normalWS = TransformObjectToWorldNormal(a.normalOS);

				return v;
			}
			
			Pixels UnlitColorFragment(Varyings v)
			{
				Pixels p;
				p.color = _Color;
				p.normals = v.normalWS;
			    return p;
			}
			
			ENDHLSL
		}
	}
}