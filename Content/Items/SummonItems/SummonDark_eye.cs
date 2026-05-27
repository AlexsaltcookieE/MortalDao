using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MortalDao.Content.NPCs.BOSS.DarkGaze;

namespace MortalDao.Content.Items.SummonItems
{
    public class SummonDark_eye : ModItem
    {
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
            return !NPC.AnyNPCs(ModContent.NPCType<DarkGaze>());
        }
        public override bool? UseItem(Player player)
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DarkGaze>());
            }
            return true;
        }
    }
}
