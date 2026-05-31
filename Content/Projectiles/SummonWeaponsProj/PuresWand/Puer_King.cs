using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand
{
    public class Puer_King : ModProjectile
    {
        private bool Land = false;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 175;          // ★碰撞宽（你可视贴图大没关系，碰撞一般用小值）
            Projectile.height = 120;         // ★碰撞高
            Projectile.friendly = true;     // 玩家发射=友方
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged; // 或 Magic / Melee / Summon
            Projectile.penetrate = -1;        // 穿透次数（-1=无限穿透）
            Projectile.timeLeft = 360;      // 存活时间(tick) 6秒≈360
            Projectile.alpha = 0;
            Projectile.light = 0.35f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;  // 撞墙消失（false=穿墙）
            Projectile.extraUpdates = 0;
            Projectile.alpha = 255;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 1.5f;
            if (Projectile.velocity.Y > 0.1f && !Land)
            {
                Projectile.frameCounter++;
                if(Projectile.frameCounter > 5)
                {
                    Projectile.frameCounter = 0;
                    if (Projectile.alpha > 0)
                    {
                        Projectile.alpha -= 55; // 每帧减少透明度，直到完全不透明
                        if (Projectile.alpha < 0)
                            Projectile.alpha = 0;
                    }
                    if (Projectile.frame < 1)
                    {
                        Projectile.frame++;
                    }
                }
            }
        }

        // 命中判定后（可选：产生 dust 粒子）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Smoke,
                    Projectile.velocity.X * 0.2f,
                    Projectile.velocity.Y * 0.2f,
                    100,
                    default,
                    1.1f);
                d.noGravity = true;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 只有向下碰撞时才弹跳
            Land = true;
            Projectile.velocity = Vector2.Zero;
            if(Projectile.velocity.Y < 0.1f && Land)
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 55; // 每帧减少透明度，直到完全不透明
                    if (Projectile.alpha < 0)
                        Projectile.alpha = 0;
                }
                Projectile.frameCounter++;
                if(Projectile.frameCounter > 5)
                {
                    if (Projectile.frame < 5)
                    {
                        Projectile.frame++;
                        if(Projectile.frame == 5)
                        {
                            Projectile.Kill();
                        }
                    }
                }
            }
            return false;
        }
        public override void Kill(int timeLeft)
        {
            // 死亡特效（撞墙/超时）
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Water,
                    5,5, 50, default, 1f);
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Water,
                    -5, -5, 50, default, 1f);
            }
        }
    }
}