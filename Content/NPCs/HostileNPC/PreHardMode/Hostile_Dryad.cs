using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace MortalDao.Content.NPCs.HostileNPC.PreHardMode
{
    public class Hostile_Dryad : ModNPC
    {
        private int _frameIndex = 0;
        private int NPCtime;
        private int _jumpCooldown = 0;  // 跳跃冷却，防止连续跳跃
        private const int JUMP_COOLDOWN_MAX = 30;  // 跳跃冷却时间（帧）
        private const float JUMP_VELOCITY = -6f;  // 跳跃速度（负值向上）
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 15;
        }
        public override void SetDefaults()
        {
            NPC.width = 22;
            NPC.height = 46;
            NPC.scale = 1.4f;
            NPC.damage = 15;
            NPC.lifeMax = 100;
            NPC.defense = 5;
            NPC.knockBackResist = 1.5f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;      // "噗"的肉感受击声
            NPC.DeathSound = SoundID.NPCDeath2;   // 僵尸倒地死亡声
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            for (int i = 0; i < 2; i++)
            {
                Gore.NewGore(
                    NPC.GetSource_Death(),
                    NPC.position,
                    NPC.velocity,
                    GoreID.TreeLeaf_Normal
                );
            }
        }
        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            for (int i = 0; i < 10; i++)
            {
                Gore.NewGore(
                    NPC.GetSource_Death(),
                    NPC.position,
                    NPC.velocity,
                    GoreID.TreeLeaf_Normal
                    ,Scale:2f
                );
            }
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
        private bool IsOnGround()
        {
            // 检查NPC脚底下是否有实心方块或平台
            Vector2 footPos = new Vector2(NPC.Center.X, NPC.Bottom.Y + 2);
            Point tilePos = footPos.ToTileCoordinates();
            Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

            return (tile.HasTile && (Main.tileSolid[tile.TileType] || tile.TileType == TileID.Platforms));
        }

        public override void AI()
        {
            if (_jumpCooldown > 0)
            {
                _jumpCooldown--;
            }
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1) return;
            Player target = Main.player[targetID];
            NPC.direction = target.Center.X - NPC.Center.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            //弹幕
            float DISTANCE = Vector2.Distance(NPC.Center, target.Center);
            if(DISTANCE < 1200f)
            {
                NPCtime++;
                if (DISTANCE < 150)
                {
                    Vector2 EscapeDir = NPC.Center - target.Center;
                    float slowMoveSpeed = MathHelper.Clamp(EscapeDir.X * 1f, -2f, 2f);
                    NPC.direction = target.Center.X - NPC.Center.X > 0 ? -1 : 1;
                    NPC.spriteDirection = NPC.direction;
                    NPC.velocity.X = slowMoveSpeed;
                    if (IsOnGround() && _jumpCooldown <= 0 && HasWallInFront() && Math.Abs(NPC.velocity.X) > 0.5f)
                    {
                        // 执行跳跃
                        NPC.velocity.Y = JUMP_VELOCITY;
                        _jumpCooldown = JUMP_COOLDOWN_MAX;
                    }
                }
                else if(NPCtime > 80)
                {
                    NPC.velocity = Vector2.Zero;
                    NPCtime = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 shootDirection = (target.Center - NPC.Center).RotatedByRandom(MathHelper.ToRadians(15));
                        shootDirection.Normalize();
                        shootDirection *= 6f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<Projectiles.HostileProj.HostileLeafShot>(), 15, 0f);
                    }
                }
            }
            //帧图逻辑
            NPC.frameCounter++;
            if (NPC.frameCounter > 5)
            {
                NPC.frameCounter = 0;
                if (NPC.velocity == Vector2.Zero)
                {
                    _frameIndex = 0;
                }
                else if (Math.Abs(NPC.velocity.Y) > Math.Abs(NPC.velocity.X))
                {
                    _frameIndex = 1;
                }
                else
                {
                    _frameIndex++;
                    if (_frameIndex > 14)
                        _frameIndex = 2;
                }
                NPC.frame.Y = _frameIndex * 46;
            }
        }
    }

}
