Shader "Hidden/Screen"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex ScreenVertex
            #pragma fragment frag
            
            #include "../ShaderLibrary/Passes/Vertex/ScreenVertex.hlsl"
            #include "../ShaderLibrary/Sobel.hlsl"

            sampler2D _Color;
            sampler2D _Normals;
            sampler2D _Depth;

            int _ScreenWidth;
            int _ScreenHeight;

            float4 frag (Varyings i) : SV_Target
            {
                const float2 step = 1 / float2(_ScreenWidth, _ScreenHeight);

                float4 normalSobel = Sobel(step, i.texcoord, _Normals);
                float4 depthSobel = Sobel(step, i.texcoord, _Depth);

                bool normalEdge = ((normalSobel.x + normalSobel.y + normalSobel.z) / 3) > 0.4f;
                bool depthEdge = depthSobel.r > 0.1f;
                float4 edge = normalEdge || depthEdge ? float4(0, 0, 0, 0) : float4(1, 1, 1, 1);
                
                float4 col = tex2D(_Color, i.texcoord);
                return col * edge;
            }
            ENDHLSL
        }
    }
}