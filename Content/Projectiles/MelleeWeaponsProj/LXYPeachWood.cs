using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class LXYPeachWood : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1; //1帧的弹幕
        }
        public override void SetDefaults() 
        {
            Projectile.damage = 20;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 3; //穿透
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;//穿墙？
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Player owner = Main.player[Projectile.owner];
                Projectile.ai[0] = owner.direction; // 仅在生成时记录一次方向
                Projectile.localAI[0] = 1f;
            }

            float dir = Projectile.ai[0];
            Projectile.spriteDirection = dir >= 0f ? 1 : -1;
            Projectile.direction = Projectile.spriteDirection;

            if (Projectile.velocity.LengthSquared() > 0.001f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                if (Projectile.spriteDirection == -1)
                {
                    Projectile.rotation += MathHelper.Pi;
                }
            }
            Projectile.ai[1]++;
            if (Projectile.ai[1] > 60) 
            {
                Projectile.Kill();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow,500);
        }
    }
}
