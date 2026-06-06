using MortalDao.Content.ModSetting.QuestSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class TaskWorldSaveSystem : ModSystem
{
    private static HashSet<int> _deliveredTasks = new HashSet<int>();
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
        // 1. 保存已完成任务列表（GlobalTaskStates → DoneTasks）
        var doneTasks = new List<int>();
        foreach (var kv in GlobalTaskSystem.GlobalTaskStates)
        {
            if (kv.Value) doneTasks.Add(kv.Key);
        }
        if (doneTasks.Count > 0)
            tag["DoneTasks"] = doneTasks;

        // 2. 保存已交付任务（无关紧要，保留即可）
        tag["DeliveredTasks"] = _deliveredTasks.ToList();

        // 3. 保存任务过程状态（GlobalTasks → TaskIDs + TaskStatuses）
        if (GlobalTaskSystem.GlobalTasks.Count > 0)
        {
            var taskIds = new List<int>(GlobalTaskSystem.GlobalTasks.Keys);
            var taskStatuses = GlobalTaskSystem.GlobalTasks.Values
                .Select(status => (byte)status)
                .ToList();
            tag["TaskIDs"] = taskIds;
            tag["TaskStatuses"] = taskStatuses;
        }
    }
    //public override void SaveWorldData(TagCompound tag)
    //{
    //    // --- 保存完成列表 ---
    //    var done = new List<int>();
    //    foreach (var kv in GlobalTaskSystem.GlobalTaskStates)
    //    {
    //        if (kv.Value) done.Add(kv.Key);
    //    }
    //    if (done.Count > 0)
    //        tag["DoneTasks"] = done;

    //    // --- 保存过程状态（Accepted / Completed 等）---
    //    if (GlobalTaskSystem.GlobalTasks.Count > 0)
    //    {
    //        var ids = new List<int>();
    //        var sts = new List<byte>();
    //        foreach (var kv in GlobalTaskSystem.GlobalTasks)
    //        {
    //            ids.Add(kv.Key);
    //            sts.Add((byte)kv.Value);
    //        }
    //        tag["TaskIDs"] = ids;
    //        tag["TaskStatuses"] = sts;
    //    }
    //    //----------------------------------------
    //    var doneTasks = new List<int>();
    //    foreach (var kv in GlobalTaskSystem.GlobalTaskStates)
    //    {
    //        if (kv.Value) // 如果任务已完成
    //            doneTasks.Add(kv.Key);
    //    }
    //    tag["DoneTasks"] = doneTasks;

    //    // 保存已交付的任务
    //    tag["DeliveredTasks"] = _deliveredTasks.ToList();

    //    // 保存过程状态
    //    tag["TaskIDs"] = GlobalTaskSystem.GlobalTasks.Keys.ToList();
    //    tag["TaskStatuses"] = GlobalTaskSystem.GlobalTasks.Values.Select(v => (byte)v).ToList();
    //}

    public override void LoadWorldData(TagCompound tag)
    {
        // 清空状态
        GlobalTaskSystem.ResetWorldState();
        _deliveredTasks.Clear();

        // 恢复完成标记
        if (tag.TryGet<List<int>>("DoneTasks", out var done))
        {
            foreach (int id in done)
                GlobalTaskSystem.GlobalTaskStates[id] = true;
        }

        // 恢复已交付标记
        if (tag.TryGet<List<int>>("DeliveredTasks", out var delivered))
        {
            foreach (int id in delivered)
                _deliveredTasks.Add(id);
        }

        // 恢复过程状态
        if (tag.TryGet<List<int>>("TaskIDs", out var taskIds) &&
            tag.TryGet<List<byte>>("TaskStatuses", out var taskStatuses))
        {
            int n = Math.Min(taskIds.Count, taskStatuses.Count);
            for (int i = 0; i < n; i++)
                GlobalTaskSystem.GlobalTasks[taskIds[i]] = (TaskStatus)taskStatuses[i];
        }

        // 强制同步：确保所有Completed状态都在GlobalTaskStates中
        foreach (var kv in GlobalTaskSystem.GlobalTasks)
        {
            if (kv.Value == TaskStatus.Completed)
                GlobalTaskSystem.GlobalTaskStates[kv.Key] = true;
        }
    }
}
    
