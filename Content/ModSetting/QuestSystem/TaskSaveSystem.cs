using MortalDao.Content.ModSetting.QuestSystem;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class TaskWorldSaveSystem : ModSystem
{
    public override void PostSetupContent()
    {
        GlobalTaskSystem.RegisterAllTaskDefinitions();
    }

    public override void ClearWorld()
    {
        GlobalTaskSystem.ResetWorldState();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        // --- 保存完成列表 ---
        var done = new List<int>();
        foreach (var kv in GlobalTaskSystem.GlobalTaskStates)
        {
            if (kv.Value) done.Add(kv.Key);
        }
        if (done.Count > 0)
            tag["DoneTasks"] = done;

        // --- 保存过程状态（Accepted / Completed 等）---
        if (GlobalTaskSystem.GlobalTasks.Count > 0)
        {
            var ids = new List<int>();
            var sts = new List<byte>();
            foreach (var kv in GlobalTaskSystem.GlobalTasks)
            {
                ids.Add(kv.Key);
                sts.Add((byte)kv.Value);
            }
            tag["TaskIDs"] = ids;
            tag["TaskStatuses"] = sts;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        // ① 确保清空（ClearWorld 应该已做，但双重保险）
        GlobalTaskSystem.ResetWorldState();

        // ② 确保所有"已注册定义"至少有一个默认条目
        //    这样即使存档里没记录这个ID，它也不会凭空变成 null 状态
        foreach (var id in GlobalTaskSystem.AllTasks.Keys)
        {
            // 默认：未完成、无过程状态
            if (!GlobalTaskSystem.GlobalTaskStates.ContainsKey(id))
                GlobalTaskSystem.GlobalTaskStates[id] = false;
        }

        // ③ 恢复完成标记
        if (tag.TryGet<List<int>>("DoneTasks", out var done))
        {
            foreach (int id in done)
                GlobalTaskSystem.GlobalTaskStates[id] = true;
        }

        // ④ 恢复过程状态（Accepted / Completed）
        if (tag.TryGet<List<int>>("TaskIDs", out var taskIds) &&
            tag.TryGet<List<byte>>("TaskStatuses", out var taskStatuses))
        {
            int n = Math.Min(taskIds.Count, taskStatuses.Count);
            for (int i = 0; i < n; i++)
                GlobalTaskSystem.GlobalTasks[taskIds[i]] = (TaskStatus)taskStatuses[i];
        }
    }
}
//using MortalDao.Content.ModSetting.QuestSystem;
//using System.Collections.Generic;
//using Terraria.IO;
//using Terraria.ModLoader;
//using Terraria.ModLoader.IO;

//namespace MortalDao.Content.ModSetting.QuestSystem
//{
//    public class TaskWorldSaveSystem : ModSystem
//    {
//        // 世界加载完内容后：注册定义
//        public override void PostSetupContent()
//        {
//            GlobalTaskSystem.RegisterAllTaskDefinitions();
//        }

//        // 新世界 / 切世界 / 重载：先清状态
//        public override void ClearWorld()
//        {
//            GlobalTaskSystem.ResetWorldState();
//        }

//        // ✅ 保存：以 AllTasks 的 key 集合为基准，不会漏
//        public override void SaveWorldData(TagCompound tag)
//        {
//            // 已完成列表（最简可靠）
//            var done = new List<int>();
//            foreach (var id in GlobalTaskSystem.AllTasks.Keys)
//            {
//                if (GlobalTaskSystem.GlobalTaskStates.TryGetValue(id, out bool v) && v)
//                    done.Add(id);
//            }
//            if (done.Count > 0)
//                tag["DoneTasks"] = done;
//            //新增：保存 GlobalTasks（每个玩家的任务状态）
//            var taskIds = new List<int>();
//            var taskStatuses = new List<byte>();
//            foreach (var kv in GlobalTaskSystem.GlobalTasks)
//            {
//                taskIds.Add(kv.Key);
//                taskStatuses.Add((byte)kv.Value);
//            }
//            if (taskIds.Count > 0)
//            {
//                tag["TaskIDs"] = taskIds;
//                tag["TaskStatuses"] = taskStatuses;
//            }
//            // （可选）如果你将来还要保存 Accepted/NotStarted 这类“过程状态”，
//            // 也建议用 AllTasks.Keys 作基准，别用 GlobalTasks 的现存 key 作基准
//        }

//        // ✅ 加载：先确保容器存在，再还原
//        public override void LoadWorldData(TagCompound tag)
//        {
//            // 此时 PostSetupContent 已经跑过了 ⇒ AllTasks 一定有定义
//            // ✅ 先确保字典里有这个 Key
//            if (!GlobalTaskSystem.GlobalTaskStates.ContainsKey(1))
//                GlobalTaskSystem.GlobalTaskStates[1] = false;
//            // 先给所有任务一个“默认未完成”状态（干净起步）
//            foreach (var id in GlobalTaskSystem.AllTasks.Keys)
//                GlobalTaskSystem.GlobalTaskStates[id] = false;

//            // 再把存档里“已完成”的标回来
//            if (tag.TryGet<List<int>>("DoneTasks", out var done))
//            {
//                foreach (int id in done)
//                {
//                    if (GlobalTaskSystem.AllTasks.ContainsKey(id))
//                        GlobalTaskSystem.GlobalTaskStates[id] = true;
//                }
//            }
//            if (tag.TryGet<List<int>>("TaskIDs", out var taskIds) && tag.TryGet<List<byte>>("TaskStatuses", out var taskStatuses))
//            {
//                for (int i = 0; i < taskIds.Count; i++)
//                {
//                    GlobalTaskSystem.GlobalTasks[taskIds[i]] = (TaskStatus)taskStatuses[i];
//                }
//            }
//            foreach (var kv in GlobalTaskSystem.GlobalTaskStates)
//                ModContent.GetInstance<MortalDao>().Logger.Info($"[Task] id={kv.Key} done={kv.Value}");
//        }

//    }
