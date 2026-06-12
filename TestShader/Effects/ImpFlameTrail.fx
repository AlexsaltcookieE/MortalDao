sampler2D imageSampler : register(s0);
float _uTime;
float4x4 MatrixTransform;

struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 Position : POSITION;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, MatrixTransform);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PS(PixelInput input) : COLOR
{
    float2 uv = input.TexCoord;
    uv = saturate(uv * 0.98 + 0.01);
    // ===== 火焰噪声采样（使用主贴图 R 通道）=====
    float noise = tex2D(imageSampler,
        float2(frac(uv.x * 1.0 - _uTime * 2.5), uv.y)).r;

    // ===== 边缘衰减（中间亮，两边暗）=====
    float bloom = pow(sin(uv.y * 3.1415926), lerp(3.0, 10.0, uv.x));
    bloom = lerp(bloom, 0.7, uv.x);

    // ===== 最终亮度 =====
    float intensity = pow(bloom, 6.0) * lerp(2.0, 7.0, noise);

    // ===== 颜色叠加 =====
    float4 finalColor = input.Color;
    finalColor.rgb *= intensity;

    return finalColor;
}

technique Colorize
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}