using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{
    public class IronOreProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.IronOre;
        private int ProjectileTimer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 200;
        }
        public override void AI()
        {
            if(ProjectileTimer > 30)
            {
                Projectile.velocity.Y += 4;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Iron);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = new Color(140, 140, 140, 90);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;

                Color color = trailColor * progress * 0.7f;

                Main.spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    color,
                    rotation,
                    origin,
                    Projectile.scale * (0.8f + progress * 0.4f),
                    SpriteEffects.None,
                    0f
                );
            }
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );
            return false;
        }
    }
}
