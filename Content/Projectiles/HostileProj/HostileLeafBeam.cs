using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.HostileProj
{
    public class HostileLeafBeam : ModProjectile
    {
        private float CurrentWidth = 0;
        private float MaxWidth = 18f;
        private bool ReachMaxWidth = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.damage = 30;
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.scale = 0.01f;
        }

        public override void AI()
        {
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
        private static bool LineIntersectsRect(Vector2 p0, Vector2 p1, Rectangle rect)
        {
            float tMin = 0f;
            float tMax = 1f;
            float dx = p1.X - p0.X;
            float dy = p1.Y - p0.Y;
            // 用 Z 符号位 trick 代替四个方向的独立分支，避免平行情况的多次判断
            float[] p = { -dx, dx, -dy, dy };
            float[] q = { p0.X - rect.Left, rect.Right - p0.X, p0.Y - rect.Top, rect.Bottom - p0.Y };

            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0f)
                {
                    // 线段平行于某条边
                    if (q[i] < 0f) return false; // 完全在矩形外侧
                    continue;
                }
                float t = q[i] / p[i];
                if (p[i] < 0)
                    tMin = tMin < t ? t : tMin;  // 进入点
                else
                    tMax = tMax > t ? t : tMax;  // 离开点

                if (tMin > tMax) return false;
            }

            return true;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 激光起点 = 弹幕中心
            Vector2 start = Projectile.Center;
            // 激光长度与 PreDraw 中的 scale.X 保持一致
            float beamLength = Projectile.scale * 800f;
            // 激光方向 = velocity 归一化
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            // 激光终点 = 起点 + 方向 × 长度
            Vector2 end = start + dir * beamLength;

            // 激光本身有厚度（CurrentWidth），把目标矩形各方向扩张半个厚度，等价于加粗激光
            int halfThickness = (int)(CurrentWidth * Projectile.scale * 0.5f);
            Rectangle expandedTarget = new Rectangle(
                targetHitbox.X - halfThickness,
                targetHitbox.Y - halfThickness,
                targetHitbox.Width + halfThickness * 2,
                targetHitbox.Height + halfThickness * 2
            );
            // 用线段-矩形相交判断
            if (LineIntersectsRect(start, end, expandedTarget))
                return true;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            if (!ReachMaxWidth)
            {
                CurrentWidth += 2.5f; // 👈 降低增量（原0.3→0.05），扩张变慢
                if (CurrentWidth >= MaxWidth)
                {
                    CurrentWidth = MaxWidth;
                    ReachMaxWidth = true; // 👈 只有达到最大宽度后才标记“可以收缩”
                }
            }
            // 闭合阶段（达到最大宽度后开始收缩）
            else
            {
                CurrentWidth -= 2.5f; // 👈 降低减量（原0.3→0.05），闭合变慢
                if (CurrentWidth <= 0)
                {
                    Projectile.Kill();
                }
            }
            Vector2 scale = new Vector2(Projectile.scale * 800f, Projectile.scale * CurrentWidth);
            Vector2 origin = new Vector2(0, texture.Height / 2f);   
            float rotation = Projectile.rotation;
            Vector2 baseScale = new Vector2(Projectile.scale * 800f, Projectile.scale * CurrentWidth);
            // ------------------- 新增：光晕层绘制 -------------------
            // 光晕参数（可自由调整）
            Color glowColor = Color.LimeGreen * 0.35f; // 光晕颜色（半透明）
            glowColor.A = 0; // 强制无Alpha遮挡
            Vector2 glowScale = baseScale * 1.4f; // 光晕比原弹幕大40%（可调整倍数）

            // 绘制光晕（先画，作为底层）
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition, // 位置与弹幕对齐
                null,
                glowColor,
                rotation,
                origin,
                glowScale,
                SpriteEffects.None,
                0f
            );
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
                    scale * factor,
                    SpriteEffects.None,
                    0f
                );
            }
            return false;
        }
    }
}
