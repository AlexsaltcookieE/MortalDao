using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs
{
    public class BreakLeg : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] =  true;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<BreakLegPlayer>().HasBreakLeg = true;
        }
    }
    public class BreakLegPlayer : ModPlayer
    {
        public bool HasBreakLeg;

        public override void ResetEffects()
        {
            HasBreakLeg = false;
        }
        public override void PostUpdate()
        {
            if (HasBreakLeg)
            {
                // 1. 彻底禁止移动：将X轴速度归零
                // 注意：如果不希望完全锁死（比如允许掉落），可以只限制 X 速度      
                Player.wingTime = 0;
                Player.velocity.X = 0;
                
            }
        }

    }
}
