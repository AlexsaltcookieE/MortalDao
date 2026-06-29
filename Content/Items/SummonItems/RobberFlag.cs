using MortalDao.Content.ModSetting.Utilities;
using MortalDao.Content.NPCs.Attacks.RobberAttack;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.SummonItems
{
    public class RobberFlag : ModItem
    {
        private static LocalizedText GetSpawnInfo(string entryName) => MortalDaoUtils.GetText($"OnspawnMessage.{entryName}");
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.UseSound = SoundID.Roar;
            Item.rare = ItemRarityID.Quest;
            Item.value = Item.buyPrice(silver: 50);
        }
        public override bool CanUseItem(Player player)
        {
            return !RobberAttackEvent.EventActive;
        }
        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                RobberAttackEvent.EventActive = true;
                RobberAttackEvent.RemainingRobbers = 0;
                Main.NewText(GetSpawnInfo("Robber").Value, 255, 200, 50);
            }
            return true;
        }
    }
}
