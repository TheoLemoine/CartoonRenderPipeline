
/**
 * Computes sobel filter around a pixel in given texture
 */
float4 Sobel(float2 step, float2 pos, sampler2D tex)
{
    float4 n[9];

    // sample all around
    n[0] = tex2D(tex, pos - step);
    n[1] = tex2D(tex, pos + step * float2(0, -1));
    n[2] = tex2D(tex, pos + step * float2(1, -1));
    n[3] = tex2D(tex, pos + step * float2(-1, 0));
    n[4] = tex2D(tex, pos);
    n[5] = tex2D(tex, pos + step * float2(1, 0));
    n[6] = tex2D(tex, pos + step * float2(-1, 1));
    n[7] = tex2D(tex, pos + step * float2(0, 1));
    n[8] = tex2D(tex, pos + step);

    // apply convolution matrix
    const float4 sobel_edge_h = n[2] + 2.0 * n[5] + n[8] - (n[0] + 2.0 * n[3] + n[6]);
    const float4 sobel_edge_v = n[0] + 2.0 * n[1] + n[2] - (n[6] + 2.0 * n[7] + n[8]);
    return sqrt(sobel_edge_h * sobel_edge_h + sobel_edge_v * sobel_edge_v);
}