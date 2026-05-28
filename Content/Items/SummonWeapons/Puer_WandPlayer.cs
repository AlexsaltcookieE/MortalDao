using MortalDao.Content.Buffs.SummonWeaponsBuffs;
using MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content
{
    public class Puer_WandPlayer : ModPlayer
    {
        public int slimeSummonCycle = 0; // 0 = King, 1 = Queen, 2 = Vanilla Slime

        public override void ResetEffects()
        {
            // 可以在这里重置，但通常不需要每帧重置
        }
        public override void PostUpdate()
        {
            if (!Main.hasFocus) return;
            int buffType = ModContent.BuffType<RainBowSlime>();
            if (Player?.active != true || Player.dead ||
                !Player.HasBuff(buffType))
            {
                CleanupAllSlimes(Player);
            }
        }
        // 可选：当玩家死亡或退出时重置
        public override void OnRespawn()
        {
            slimeSummonCycle = 0;
        }
        private void CleanupAllSlimes(Player player)
        {
            int kingType = ModContent.ProjectileType<Baby_Slime_King>();
            int queenType = ModContent.ProjectileType<Baby_Slime_Queen>();
            int pureType = ProjectileID.BabySlime;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI) continue;

                if (p.type == kingType || p.type == queenType || p.type == pureType)
                {
                    p.Kill(); // Kill 会触发 Death 等正常流程；如果想静默消可以用 p.active = false
                }
            }
        }
    }
}