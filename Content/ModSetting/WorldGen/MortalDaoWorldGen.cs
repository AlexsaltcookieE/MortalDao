using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Placeables.Ores;
using MortalDao.Content.Items.SummonItems;
using MortalDao.Content.ModSetting.UI;
using MortalDao.Content.Tiles;
using MortalDao.Content.Tiles.Ore;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.GameContent.UI;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MortalDao.Content.ModSetting
{
    public class MortalDaoGenarateSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)//世界生成
        {
            //插入生成矿石的命令
            int OresIndex = tasks.FindIndex(pass => pass.Name.Equals("Shinies", StringComparison.OrdinalIgnoreCase));
            if(OresIndex != -1)
            {
                tasks.Insert(OresIndex + 1, new PassLegacy("MortalDao Ores", GenerateCyan_Fe));
                tasks.Insert(OresIndex + 2, new PassLegacy("MortalDao Ores", GenerateDownGrade_LingShi));
            }
            int dungeonIndex = tasks.FindIndex(pass => pass.Name.Equals("dungeon",StringComparison.OrdinalIgnoreCase));
            if(dungeonIndex != -1)
            {
                tasks.Insert(dungeonIndex + 1, new PassLegacy("MortalDao Dungeon Chests", GenerateBloodyChestsInDungeon));
            }
        }
        public override void PostWorldGen()
        {
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    var t = Main.tile[i, j];
                    if (t.HasTile && t.TileType == ModContent.TileType<DownGradeLingShi>())
                    {
                        DownGradeLingShi.ApplyConnectFrame(i, j);
                        // 让区块重新承认帧变化（生成期单人可用）
                        WorldGen.SquareTileFrame(i, j, true);
                    }
                }
            }
        }
        private void GenerateDownGrade_LingShi(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "生成一些带有仙气的矿石";
            int OreTileType = ModContent.TileType<DownGradeLingShi>();
            int OreVein = (int)(Main.maxTilesX * Main.maxTilesY * 0.00006f);//矿脉密度
            //矿脉生成层级
            int MinLayerY = (int)Main.rockLayer;
            int MaxLayerY = Main.maxTilesY - 200;
            //Gen
            for (int i = 0; i < OreVein; i++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(MinLayerY, MaxLayerY);
                double VeinThickness = WorldGen.genRand.Next(4, 8);
                int VeinLength = WorldGen.genRand.Next(3, 7);
                WorldGen.TileRunner(x, y, VeinThickness, VeinLength, OreTileType);
            }
        }
        private void GenerateCyan_Fe(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "生成一些带有仙气的矿石";
            int OreTileType = ModContent.TileType<Cyan_Fe>();
            int OreVein = (int)(Main.maxTilesX * Main.maxTilesY * 0.00004f);//矿脉密度
            //矿脉生成层级
            int MinLayerY = (int)Main.rockLayer;
            int MaxLayerY = Main.maxTilesY - 200;
            //Gen
            for (int i = 0; i < OreVein; i++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(MinLayerY, MaxLayerY);
                double VeinThickness = WorldGen.genRand.Next(4, 8);
                int VeinLength = WorldGen.genRand.Next(3, 7);
                WorldGen.TileRunner(x, y, VeinThickness, VeinLength, OreTileType);
            }
        }
        
        private void GenerateBloodyChestsInDungeon(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "封印魔尊的爪牙";
            int tileType = ModContent.TileType<Bloody_Chest>();
            int targetCount = 2; // 目标生成数量，根据需要调整
            int placed = 0;
            int scanned = 0;
            List<Point> dungeonChestPositions = [];

            // 先收集地牢中已有箱子的坐标
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null) continue;

                int x = chest.x;
                int y = chest.y;
                scanned++;

                if (!Main.wallDungeon[Main.tile[x, y].WallType]) continue;
                dungeonChestPositions.Add(new Point(x, y));
            }

            // 打乱顺序后替换前 targetCount 个，避免总是固定位置
            for (int i = dungeonChestPositions.Count - 1; i > 0; i--)
            {
                int j = WorldGen.genRand.Next(i + 1);
                (dungeonChestPositions[i], dungeonChestPositions[j]) = (dungeonChestPositions[j], dungeonChestPositions[i]);
            }

            for (int i = 0; i < dungeonChestPositions.Count && placed < targetCount; i++)
            {
                int x = dungeonChestPositions[i].X;
                int y = dungeonChestPositions[i].Y;
                bool valid = true;
                for (int dx = 0; dx < 2; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        Tile tile = Main.tile[x + dx, y + dy];
                        if (!tile.HasTile)
                        {
                            valid = false;
                            break;
                        }

                        tile.TileType = (ushort)tileType;
                        tile.TileFrameX = (short)(36 + dx * 18);
                        tile.TileFrameY = (short)(dy * 18);
                    }
                    if (!valid) break;
                }

                if (valid)
                {
                    placed++;
                }
            }
            int BloodChestIndex = Chest.FindChest(dungeonChestPositions[0].X, dungeonChestPositions[0].Y);
            Chest BChest = Main.chest[BloodChestIndex];
            if (BloodChestIndex >= -1)
            {
                Chest chest = Main.chest[BloodChestIndex];
                for (int k = 0; k < Chest.maxItems; k++)
                {
                    chest.item[k].TurnToAir();
                }
            }
            BChest.item[0].SetDefaults(ModContent.ItemType<SummonDark_eye>());
            Mod.Logger.Info($"[MortalDao] Dungeon bloody chest gen: placed {placed}/{targetCount}, scanned {scanned} chests");
        }
    }
}