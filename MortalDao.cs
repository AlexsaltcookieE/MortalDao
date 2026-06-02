
using Microsoft.Xna.Framework;
using MortalDao.Content.ModSetting.QuestSystem;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MortalDao.Content.ModSetting.QuestSystem.GlobalTaskSystem;

namespace MortalDao
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class MortalDao : Mod
	{
        public override void Load()
        {
            //GlobalTaskSystem.Load();   // ✅ 加这一行
            //Main.NewText("✅ MortalDao 模组已加载");
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            //PacketType type = (PacketType)reader.ReadByte();
            byte raw = reader.ReadByte();
            ModContent.GetInstance<MortalDao>().Logger.Info($"[HandlePacket] raw={raw}");
            PacketType type = (PacketType)raw;
            switch (type)
            {
                case PacketType.SyncTask:
                    int taskID = reader.ReadInt32();
                    TaskStatus status = (TaskStatus)reader.ReadByte();
                    GlobalTaskSystem.GlobalTasks[taskID] = status;
                    // 如果是服务器收到，广播给其他人
                    if (Main.netMode == NetmodeID.Server)
                    {
                        GlobalTaskSystem.BroadcastTask(taskID, status);
                    }
                    break;
                case PacketType.RequestCompleteTask:
                    int taskId = reader.ReadInt32();
                    GlobalTaskSystem.ServerCompleteTask(taskId, whoAmI);
                    break;

                case PacketType.TaskCompleted:
                    int completedTaskId = reader.ReadInt32();
                    GlobalTaskSystem.GlobalTaskStates[completedTaskId] = true;

                    // 更新UI
                    Main.NewText($"任务 {GlobalTaskSystem.AllTasks[completedTaskId].Name} 已完成！", Color.Green);
                    break;
            }
        }

    }
}
