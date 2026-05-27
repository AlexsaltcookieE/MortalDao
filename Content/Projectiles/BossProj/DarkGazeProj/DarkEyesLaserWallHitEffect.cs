using MortalDao.Content.Buffs.BossBuffs.DarkGazeBuffs;
using MortalDao.Content.NPCs.BOSS.DarkGaze;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.DarkGazeProj
{
    public class DarkEyesLaserWallHitEffect : GlobalProjectile
    {
        public override bool InstancePerEntity => false;
        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<DarkGaze>()))
            {
                return;
            }
            bool isLaserWall = projectile.type == ModContent.ProjectileType<Dead_ray>() &&
                               (projectile.ai[1] == 2f || projectile.ai[1] == 3f);
            bool isSideDemonSickle = projectile.type == ProjectileID.DemonSickle && projectile.hostile;
            if (isLaserWall || isSideDemonSickle)
            {
                // 5s debuff when player is hit by laserWall package projectiles.
                target.AddBuff(ModContent.BuffType<Evil_aura>(), 60 * 5);
            }
        }
    }
}
