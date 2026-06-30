using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj
{
    public class LightningBallProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // 2 = 记录位置 + 旋转 + 精灵方向，闪电一般用 2 或 0 都行
            ProjectileID.Sets.TrailingMode[Type] = 2;
            // 轨迹缓存长度，timeLeft 是 16，这里设 10~16 都行
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;   // 你在常量里再补一个 LIFETIME = 16
            Projectile.alpha = 0;
            Projectile.light = 0.6f;   // 闪电自己发光
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 如果你准备了闪电贴图就用 TextureAssets.Projectile[Type].Value
            // 闪电一般用纯白长条渐变图（中间亮两端暗）就够了
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle rect = new Rectangle(0, 0, tex.Width, frameHeight);
            Vector2 origin = new Vector2(tex.Width / 2f, frameHeight / 2f);

            int trailLen = ProjectileID.Sets.TrailCacheLength[Type];

            for (int i = 0; i < trailLen; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;

                float factor = 1f - (float)i / trailLen;          // 1→0 新→旧
                Vector2 drawPos = Projectile.oldPos[i]
                                + Projectile.Size / 2f
                                - Main.screenPosition;

                // —— 外层辉光（加色，A=0 是关键）——
                Color glow = new Color(255, 140, 100, 0);   // 蓝紫闪电光
                glow.A = 0;
                float glowScale = MathHelper.Lerp(1.6f, 0.2f, 1 - factor); // 尾部收窄
                Main.EntitySpriteDraw(tex, drawPos, rect,
                    glow * factor,
                    Projectile.oldRot[i],
                    origin,
                    new Vector2(glowScale, 1f),   // X 拉宽 = 辉光，Y=1 沿速度方向
                    SpriteEffects.None, 0);

                // —— 内层核心（白偏黄）——
                Color core = new Color(255, 240, 200, 0);
                core.A = 0;
                float coreScale = MathHelper.Lerp(0.6f, 0.05f, 1 - factor);
                Main.EntitySpriteDraw(tex, drawPos, rect,
                    core * factor * 1.2f,
                    Projectile.oldRot[i],
                    origin,
                    new Vector2(coreScale, 1f),
                    SpriteEffects.None, 0);
            }

            return false;   // 阻止原版默认绘制
        }
    }
}