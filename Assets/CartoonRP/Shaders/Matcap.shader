Shader "CRP/Unlit/Matcap" {
	
	Properties {
		_MatCapTexture("MatCap", 2D) = "white"	
	}
	
	SubShader {
		
		Pass {

			Tags { "LightMode" = "CRPUnlit" }
			
			HLSLPROGRAM
			#pragma vertex MatcapVertex
			#pragma fragment MatcapFragment

			#include "../ShaderLibrary/Common.hlsl"

			CBUFFER_START(UnityPerMaterial)
				sampler2D _MatCapTexture;
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
				float2 uvMatCap : MATCAP_UV;
			};

			struct Pixels
			{
				float3 color : COLOR0;
				float3 normals : COLOR1;
			};

			Varyings MatcapVertex(Attributes a)
			{
				Varyings v;
				float3 positionWS = TransformObjectToWorld(a.positionOS.xyz);
				v.positionCS = TransformWorldToHClip(positionWS);

				// get normals in view space, then flatten and remap
				v.normalWS = TransformObjectToWorldNormal(a.normalOS);
				v.uvMatCap = mul(UNITY_MATRIX_V, v.normalWS).xy * 0.5 + 0.5;

				return v;
			}
			
			Pixels MatcapFragment(Varyings v)
			{
				Pixels p;
				p.color = tex2D(_MatCapTexture, v.uvMatCap);
				p.normals = v.normalWS;
			    return p;
			}
			
			ENDHLSL
		}
		
	}
}