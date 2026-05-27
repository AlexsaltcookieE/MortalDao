using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.SummonWeaponsBuffs
{
    public class RainBowSlime : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;//不保存
            Main.buffNoTimeDisplay[Type] = true;//不显示持续时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 180;
        }
    }
}