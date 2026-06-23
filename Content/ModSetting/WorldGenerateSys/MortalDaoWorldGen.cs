using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
namespace MortalDao.Content.ModSetting.WorldGenerateSys
{
    public class MortalDaoWorldGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            //1.生成箱子
            //插入生成地牢箱子后
            int dungeonChestsIndex = tasks.FindIndex(p => p.Name == "Dungeon Chests");
            if (dungeonChestsIndex != -1)
            {
                // 权重给50，和原版Dungeon Chests的量级一致，不会明显影响进度条比例
                tasks.Insert(dungeonChestsIndex + 1, new BloodyChestPass("Generating Bloody Chest!", 50f));
            }
            else
            {
                // 极端情况：原版步骤被其他Mod移除，退而求其次插在地牢生成后
                int dungeonIndex = tasks.FindIndex(p => p.Name == "Dungeon");
                if (dungeonIndex != -1)
                {
                    tasks.Insert(dungeonIndex + 1, new BloodyChestPass("Generating Bloody Chest!", 50f));
                    Mod.Logger.Warn("Can't find #Dungeon Chests# Progress,Delay Generation after Dungeon generation,error may occur!");
                }
                else
                {
                    Mod.Logger.Warn("Can't find any Progress to generate dungeon,Skip Spawning Bloody Chest!");
                }
            }

            //2.生成沙漠贼窝
            int pyramidsIndex = tasks.FindIndex(p => p.Name == "Pyramids");
            if (pyramidsIndex != -1)
            {
                // 权重30和原版地表结构量级匹配，不影响进度条显示
                tasks.Insert(pyramidsIndex + 1, new ThiefCampPass("Mortal Dao: Generating Thief's Desert Camp", 30f));
            }
            else
            {
                // 兜底：找不到金字塔则插在地表箱子生成前
                int surfaceChestsIndex = tasks.FindIndex(p => p.Name == "Surface Chests");
                if (surfaceChestsIndex != -1)
                {
                    tasks.Insert(surfaceChestsIndex, new ThiefCampPass("Mortal Dao: Generating Thief's Desert Camp", 30f));
                    Mod.Logger.Warn("Can't find #Pyramids# Progress, Generate Thief Camp before Surface Chests.");
                }
                else
                {
                    // 二次兜底：插在生物群系生成完成后
                    int biomesIndex = tasks.FindIndex(p => p.Name == "Biomes");
                    if (biomesIndex != -1)
                    {
                        tasks.Insert(biomesIndex + 1, new ThiefCampPass("Mortal Dao: Generating Thief's Desert Camp", 30f));
                        Mod.Logger.Warn("Can't find #Pyramids# or #Surface Chests# Progress, Generate Thief Camp after Biomes.");
                    }
                    else
                    {
                        Mod.Logger.Warn("Can't find suitable position to generate Thief Camp, skip this generation.");
                    }
                }
            }
        }
    }
}
