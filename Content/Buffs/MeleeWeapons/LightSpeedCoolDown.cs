using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.MeleeWeapons
{
    public class LightSpeedCoolDown: ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<LightSpeedCoolDownPlayer>().HasLightSpeedCoolDown = true;
        }
    }
    public class LightSpeedCoolDownPlayer : ModPlayer
    {
        public bool HasLightSpeedCoolDown;
        public override void ResetEffects()
        {
            HasLightSpeedCoolDown = false;
        }
    }
}
