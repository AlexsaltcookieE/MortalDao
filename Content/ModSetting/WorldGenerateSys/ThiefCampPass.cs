using MortalDao.Content.ModSetting.WorldData;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace MortalDao.Content.ModSetting.WorldGenerateSys
{
    public class ThiefCampPass : GenPass
    {
        public ThiefCampPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Thief's Camp";

            // 房子尺寸
            int houseWidth = 24;
            int houseHeight = 10;

            for (int attempt = 0; attempt < 1000; attempt++)//尝试找到沙漠表面 次数1000
            {
                int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);//减去海洋的沙漠
                int y = 0;
                bool foundDesertSurface = false;
                for (int checkY = (int)Main.worldSurface - 50; checkY > 50; checkY--)
                {
                    if (WorldGen.InAPlaceWithWind(x, checkY, 10, 10) &&
                        Main.tile[x, checkY].HasTile &&
                        (Main.tile[x, checkY].TileType == TileID.Sand ||
                         Main.tile[x, checkY].TileType == TileID.HardenedSand ||
                         Main.tile[x, checkY].TileType == TileID.Sandstone))
                    {
                        // 确保上方是空气（表面）
                        if (!Main.tile[x, checkY - 1].HasTile)
                        {
                            y = checkY;
                            foundDesertSurface = true;
                            break;
                        }
                    }
                }
                if (!foundDesertSurface) continue;

                bool spaceClear = true;
                for (int i = 0; i < houseWidth; i++)
                {
                    for (int j = 1; j <= houseHeight + 1; j++) // +1 计算合适位置
                    {
                        if (Main.tile[x + i, y - j].HasTile)
                        {
                            spaceClear = false;
                            break;
                        }
                    }
                    if (!spaceClear) break;
                }

                if (!spaceClear) continue;
                BuildCamp(x, y, houseWidth, houseHeight);
                break;
            }
        }
        private void BuildCamp(int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {   
                WorldGen.PlaceTile(x + i,y,TileID.Sand, true, false, -1, 0);
                for (int j = 0; j < height; j++)
                {
                    //木梁
                    if (i == 0 || i == width - 1 || i == 2 || i == width - 1 - 2)
                    {
                        WorldGen.PlaceTile(x + i, y - j - 1, TileID.WoodenBeam);
                    }
                    //木头
                    if ((i == 1 && j == height - 1) || (i == width - 1 - 1 && j == height - 1))
                    {
                        WorldGen.PlaceTile(x + i, y - j - 1, TileID.LivingWood);
                    }
                    //砂岩椅子
                    if ((i == 5 && j == 0) || (i == 18 && j == 0)|| (i == 6 && j == 0) || (i == 19 && j == 0))
                    {
                        WorldGen.PlaceTile(x + i, y - j - 1, TileID.SandstoneBrick);
                    }
                    if ((i == 5 && j == 1)|| (i == 19 && j == 1))
                    {
                        WorldGen.PlaceTile(x + i, y - j - 1, TileID.SandstoneBrick);
                    }
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //篝火
                    if (i == 12 && j == 0)
                    {
                        WorldGen.PlaceObject(x + i, y - j - 1, TileID.Campfire);
                    }
                    //旗帜
                    if ((i == 1 && j == height - 2) || (i == width - 1 - 1 && j == height - 2))
                    {
                        WorldGen.PlaceBanner(x + i, y - j - 1, TileID.Banners);
                    }
                }
            }
            ThiefCampWorldData.CampCenterX = x + width / 2;
            ThiefCampWorldData.CampCenterY = y - height / 2;
        }
    }
}
