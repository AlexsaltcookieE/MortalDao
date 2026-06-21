using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{

    public class WeirdBoulderProj : ModProjectile
    {
        private const float Gravity = 0.25f;
        private const float Max_Fall_Speed = 20f;
        private const float Rotation_Speed = 0.15f;
        private bool HasBounce;
        public override string Texture => "MortalDao/Content/Items/SummonItems/Weird_Boulder";
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 2f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.knockBack = 8f;
            Projectile.damage = 60;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            if (Projectile.velocity.Y < Max_Fall_Speed)
            {
                Projectile.velocity.Y += Gravity;
            }
            Projectile.rotation += Projectile.velocity.X * Rotation_Speed;
            Projectile.velocity.X *= 0.99f;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.position,
                        Projectile.width,
                        Projectile.height,
                        DustID.MagicMirror,
                        Scale: 1.5f
                    );
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
            }
            if (Projectile.velocity.Y == 0)
            {
                SoundEngine.PlaySound(SoundID.Item53, Projectile.position);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            if (Math.Abs(oldVelocity.X) > 2f)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            }
            if (Math.Abs(oldVelocity.Y) > 2f)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            if (HasBounce)
            {   if (Main.netMode != NetmodeID.Server)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center,Vector2.Zero, ModContent.ProjectileType<GoldElementWarning>(), 0, 0, Main.myPlayer);
                }
                Projectile.Kill();
            }
            HasBounce = true;
            return false;
        }
    } 
}
