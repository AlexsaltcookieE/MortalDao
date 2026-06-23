using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class AxeRobber : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 56;
            NPC.scale = 1.4f;
            NPC.damage = 15;
            NPC.lifeMax = 100;
            NPC.defense = 5;
            NPC.knockBackResist = 1f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;      // "噗"的肉感受击声
            NPC.DeathSound = SoundID.NPCDeath2;   // 僵尸倒地死亡声
        }
        private bool HasWallInFront()
        {
            // 获取NPC脚部的位置（用于检测地面障碍）
            int direction = NPC.direction;

            // 检测点1：NPC脚部前方一格
            Vector2 checkPos1 = new Vector2(
                NPC.Center.X + (direction * (NPC.width / 2 + 4)),
                NPC.Bottom.Y - 2
            );

            // 检测点2：NPC腰部高度前方一格（防止低矮障碍）
            Vector2 checkPos2 = new Vector2(
                NPC.Center.X + (direction * (NPC.width / 2 + 4)),
                NPC.Center.Y
            );

            // 转换为图块坐标
            Point tilePos1 = checkPos1.ToTileCoordinates();
            Point tilePos2 = checkPos2.ToTileCoordinates();

            // 检查这两个位置是否有实心方块
            Tile tile1 = Framing.GetTileSafely(tilePos1.X, tilePos1.Y);
            Tile tile2 = Framing.GetTileSafely(tilePos2.X, tilePos2.Y);

            bool solid1 = tile1.HasTile && Main.tileSolid[tile1.TileType];
            bool solid2 = tile2.HasTile && Main.tileSolid[tile2.TileType];

            // 如果任一位置有墙，返回true
            return solid1 || solid2;
        }
        public override void AI()
        {
            //寻找玩家逻辑
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1) return;
            Player target = Main.player[targetID];

            float Distance = Vector2.Distance(target.Center, NPC.Center);
            if(Distance < 150)
            {
                //以很快的速度奔来
            }
        }
    }
}
