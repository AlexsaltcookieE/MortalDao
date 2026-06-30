using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj
{
    public class JiuTongProj: ModProjectile
    {

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = true; // 不撞墙，避免回收前意外消失
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1; // 无限穿透，减速/悬停阶段可以多次命中玩家
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 0.5f;
            Projectile.timeLeft = 300; // 延长到4秒，足够覆盖整个流程（原200可能临界）
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
    }
}