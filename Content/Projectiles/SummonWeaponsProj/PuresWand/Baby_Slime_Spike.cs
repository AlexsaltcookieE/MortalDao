using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand
{
    public class Baby_Slime_Spike : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SpikedSlimeSpike;
        public override void SetStaticDefaults()
        {
            // 可选：设置原版弹幕的替代贴图或行为
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;      // 关键：设为友方
            Projectile.hostile = false;      // 关键：设为非敌方
            Projectile.DamageType = DamageClass.Summon; // 关键：设为召唤伤害
            Projectile.penetrate = 1;       // 击中敌人后消失
            Projectile.timeLeft = 120;       // 存活时间（2秒）
            Projectile.aiStyle = 1;          // 使用原版木箭的AI（直线飞行+重力）
            AIType = ProjectileID.WoodenArrowFriendly; // 模仿木箭的行为
            Projectile.ignoreWater = true;
        }

        // 可选：如果你想用原版尖刺的贴图，可以在这里绘制，或者直接在SetDefaults里用原版贴图
        // 如果需要使用原版尖刺的外观，通常不需要额外写Draw代码，只要ProjectileID对应即可，但这里我们是Mod弹幕
    }
}