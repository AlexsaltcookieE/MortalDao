using MortalDao.Content.NPCs.TownNPCs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.UI.The_Sencond
{
    public class TheSecondSpawn : ModSystem
    {
        public override void PostWorldGen()
        {
            //初始NPC
            SpawnThe_Second();
        }
        public void SpawnThe_Second()
        {
            bool npcExists = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<The_Second>())
                {
                    npcExists = true;
                    break;
                }
            }
            if (!npcExists)
            {
                int spawnX = Main.spawnTileX * 16 + 8; // 出生点X坐标
                int spawnY = Main.spawnTileY * 16 - 64; // 出生点Y坐标（向导上方）
                                                        // 修复参数：添加玩家索引（Main.myPlayer 表示本地玩家）
                IEntitySource source = new EntitySource_WorldGen("SpawnTheSecond");
                NPC.NewNPCDirect(
                    source,
                    spawnX,
                    spawnY,
                    ModContent.NPCType<The_Second>()
                );
            }
        }
    }
}
