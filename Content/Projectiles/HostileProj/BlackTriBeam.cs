using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;

namespace MortalDao.Content.Projectiles.HostileProj
{
    public class BlackTriBeam : ModProjectile
    {
        // 用一张1x1或16x16的白色像素纹理即可，用颜色控制最终效果

        // 激光最大长度（像素）
        private const float MaxBeamLength = 600f;

        // 尾部宽度（细端）
        private const float TailWidth = 4f;

        // 头部宽度（粗端）
        private const float HeadWidth = 24f;

        // 生长速度（每帧增加的长度）
        private const float GrowSpeed = 30f;

        public float beamLength = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 激光生长逻辑
            if (beamLength < MaxBeamLength)
            {
                beamLength += GrowSpeed;
                if (beamLength > MaxBeamLength)
                    beamLength = MaxBeamLength;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float rotation = direction.ToRotation();

            // 把激光分成若干段，每段宽度不同，从尾到头变粗
            int segments = 30;
            float segmentLength = beamLength / segments;

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1); // 0~1
                float width = MathHelper.Lerp(TailWidth, HeadWidth, t);
                // 当前段的中心位置
                Vector2 segmentCenter =
                    Projectile.Center + direction * (t * beamLength);
                // 用白色纹理 + 黑色颜色，用半透明模拟"能量感"
                Color beamColor = Color.Black * 0.85f;

                Main.spriteBatch.Draw(
                    tex,
                    segmentCenter - Main.screenPosition,
                    null,
                    beamColor,
                    rotation,
                    new Vector2(tex.Width / 2f, tex.Height / 2f),
                    new Vector2(segmentLength / tex.Width, width / tex.Height),
                    SpriteEffects.None,
                    0f
                );
            }

            return false;
        }
    }
}