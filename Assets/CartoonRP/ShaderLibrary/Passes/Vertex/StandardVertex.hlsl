#include "../../Common.hlsl"

// standard vertex core, putting vertex in the right place
float4 StandardVertex (float3 positionOS : POSITION) : SV_POSITION
{
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToHClip(positionWS);
}