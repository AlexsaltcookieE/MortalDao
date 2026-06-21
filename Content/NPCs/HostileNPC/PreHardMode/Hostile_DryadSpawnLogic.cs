using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.HostileNPC.PreHardMode
{
    // 全局图块类，用于处理砍树事件
    public class DryadSpawnGlobalTile : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            // 检查是否是树木相关的图块
            if (!fail && !effectOnly && IsTreeTile(type))
            {
                // 10%概率生成
                if (Main.rand.Next(100) < 10)
                {
                    // 获取玩家位置（最近的玩家）
                    Player player = Main.player[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];

                    // 确保玩家存在且活跃
                    if (player.active)
                    {
                        if(Main.netMode != NetmodeID.Server)
                        {
                            foreach(NPC npc in Main.npc)
                            {
                                if(npc.active && npc.type == ModContent.NPCType<Hostile_Dryad>())
                                {
                                    return;
                                }
                            }
                            // 在砍树位置生成敌对树妖
                            int npcIndex = NPC.NewNPC(
                                Player.GetSource_NaturalSpawn(),
                                (int)(i * 16 + 8),  // X坐标（图块中心）
                                (int)(j * 16 + 8),  // Y坐标（图块中心）
                                ModContent.NPCType<Hostile_Dryad>()  // 敌对树妖NPC类型
                            );
                        }
                    }
                }
            }
        }

        // 判断是否为树木图块
        private bool IsTreeTile(int tileType)
        {
            return tileType == TileID.Trees ||           // 普通树
                   tileType == TileID.PalmTree ||       // 棕榈树
                   tileType == TileID.PineTree ||       // 松树
                   tileType == TileID.MushroomTrees ||  // 蘑菇树
                   tileType == TileID.VanityTreeSakura || // 樱花树
                   tileType == TileID.VanityTreeYellowWillow; // 黄柳树
        }
    }
}