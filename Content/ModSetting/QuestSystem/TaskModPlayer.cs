using MortalDao.Content.ModSetting.QuestSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MortalDao.Content.ModSetting.QuestSystem.GlobalTaskSystem;

namespace MortalDao.Content.ModSetting.QuestSystem
{
    public class TaskModPlayer : ModPlayer
    {
        // 存储击杀数量
        public Dictionary<int, int> KillCounts = new();

        // 存储已完成的任务
        public HashSet<int> CompletedTasks = new();
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // 向服务器请求同步所有任务状态
                ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
                packet.Write((byte)GlobalTaskSystem.PacketType.SyncPlayerTask);
                packet.Write(Player.whoAmI);
                packet.Send();
            }
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // 同步任务进度
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<MortalDao>().GetPacket();
                packet.Write((byte)PacketType.SyncPlayerTask);
                packet.Write(Player.whoAmI);
                packet.Write(CompletedTasks.Count);
                foreach (var taskId in CompletedTasks)
                    packet.Write(taskId);
                packet.Send(toWho, fromWho);
            }
        }
    }
}