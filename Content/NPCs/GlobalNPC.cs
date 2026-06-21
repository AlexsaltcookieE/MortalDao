using MortalDao.Content.Items.Marterials;
using MortalDao.Content.Items.Specials;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs
{
    [Autoload(true)]
    public class GlobalNPCLoot : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
        }
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.RedDevil)
            {
                npcLoot.Add(ItemDropRule.ByCondition(new Conditions.BeatAnyMechBoss(),ModContent.ItemType<Bloody_key>(),10,1,1));
            }
            if (npc.type == NPCID.BigPantlessSkeleton || npc.type == NPCID.SmallPantlessSkeleton || npc.type == NPCID.BigMisassembledSkeleton || npc.type == NPCID.SmallMisassembledSkeleton
                || npc.type == NPCID.BigHeadacheSkeleton || npc.type == NPCID.SmallHeadacheSkeleton || npc.type == NPCID.BigSkeleton || npc.type == NPCID.Skeleton
                || npc.type == NPCID.UndeadMiner || npc.type == NPCID.Tim || npc.type == NPCID.ArmoredSkeleton || npc.type == NPCID.SkeletonArcher || npc.type == NPCID.UndeadViking
                || npc.type == NPCID.RuneWizard || npc.type == NPCID.HeadacheSkeleton || npc.type == NPCID.MisassembledSkeleton || npc.type == NPCID.PantlessSkeleton || npc.type == NPCID.SkeletonAlien
                || npc.type == NPCID.SkeletonAstonaut)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<resentment>(), 10, 1, 1));
            }
        }
    }
}