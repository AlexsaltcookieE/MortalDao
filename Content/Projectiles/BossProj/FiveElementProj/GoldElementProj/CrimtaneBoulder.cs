using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj;
using Terraria.DataStructures;

namespace YourModName.Projectiles
{
    public class CrimtaneBoulder : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Boulder;
        // 配置参数

        private const float GRAVITY = 0.25f;
        private const float MAX_FALL_SPEED = 20f;
        private const int BOUNCE_DAMAGE_REDUCTION = 10;
        private const float ROTATION_SPEED = 0.15f;
        private const float Spreate_SPEED = 10f;
        private int bounceCount = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 600; // 10秒后消失
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            // 物理属性
            Projectile.knockBack = 8f;
            Projectile.damage = 60;
            // 视觉
            Projectile.scale = 1.2f;
        }
        
        public override void AI()
        {
            // 重力模拟
            if (Projectile.velocity.Y < MAX_FALL_SPEED)
            {
                Projectile.velocity.Y += GRAVITY;
            }
            // 旋转效果
            Projectile.rotation += Projectile.velocity.X * ROTATION_SPEED;
            // 水平阻力
            Projectile.velocity.X *= 0.99f;

            // 生成灰尘轨迹
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.Blood,
                        Scale: 1.5f
                    );
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
            }
            // 落地检测
            if (Projectile.velocity.Y == 0)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            // 播放撞击音效
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            // 生成撞击粒子
            // 水平反弹（逐渐减弱）
            if (Math.Abs(oldVelocity.X) > 2f)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            }
            // 垂直反弹（逐渐减弱）
            if (Math.Abs(oldVelocity.Y) > 2f)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            // 每次反弹减少伤害
            Projectile.damage = Math.Max(10, Projectile.damage - BOUNCE_DAMAGE_REDUCTION);
            bounceCount++;
            if(bounceCount >= 6)
            {
                Projectile.Kill();
            }
            Main.NewText(bounceCount);
            return false; // 不销毁弹幕
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCHit41, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            // 死亡时生成碎石效果
            if(Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 7; i++)
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.Crimstone,
                        Scale: 1.5f
                    );
                    dust.velocity = new Vector2(
                        Main.rand.NextFloat(-3f, 3f),
                        Main.rand.NextFloat(-3f, 0f)
                    );
                    dust.noGravity = false;
                }
                ShootArrows();
            }
            // 播放破碎音效
            SoundEngine.PlaySound(SoundID.Item50, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制带阴影的巨石
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // 绘制阴影
            Vector2 shadowOffset = new Vector2(3, 3);
            Color shadowColor = new Color(0, 0, 0, 100);
            Main.EntitySpriteDraw(
                texture,
                Projectile.position - Main.screenPosition + shadowOffset,
                null,
                shadowColor,
                Projectile.rotation,
                texture.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // 绘制主纹理
            Main.EntitySpriteDraw(
                texture,
                Projectile.position - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
        private void ShootArrows()
        {
            // 箭矢伤害（可以是巨石伤害的50%）
            int arrowDamage = (int)(Projectile.damage * 0.5f);

            // 1. 向上发射
            Vector2 upVelocity = new Vector2(0, -Spreate_SPEED);
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                upVelocity,
                ModContent.ProjectileType<CrimtaneOreProj>(),  // 使用敌方的木箭
                arrowDamage,
                2f,  // 击退力
                Projectile.owner
            );

            // 2. 向左上发射（X负方向，Y正方向）
            Vector2 leftDownVelocity = new Vector2(-Spreate_SPEED, -Spreate_SPEED);
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                leftDownVelocity,
                ModContent.ProjectileType<CrimtaneOreProj>(),
                arrowDamage,
                2f,
                Projectile.owner
            );

            // 3. 向右上发射（X正方向，Y正方向）
            Vector2 rightDownVelocity = new Vector2(Spreate_SPEED, -Spreate_SPEED);
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                rightDownVelocity,
                ModContent.ProjectileType<CrimtaneOreProj>(),
                arrowDamage,
                2f,
                Projectile.owner
            );

            // 播放弓箭发射音效
            SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
        }
    }
}