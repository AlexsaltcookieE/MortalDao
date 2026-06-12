// 路径：MortalDao/Content/ModSetting/RendererSystem/TrailVertex.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MortalDao.Content.ModSetting.RendererSystem
{
    /// <summary>
    /// 弹幕轨迹顶点结构体（匹配 HLSL 的 VertexInput 输入）
    /// </summary>
    public struct TrailVertex : IVertexType
    {
        #region 字段（与 Shader 一一对应）
        public Vector3 Position;
        public Color Color;
        public Vector3 TexCoord;
        #endregion

        #region 顶点声明（必须定义，用于 GPU 解析数据）
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            // 位置（12字节，Vector3）
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            // 颜色（4字节，Color）
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            // 纹理坐标（12字节，Vector3）
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );
        #endregion

        #region 构造函数
        public TrailVertex(Vector3 position, Color color, Vector3 texCoord)
        {
            Position = position;
            Color = color;
            TexCoord = texCoord;
        }
        #endregion

        #region IVertexType 实现
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
        #endregion
    }
}