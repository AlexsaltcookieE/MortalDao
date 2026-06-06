using MortalDao.Content.Items.Marterials;
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
            //特殊
            //1.---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            if (npc.type == NPCID.RedDevil && HasDefeatedTwoMechBosses())
            {
                if (Main.rand.Next(1, 6) > 1)//1/5的概率掉落
                {
                    Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.ItemType<Bloody_key>());
                }
            }
            //材料
            //1.---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            if (npc.type == NPCID.BigPantlessSkeleton || npc.type == NPCID.SmallPantlessSkeleton || npc.type == NPCID.BigMisassembledSkeleton || npc.type == NPCID.SmallMisassembledSkeleton
                || npc.type == NPCID.BigHeadacheSkeleton || npc.type == NPCID.SmallHeadacheSkeleton || npc.type == NPCID.BigSkeleton || npc.type == NPCID.Skeleton
                || npc.type == NPCID.UndeadMiner || npc.type == NPCID.Tim || npc.type == NPCID.ArmoredSkeleton || npc.type == NPCID.SkeletonArcher || npc.type == NPCID.UndeadViking
                || npc.type == NPCID.RuneWizard || npc.type == NPCID.HeadacheSkeleton || npc.type == NPCID.MisassembledSkeleton || npc.type == NPCID.PantlessSkeleton || npc.type == NPCID.SkeletonAlien
                || npc.type == NPCID.SkeletonAstonaut) 
            {
                if(Main.rand.Next(1,30) == 1)//1/30的概率掉落
                {
                    Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.ItemType<resentment>());
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