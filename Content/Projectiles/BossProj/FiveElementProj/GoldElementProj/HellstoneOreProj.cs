using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{
    public class Hellstone : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Hellstone;
        private int ProjectileTimer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 60;
            // 确保弹幕在网络中同步
            Projectile.netImportant = true;
        }

        public override void OnKill(int timeLeft)
        {
            // 仅非服务器环境播放音效和生成粒子
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Lava, Scale: 1f);
                }
                float baseRotation = Projectile.velocity.ToRotation();
                float[] angleOffsets = new float[]
                {
                                    -(MathHelper.PiOver4)/2,
                                    0f,
                                    MathHelper.PiOver4/2
                };
                foreach (float offset in angleOffsets)
                {
                    Vector2 velocity = Vector2.UnitX.RotatedBy(baseRotation + offset) * 10f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_Death(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<MeteroriteOreProj>(), // 可替换成你的自定义激光
                        22,
                        2f,
                        Projectile.owner
                    );
                }
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Burning, 180);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制代码仅在客户端执行（服务器不会调用此方法）
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = Color.Orange;

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