float time;
sampler2D tex : register(s0);

float4 MainPS(float2 uv : TEXCOORD0) : COLOR
{
    uv.x += sin(uv.y * 10 + time * 5) * 0.05;
    float4 c = tex2D(tex, uv);
    c.a *= 0.8;
    return c;
}