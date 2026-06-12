using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.HostileProj
{
    public class HostileLeafShot : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CrystalLeafShot;
        private bool PlaySound = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!PlaySound)
            {
                SoundEngine.PlaySound(SoundID.Item4,Projectile.Center);
                PlaySound = true;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.8f, 0.4f);

            if (Projectile.timeLeft % 3 == 0)
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Terra,
                    Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f,
                    150, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            float rotation = Projectile.rotation;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float factor = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.LimeGreen * (factor * 0.4f);
                trailColor.A = 0;

                Main.spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    rotation,
                    origin,
                    Projectile.scale * factor,
                    SpriteEffects.None,
                    0f
                );
            }
            Main.EntitySpriteDraw(texture,Projectile.Center, null, lightColor,Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
