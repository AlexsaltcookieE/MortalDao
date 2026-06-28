using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class PeachWoodBladeHitbox : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // 透明

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.knockBack = 5f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2; // 每帧刷新
            Projectile.hide = true;  // 不绘制
            Projectile.penetrate = 3;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
