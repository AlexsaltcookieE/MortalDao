using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.QuestSystem
{
    public class TaskDefinition
    {
        public int ID;
        public string Name;
        public string Description;
        public List<TaskRequirement> Requirements = new();
        public List<TaskReward> Rewards = new();

        // 检查是否完成
        public bool CheckCompletion(Player player)
        {
            foreach (var req in Requirements)
            {
                if (!req.IsMet(player)) return false;
            }
            return true;
        }
    }

    public abstract class TaskRequirement
    {
        public abstract bool IsMet(Player player);
    }

    // 击杀怪物需求
    //public class KillNPCRequirement : TaskRequirement
    //{
    //    public int NpcID;
    //    public int RequiredCount;

    //    public override bool IsMet(Player player)
    //    {
    //        // 使用 ModPlayer 存储击杀数
    //        var modPlayer = player.GetModPlayer<TaskModPlayer>();
    //        return modPlayer.KillCounts.TryGetValue(NpcID, out int count)
    //               && count >= RequiredCount;
    //    }
    //}
    // 收集物品需求
    public class CollectItemRequirement : TaskRequirement
    {
        public int ItemID;
        public int RequiredStack;

        public override bool IsMet(Player player)
        {
            return player.CountItem(ItemID) >= RequiredStack;
        }
    }
    //NPC住房需求
    public class CheckNPCHouseRequirement : TaskRequirement
    {
        public int NpcID; // 要检测的NPC类型ID
        public override bool IsMet(Player player)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == NpcID)
                {
                    if(npc.homeless == false)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            Main.NewText("NPC NOT FOUND");
            return false;
        }
    }
    // 任务奖励
    public class TaskReward
    {
        public int ItemID;
        public int Stack;
    }
}