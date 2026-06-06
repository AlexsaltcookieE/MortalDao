using MortalDao.Content.Items.Placeables.Ores;
using MortalDao.Content.ModSetting.QuestSystem;
using MortalDao.Content.NPCs.TownNPCs;
using MortalDao.Content.Tiles.Ore;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace MortalDao.Content.ModSetting.QuestSystem
{
    public enum TaskStatus
    {
        NotStarted,
        Accepted,
        Completed,
    }
    public class PlayerTaskData
    {
        public int TaskID;
        public TaskStatus status;
    }
    public static class GlobalTaskSystem
    {
        public static Dictionary<int, TaskStatus> GlobalTasks = new();
        public static Dictionary<int, TaskDefinition> AllTasks = new();
        public static Dictionary<int, bool> GlobalTaskStates = new(); // true = 完成

        // ⛔ 删掉 static 构造函数里的 InitTask1()
        // static GlobalTaskSystem() { InitTask1(); }  ← 删掉这一行

        /// <summary>
        /// 只注册“任务定义”（在 PostSetupContent 调一次）
        /// </summary>
        //public static void RegisterAllTaskDefinitions()
        //{
        //    // 先确保容器存在（只注册一次，不覆盖）
        //    if (AllTasks == null) AllTasks = new();
        //    // 我们不需要清空 AllTasks，它是“定义”，跨世界也成立

        //    RegisterTaskDef_1();
        //}

        /// <summary>
        /// 只清“世界运行时状态”，不清定义
        /// </summary>
        public static void ResetWorldState()
        {
            GlobalTasks?.Clear();
            GlobalTaskStates?.Clear();
        }

        // --------------- 任务 1 的定义（纯定义，不做状态假设） ---------------
        public static void RegisterAllTaskDefinitions()
        {
            if (AllTasks.ContainsKey(1)) return;

            RegisterTask(new TaskDefinition
            {
                ID = 1,
                Name = "小二的家",
                Requirements =
            {
                new CheckNPCHouseRequirement { NpcID = ModContent.NPCType<The_Second>() }
            },
                Rewards =
            {
                new TaskReward { ItemID = ModContent.ItemType<DownGrade_LingShi>(), Stack = 10 }
            }
            });
        }
        // RegisterTask（只管定义，不改运行状态）
        public static void RegisterTask(TaskDefinition task)
        {
            AllTasks[task.ID] = task;
            // ⚠ 注意：这里“不”去碰 GlobalTaskStates
        }

        public static void CompleteTask(int taskId)
        {
            GlobalTaskStates[taskId] = true;
        }
        public static void SyncTask(int taskID, TaskStatus status)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                GlobalTasks[taskID] = status;
                return;
            }
            // 客户端 → 服务器
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
                packet.Write((byte)PacketType.SyncTask);
                packet.Write(taskID);
                packet.Write((byte)status);
                packet.Send();
            }
            //修复：客户端也要更新本地状态
            GlobalTasks[taskID] = status;
        }
        public static void BroadcastTask(int taskID, TaskStatus status)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                GlobalTasks[taskID] = status;
                return;
            }
            // 服务器 → 所有客户端
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
                packet.Write((byte)PacketType.SyncTask);
                packet.Write(taskID);
                packet.Write((byte)status);
                packet.Send(-1, -1); // 发送给所有客户端
            }
        }
        public enum PacketType : byte
        {
            SyncTask = 0,
            SyncDialogue = 1,
            RequestCompleteTask = 2,
            TaskCompleted = 3,
            SyncPlayerTask = 4
        }
        public static void RequestCompleteTask(int taskId, Player player)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                ServerCompleteTask(taskId, player.whoAmI);
                GiveRewardsToAllPlayers(taskId);
                return;
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
                packet.Write((byte)PacketType.RequestCompleteTask);
                packet.Write(taskId);
                packet.Send();
                GiveRewardsToAllPlayers(taskId);
            }
        }
        private static void SendTaskFailMessage(int toWhoAmI)
        {
            if (Main.netMode != NetmodeID.Server) return;

            // 给指定玩家发一条提示
            NetMessage.SendData(
                MessageID.SmartTextMessage,
                toWhoAmI,
                -1,
                NetworkText.FromLiteral("§c任务条件未满足！"),
                255,
                255, 100, 100
            );
        }

        // 服务器处理交任务
        public static void ServerCompleteTask(int taskId, int requesterWhoAmI)
        {
            if (Main.netMode != NetmodeID.Server) return;
            if (!AllTasks.ContainsKey(taskId))
            {
                SendTaskFailMessage(requesterWhoAmI);
                return;
            }
            if (GlobalTaskStates.TryGetValue(taskId, out bool done) && done)
            {
                SendTaskFailMessage(requesterWhoAmI);
                return;
            }
            if (GlobalTasks.ContainsKey(taskId))
                GlobalTasks[taskId] = TaskStatus.Completed;
            GlobalTaskStates[taskId] = true;
            BroadcastTaskComplete(taskId);
            SyncTask(taskId, TaskStatus.Completed);
        }

        private static void BroadcastTaskComplete(int taskId)
        {
            ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
            packet.Write((byte)PacketType.TaskCompleted);
            packet.Write(taskId);
            packet.Send(-1, -1); // 广播
        }
        private static void GiveRewardsToAllPlayers(int taskId)
        {
            var task = AllTasks[taskId];

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active) continue;

                var modPlayer = player.GetModPlayer<TaskModPlayer>();

                // ✅ 防止重复添加
                if (modPlayer.CompletedTasks.Contains(taskId))
                    continue;

                foreach (var reward in task.Rewards)
                {
                    player.QuickSpawnItem(
                        new EntitySource_Misc("Task:" + taskId),
                        reward.ItemID,
                        reward.Stack
                    );
                }

                modPlayer.CompletedTasks.Add(taskId);
            }
        }
        public static bool CanCompleteTask(int taskId)
        {
            if (!GlobalTasks.TryGetValue(taskId, out var status))
                return false;
            if (status != TaskStatus.Accepted)
                return false;
            if (GlobalTaskStates.TryGetValue(taskId, out bool done) && done)
                return false;
            if (AllTasks.TryGetValue(taskId, out var def))
                return def.CheckCompletion(Main.LocalPlayer);
            return false;
        }
    }
}