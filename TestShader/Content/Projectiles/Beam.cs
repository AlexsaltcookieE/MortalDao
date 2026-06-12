using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestShader.Content.Projectiles
{
    public class Beam : ModProjectile
    {
        public override string Texture => "TestShader/Content/Textures/ScarletDevilStreak";
        private string noiseTexturePath = "TestShader/Content/Textures/BasicTrail";
        private float CurrentW = 0f;
        private const float W = 5.0f;
        private bool Dilate = false;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.damage = 999999;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 激光起点
            Vector2 start = Projectile.Center;
            // 激光方向（单位向量）
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            // 激光长度
            float laserLength = 2000f;
            // 终点
            Vector2 end = start + dir * laserLength;
            // 碰撞宽度（与当前绘制宽度一致）
            float width = CurrentW * Projectile.scale;
            float CollisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width,ref CollisionPoint);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中效果
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(
                    target.Center,
                    0, 0,
                    DustID.SolarFlare,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    100,
                    default,
                    1.5f
                );
            }
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (CurrentW <= W && !Dilate)
            {
                CurrentW += 0.5f;
                if (CurrentW >= W)
                {
                    CurrentW = W;
                    Dilate = true;
                }
            }
            else if (CurrentW > 0)
            {
                CurrentW -= 0.5f;
                if (CurrentW < 0)
                {
                    CurrentW = 0;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D maskTex = ModContent.Request<Texture2D>(noiseTexturePath).Value;
            Effect effect = TestShader.ImpFlameEffect?.Value;
            SpriteBatch sb = Main.spriteBatch;

            //拉伸
            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            float laserLength = 2000f;
            float laserWidth = Projectile.scale;

            if (effect != null)
            {
                
                // 设参数
                effect.Parameters["_uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                effect.Parameters["maskSampler"]?.SetValue(maskTex);
                sb.End(); // 结束主批次
                sb.Begin(
                    SpriteSortMode.Immediate,
                    BlendState.Additive,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullCounterClockwise,
                    effect,                       // ← effect 交给 Begin
                    Main.GameViewMatrix.TransformationMatrix
                );
                sb.Draw(tex,
                    Projectile.Center - Main.screenPosition,
                    null, Color.White, Projectile.rotation,
                    origin, new Vector2(laserLength / tex.Width, CurrentW),
                    SpriteEffects.None, 0f);

                sb.End();
                // 恢复主 SpriteBatch
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // 无 shader 时走默认绘制
                sb.Draw(ModContent.Request<Texture2D>(Texture).Value,
                    Projectile.Center - Main.screenPosition,
                    null, lightColor, Projectile.rotation,
                    ModContent.Request<Texture2D>(Texture).Value.Size() * 0.5f,
                    Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}