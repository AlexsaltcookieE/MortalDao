using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Ghost_Explosion : ModProjectile
    {
        // 你可以调整的属性
        private const int TotalFrames = 10; // 你的贴图有 10 帧
        private const int AnimationSpeed = 7; // 每 5 帧切换一次图片（数值越小越快）
        private bool CanDamage;

        public override void SetStaticDefaults()
        {
            // ✅ 告诉游戏这个弹幕的贴图有几帧
            Main.projFrames[Projectile.type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;  // 贴图的宽度
            Projectile.height = 50; // 贴图的高度
            Projectile.friendly = false; // 是否对玩家友好（敌人的弹幕设为false）
            Projectile.hostile = true;  // 是否对玩家敌对（敌人的弹幕设为true）
            Projectile.penetrate = 1;   // 能穿透几个敌人（1表示击中一个就消失）
            Projectile.timeLeft = 600;  // 存活时间（600帧 = 10秒）
            Projectile.alpha = 0;       // 透明度 (0=完全可见, 255=透明)
            Projectile.light = 0.5f;    // 发出的光亮度
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 是否撞墙消失（爆炸弹幕通常设为false，让它飞一会）        // 缩放比例
            Projectile.scale = 5f;
        }

        public override void AI()
        {
            if (++Projectile.frameCounter >= AnimationSpeed)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= TotalFrames)
                {
                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                    Projectile.frame = 6;
                }
            }
            if (!CanDamage)
            {
                if(Projectile.frame > 5)
                {
                    CanDamage = true;
                }
            }
            foreach (Player p in Main.player)
            {
                if (!p.active || p.dead) continue;
                if (Projectile.Hitbox.Intersects(p.Hitbox) && CanDamage)
                {
                    p.immuneTime = 0;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 2. 计算单帧的矩形区域 (因为你的贴图是10帧横排)
            int frameHeight = texture.Height / TotalFrames;
            Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            // 3. 🌟 计算原点：让原点位于当前帧的正中心
            // 这样旋转和绘制都会以爆炸中心为准
            Vector2 origin = sourceRectangle.Size() / 2f;

            // 4. 绘制
            // 注意：这里减去 Main.screenPosition 是为了将世界坐标转为屏幕坐标
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition, // 位置：弹幕中心
                sourceRectangle,                        // 源矩形：当前帧
                lightColor * ((255 - Projectile.alpha) / 255f), // 颜色叠加（处理透明度）
                Projectile.rotation,                     // 旋转角度
                origin,                                  // 🔴 原点：这里是关键！
                Projectile.scale,                       // 缩放
                SpriteEffects.None,                      // 特效
                0f                                       // 层级
            );

            return false; // 返回 false，因为我们手动绘制了，不需要默认绘制
        }
    }
}