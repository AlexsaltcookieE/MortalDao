using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework;
using MortalDao.Content.Items.SummonItems;
using MortalDao.Content.Tiles;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MortalDao.Content.ModSetting.WorldGenerateSys
{
    public class BloodyChestPass : GenPass
    {
        public BloodyChestPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "封印魔尊的爪牙";
            //箱子
            int bloodyChestType = ModContent.TileType<Bloody_Chest>();
            //数量
            const int targetCount = 2; // 目标生成数量，可按需调整
            int placed = 0;
            int scanned = 0;
            List<Point> dungeonChestPositions = new List<Point>();
            //收集当前世界箱子数量
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null) continue;
                scanned++;
                if (!Main.wallDungeon[Main.tile[chest.x, chest.y].WallType])
                    continue;
                dungeonChestPositions.Add(new Point(chest.x, chest.y));
            }
            // 容错：没找到地牢箱子直接结束，避免后续空引用崩溃
            if (dungeonChestPositions.Count == 0)
            {
                ModLogger.Logger.Warn("Can't Find Any Gold Chest , Skip Blood Chest Generation");
                progress.Set(1f);
                return;
            }
            // 打乱顺序，避免固定替换前N个位置
            for (int i = dungeonChestPositions.Count - 1; i > 0; i--)
            {
                int j = WorldGen.genRand.Next(i + 1);
                (dungeonChestPositions[i], dungeonChestPositions[j]) = (dungeonChestPositions[j], dungeonChestPositions[i]);
            }
            //替换前targetCount个为血箱，并逐个填充物品
            for (int i = 0; i < dungeonChestPositions.Count && placed < targetCount; i++)
            {
                Point pos = dungeonChestPositions[i];
                int x = pos.X;
                int y = pos.Y;
                bool isValid = true;
                // 校验2x2箱子区域是否完整（防止地牢结构被破坏的情况）
                for (int dx = 0; dx < 2; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        Tile tile = Main.tile[x + dx, y + dy];
                        if (!tile.HasTile)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (!isValid) break;
                }
                for (int dx = 0; dx < 2; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        Tile tile = Main.tile[x + dx, y + dy];
                        tile.TileType = (ushort)bloodyChestType;
                        tile.TileFrameX = (short)(36 + dx * 18); // 18是单个瓦片像素宽
                        tile.TileFrameY = (short)(dy * 18);
                    }
                }
                int chestIdx = Chest.FindChest(x, y);
                if (chestIdx != -1) // 修复原逻辑判断错误：FindChest返回-1才是没找到
                {
                    Chest chest = Main.chest[chestIdx];
                    // 清空原有物品
                    for (int k = 0; k < Chest.maxItems; k++)
                        chest.item[k].TurnToAir();

                    // 放入召唤物，可调整位置和数量
                    chest.item[0].SetDefaults(ModContent.ItemType<SummonDark_eye>());
                    chest.name = "BloodyChest"; // 可选：自定义箱子名称
                    placed++;
                }
                else
                {
                    ModLogger.Logger.Warn($"Coordinate({x},{y})Can't find any chest,Skip filling Chest!");
                }
                progress.Set((float)(i + 1) / dungeonChestPositions.Count);
            }
            ModLogger.Logger.Info($"[MortalDao] 地牢血箱生成完成：成功放置{placed}/{targetCount}，共扫描{scanned}个箱子，候选地牢箱子{dungeonChestPositions.Count}个");
        }
        private Mod ModLogger => ModContent.GetInstance<MortalDao>();
    }
}
