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

        }
        // 可选：当玩家死亡或退出时重置
        public override void OnRespawn()
        {
            slimeSummonCycle = 0;
        }
    }
}