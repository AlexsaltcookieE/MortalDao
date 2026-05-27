using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Milion_Arrow : ModProjectile
    {
        private int ProjectileTimer;
        private const int WarningTime = 120;
        private Vector2 realStartPoint;
        public static Dictionary<int, (Vector2 Pos, Vector2 Dir)> ActiveWarnings = new Dictionary<int, (Vector2, Vector2)>();
        private bool PlaySound = false;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;

        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 800;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.velocity = Vector2.Zero;
        }
        public override void AI()
        {
            ProjectileTimer++;
            Projectile.netUpdate = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            if (ProjectileTimer <= WarningTime)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.alpha = (int)(100 + Math.Sin(ProjectileTimer * 0.2f) * 50);
                Vector2 dir = (Projectile.ai[0] != 0 || Projectile.ai[1] != 0) ?
                             new Vector2(Projectile.ai[0], Projectile.ai[1]).SafeNormalize(Vector2.UnitX) :
                             Vector2.UnitX;
                if (ActiveWarnings.ContainsKey(Projectile.whoAmI))
                {
                    ActiveWarnings[Projectile.whoAmI] = (Projectile.Center, dir);
                }
                else
                {
                    ActiveWarnings.Add(Projectile.whoAmI, (Projectile.Center, dir));
                }
                return;
            }
            if (ActiveWarnings.ContainsKey(Projectile.whoAmI))
            {
                ActiveWarnings.Remove(Projectile.whoAmI);
            }
            if (ProjectileTimer >= WarningTime + 1)
            {
                if (!PlaySound)
                {
                    foreach (Player p in Main.player)
                    {
                        if (p.active || !p.dead)
                        {
                            SoundEngine.PlaySound(SoundID.Item5, p.Center);
                            PlaySound = true;
                        }
                    }
                }
                float speedX = Projectile.ai[0]; // 假设你在创建弹幕时把速度X存进去了
                float speedY = Projectile.ai[1]; // 假设你在创建弹幕时把速度Y存进去了
                if (speedX == 0 && speedY == 0) speedX = 10f; // 默认速度
                Projectile.velocity = new Vector2(speedX, speedY);
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);
        }
        public override void OnKill(int timeLeft)
        {
            if (ActiveWarnings.ContainsKey(Projectile.whoAmI))
            {
                ActiveWarnings.Remove(Projectile.whoAmI);
            }
            base.OnKill(timeLeft);
        }
    } 
    
}

