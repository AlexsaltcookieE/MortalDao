sampler uImage0 : register(s0);   // SpriteBatch 唯一喂进来的贴图
float uTime;                       // 由 C# 侧传入的游戏时间
float uOpacity;                   // 不透明度（预留）

// ---------- 入口结构体 ----------
struct PS_INPUT
{
    float2 uv : TEXCOORD0;
};

// ---------- 像素着色器主函数 ----------
float4 MainPS(PS_INPUT input) : SV_Target
{
    // 把 UV 原点移到中心 (0,0)，方便做对称形状
    float2 uv = input.uv - 0.5;

    // 拉伸成菱形比例（y 方向压更窄，x 方向拉更长）
    uv.y *= 0.6;
    uv.x *= 0.2;

    // 曼哈顿距离 = |x| + |y|，天然画出菱形
    float d = abs(uv.x) + abs(uv.y);

    // 光晕主色（叶绿绿色调）
    float3 glow = float3(0.55, 1.0, 0.7);

    // 呼吸节奏：±5% 周期摆动，让光晕"活"起来
    float breath = sin(uTime * 5.0) * 0.05 + 1.0;

    // 内核（很亮很细的白核） + 外层光晕
    float core    = 1.0 - smoothstep(0.0, 0.002 * breath, d);
    float falloff = 1.0 - smoothstep(0.0, 0.02  * breath, d);

    // 加法混合：暗部输出 0，只保留光晕
    float3 color = glow * falloff + float3(1.0, 1.0, 1.0) * core;
    color *= falloff * uOpacity;

    return float4(color, 1.0);
}

// ---------- 技术 & Pass ----------
technique LeafLaser
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
}