using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MortalDao.Content.ModSetting.UI
{
    public class RealmEXPBar : UIElement
    {
        private float _progress;

        private float _barWidth;
        private float _barHeight;

        private Microsoft.Xna.Framework.Color _BorderColor = Microsoft.Xna.Framework.Color.Black;
        private Microsoft.Xna.Framework.Color _BackGroundColor = Microsoft.Xna.Framework.Color.White;
        private Microsoft.Xna.Framework.Color _FillColor =  Microsoft.Xna.Framework.Color.Green;

        public void SetProgress(float Current,float max)
        {
            _progress = max > 0f ? MathHelper.Clamp(Current / max, 0f, 1f) : 0f;
        }
        public void SetColors(Microsoft.Xna.Framework.Color? border = null, Microsoft.Xna.Framework.Color? background = null, Microsoft.Xna.Framework.Color? fill = null)
        {
            if (border.HasValue) _BorderColor = border.Value;
            if (background.HasValue) _BackGroundColor = background.Value;
            if (fill.HasValue) _FillColor = fill.Value;
        }
        public RealmEXPBar(float width, float height)
        {
            _barWidth = width;
            _barHeight = height;
            Width.Set(width, 0f);
            Height.Set(height, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)_barWidth, (int)_barHeight);

            // 背景
            spriteBatch.Draw(tex, rect, _BackGroundColor);

            // 填充
            if (_progress > 0.001f)
            {
                int fillWidth = (int)(_barWidth * _progress);
                if (fillWidth > 0)
                    spriteBatch.Draw(tex,
                        new Rectangle((int)dims.X, (int)dims.Y, fillWidth, (int)_barHeight),
                        _FillColor);
            }

            // 1px 黑色边框（可选）
            spriteBatch.Draw(tex, new Rectangle(rect.X, rect.Y, rect.Width, 1), _BorderColor);
            spriteBatch.Draw(tex, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), _BorderColor);
            spriteBatch.Draw(tex, new Rectangle(rect.X, rect.Y, 1, rect.Height), _BorderColor);
            spriteBatch.Draw(tex, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), _BorderColor);
        }
    }
}
