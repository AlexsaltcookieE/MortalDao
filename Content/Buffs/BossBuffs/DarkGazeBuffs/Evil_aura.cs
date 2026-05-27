using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.BossBuffs.DarkGazeBuffs    
{
    public class Evil_aura : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] =  false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<Evil_auraPlayer>().HasEvilAura = true;
        }
    }
    public class Evil_auraPlayer : ModPlayer
    {
        public bool HasEvilAura;
        public bool JustDrankHealingPotion;
        private int healingPotionFlagTimer;
        public override void ResetEffects()
        {
            HasEvilAura = false;
            JustDrankHealingPotion = false;
        }
        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (healValue <= 0)
            {
                return;
            }
            // Mark "just drank healing potion" window.
            JustDrankHealingPotion = true;
            healingPotionFlagTimer = 30; // 0.5s window at 60 FPS
            if (HasEvilAura)
            {
                // EvilAura active: block all life restoration from consumed healing items.
                healValue = 0;
            }
        }
        public override void PostUpdate()
        {
            if (healingPotionFlagTimer > 0)
            {
                healingPotionFlagTimer--;
                JustDrankHealingPotion = true;
            }
        }
    }
}
