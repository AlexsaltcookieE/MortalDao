using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.ExtraTextures.MainMenu
{
    public class  MainMenu : ModMenu
    {
        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("MortalDao/Content/ExtraTextures/MainMenu/MenuLogo");
        public override bool PreDrawLogo(SpriteBatch sb, ref Vector2 center, ref float rot, ref float scale, ref Color color)
        {
            var tex = Logo.Value;
            var origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            sb.Draw(tex, center, null, color, rot, origin, 0.2f, SpriteEffects.None, 0f);
            return false; // 已经自己画了，不让默认再画一次
        }
    }
    public class MainMenuSystem : ModSystem
    {

    }
}
