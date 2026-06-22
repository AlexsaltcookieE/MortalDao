using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    // 必须继承 ModProjectile 才能作为泰拉瑞亚弹幕
    public class YueMaidenSwordBlade : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10; // 弹幕碰撞宽度（需和贴图匹配）
            Projectile.height = 10; // 弹幕碰撞高度
            Projectile.aiStyle = -1; // 禁用默认AI（如果是近战剑刃，通常手动控制运动）
            Projectile.friendly = true; // 友方弹幕（对敌人造成伤害）
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 不穿透
            Projectile.tileCollide = true; // 不穿墙
            Projectile.timeLeft = 60; // 存在时间（帧）
            Projectile.ignoreWater = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.DD2_SkeletonHurt, Projectile.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                // 生成8-12个爆发性粒子，向四周扩散
                for (int i = 0; i < Main.rand.Next(8, 13); i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f); // 更快的扩散速度
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch,
                        dustVel.X, dustVel.Y, 80, Color.White, Main.rand.NextFloat(3f, 4f));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.alpha = 30; // 比飞行粒子更亮，突出命中反馈
                }
            }
        }

        // ===== 消失时（超时/撞墙）的粒子效果 =====
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                // 生成6-10个粒子，强度略低于命中特效
                for (int i = 0; i < Main.rand.Next(6, 11); i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch,
                        dustVel.X, dustVel.Y, 90, Color.White, Main.rand.NextFloat(3f, 4f));
                    dust.noGravity = true;
                    dust.noLight = true;
                    dust.alpha = 40;
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("MortalDao/Assets/Sounds/Items/YueMaidenBlade"), Projectile.Center);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(5,8))
                {
                    // 粒子位置在弹幕中心附近随机偏移
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                    // 粒子速度：跟随弹幕飞行方向+随机扩散
                    Vector2 dustVel = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);

                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.WhiteTorch, dustVel.X, dustVel.Y,
                        100, Color.White, Main.rand.NextFloat(1.5f, 2f));
                    dust.noGravity = true; // 关闭重力，粒子不会下落
                    dust.noLight = true;   // 不受环境光影响，始终保持纯白
                    dust.alpha = 50;       // 透明度，数值越低越亮（0=完全不透明，255=全透明）
                }
            }
        }

        // 核心：重写 PreDraw 控制绘制效果
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取弹幕贴图（需在 ModContent 中注册，路径对应你的贴图位置）
            Texture2D texture = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/MelleeWeaponsProj/YueMaidenSwordBlade").Value;
            // 计算绘制位置（弹幕中心对齐，避免偏移）
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            // 计算绘制原点（贴图中心，和弹幕碰撞盒中心一致）
            Vector2 origin = texture.Size() / 2f;
            // 保存原始混合状态，绘制后恢复（避免影响其他弹幕）
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive, // 混合模式：Alpha 混合（默认，适合大多数情况）
                SamplerState.AnisotropicClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );
            // 绘制贴图：用灰度颜色叠加，实现黑白效果
            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                Color.White,
                Projectile.rotation, // 旋转角度（剑刃通常需要跟随挥舞旋转）
                origin,
                Projectile.scale, // 缩放
                SpriteEffects.None,
                0f
            );

            // 恢复默认绘制状态
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );
            return false; // 返回 false 阻止默认绘制（避免重复绘制原始彩色贴图）
        }
    }
}