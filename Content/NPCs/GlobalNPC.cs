using MortalDao.Content.Items.Specials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs
{ 

    public class BloodyKeyDrops : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.RedDevil && HasDefeatedTwoMechBosses())
            {
                if (Main.rand.Next(1,6)> 1)//1/5的概率掉落
                {
                    Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.ItemType<Bloody_key>());
                }
            }
        }

        private static bool HasDefeatedTwoMechBosses()
        {
            int defeatedCount = 0;
            if (NPC.downedMechBoss1) defeatedCount++;
            if (NPC.downedMechBoss2) defeatedCount++;
            if (NPC.downedMechBoss3) defeatedCount++;
            return defeatedCount >= 2;
        }
    }
}