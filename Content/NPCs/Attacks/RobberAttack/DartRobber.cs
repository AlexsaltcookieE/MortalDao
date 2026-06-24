using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class DartRobber : ModNPC
    {
        private int jumpTimer = 0;
        private int StateTime = 0;
        private int ProjectileTime = 0;
        private int ProjectileCoolDown = 240;
        public override string Texture => "MortalDao/Content/NPCs/Attacks/RobberAttack/AxeRobber";
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (RobberAttackEvent.EventActive)
            {
                return 1f;
            }
            return 0;
        }
        public override void OnKill()
        {
            RobberAttackEvent.RemainingRobbers = RobberAttackEvent.RemainingRobbers - 1;
        }

        public override void SetStaticDefaults()
        {
            // ✅ 只需要设置轨迹长度即可，NPC的TrailingMode不会自动滚存，留着也没用可以删掉
            NPCID.Sets.TrailCacheLength[NPC.type] = 4;
        }
        private enum AttackState
        {
            Normal,          // 正常巡逻
            JumpingUp,       // 跳向天空
            Darts       // 梅花镖
        }

        private AttackState currentState = AttackState.Normal;
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
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
        }
        private void UpdateTrail()
        {
            // 滚存位置：把旧轨迹往后挪，最新位置放最前面（索引0）
            for (int i = NPC.oldPos.Length - 1; i > 0; i--)
            {
                NPC.oldPos[i] = NPC.oldPos[i - 1];
            }
            NPC.oldPos[0] = NPC.position; // 当前帧的左上角位置存到最前面

            // 滚存旋转：和位置逻辑完全一致
            for (int i = NPC.oldRot.Length - 1; i > 0; i--)
            {
                NPC.oldRot[i] = NPC.oldRot[i - 1];
            }
            NPC.oldRot[0] = NPC.rotation; // 当前帧的旋转存到最前面
        }
        private bool HasWallInFront()
        {
            int direction = NPC.direction;
            Vector2 checkPos1 = new Vector2(
                NPC.Center.X + (direction * (NPC.width / 2 + 4)),
                NPC.Bottom.Y - 2
            );
            Vector2 checkPos2 = new Vector2(
                NPC.Center.X + (direction * (NPC.width / 2 + 4)),
                NPC.Center.Y
            );
            Point tilePos1 = checkPos1.ToTileCoordinates();
            Point tilePos2 = checkPos2.ToTileCoordinates();
            Tile tile1 = Framing.GetTileSafely(tilePos1.X, tilePos1.Y);
            Tile tile2 = Framing.GetTileSafely(tilePos2.X, tilePos2.Y);
            bool solid1 = tile1.HasTile && Main.tileSolid[tile1.TileType];
            bool solid2 = tile2.HasTile && Main.tileSolid[tile2.TileType];
            return solid1 || solid2;
        }

        public override void AI()
        {
            UpdateTrail();
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }

            Player target = Main.player[targetID];
            float distance = Vector2.Distance(NPC.Center, target.Center);
            switch (currentState)
            {
                case AttackState.Normal:
                    HandleNormalBehavior(target, distance);
                    break;
                case AttackState.JumpingUp:
                    HandleJumpUp(target);
                    break;
                case AttackState.Darts:
                    HandleDarts(target);
                    break;
                    
            }
        }
        private void HandleJumpUp(Player target)
        {
            jumpTimer++;
            // 上升阶段：垂直向上，水平轻微晃动
            NPC.velocity.X = (float)Math.Sin(jumpTimer * 0.1f) * 2f;
            // 当达到最高点开始下落时，切换到俯冲状态
            if (NPC.velocity.Y >= 0 && jumpTimer > 20)
            {
                currentState = AttackState.Darts;
                NPC.velocity.Y = 0; // 重置垂直速度
            }
        }
        private void HandleDarts(Player target)
        {
            if(StateTime < 40)
            {
                StateTime++;
                NPC.velocity *= 0;
                Vector2 PredictPos1 = target.Center + target.velocity * 5f;
                Vector2 PredictPos2 = target.Center + target.velocity * 10f;
                Vector2 PredictPos3 = target.Center + target.velocity * 15f;
                Vector2 Dir1 = Vector2.Normalize(PredictPos1 - NPC.Center);
                Vector2 Dir2 = Vector2.Normalize(PredictPos2 - NPC.Center);
                Vector2 Dir3 = Vector2.Normalize(PredictPos3 - NPC.Center);
                if(StateTime > 10 && ProjectileTime == 0)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Dir1 * 15, ModContent.ProjectileType<PlumBlossomDart>(), 10, 1f, Main.myPlayer);
                    ProjectileTime++;   
                }
                if(StateTime > 20 && ProjectileTime == 1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Dir2 * 15, ModContent.ProjectileType<PlumBlossomDart>(), 10, 1f, Main.myPlayer);
                    ProjectileTime++;
                }
                if (StateTime > 30 && ProjectileTime == 2)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Dir3 * 15, ModContent.ProjectileType<PlumBlossomDart>(), 10, 1f, Main.myPlayer);
                    ProjectileTime++;
                }
            }
            else
            {
                currentState = AttackState.Normal;
                StateTime = 0;
                ProjectileTime = 0;
            }

        }
        private void HandleNormalBehavior(Player target, float distance)
        {
            if (ProjectileCoolDown > 0)
            {
                ProjectileCoolDown--;
            }
            else
            {
                ProjectileCoolDown = 0;
            }
            if (distance < 600 && NPC.velocity.Y == 0 && ProjectileCoolDown == 0)
            {
                // 触发跳跃攻击
                currentState = AttackState.JumpingUp;
                jumpTimer = 0;
                NPC.velocity.Y = -14f; // 强力跳跃
                NPC.velocity.X = 0; // 停止水平移动
                // 播放音效
                ProjectileCoolDown = 240;
                SoundEngine.PlaySound(SoundID.Item82, NPC.position); // 使用合适的音效
                return;
            }
            // 原有的移动逻辑
            if (distance < 1500f)
            {
                // 检测脚下 Tile
                bool playerBelow = target.Center.Y > NPC.Center.Y + NPC.height;
                Point tileBelow = NPC.Bottom.ToTileCoordinates();
                Tile belowTile = Framing.GetTileSafely(tileBelow.X, tileBelow.Y);
                bool standingOnPlatform = belowTile.HasTile && TileID.Sets.Platforms[belowTile.TileType];
                if (standingOnPlatform && playerBelow)
                {
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = 4f; // 向下掉落速度（可调）
                }
                else
                {
                    NPC.noTileCollide = false;
                }
                Vector2 dirToPlayer = target.Center - NPC.Center;
                dirToPlayer.Normalize();

                float moveSpeed = 4f;
                NPC.velocity.X = dirToPlayer.X * moveSpeed;

                NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                if (HasWallInFront() && NPC.velocity.Y == 0)
                {
                    NPC.velocity.Y = -9f;
                }
            }
            else
            {
                NPC.velocity.X *= 0.9f;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = Color.White; // 橙色残影比白色更明显，方便调试

            // 计算NPC中心的偏移量（避免碰撞箱和视觉大小不一致的问题）
            Vector2 centerOffset = NPC.Center - NPC.position;

            // 画残影：i=0是最新的（离本体最近），i越大越旧
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                if (NPC.oldPos[i] == Vector2.Zero) continue; // 跳过未初始化的帧

                // ✅ 进度计算：最新的残影最亮最大，最旧的几乎透明
                float progress = 1f - (float)i / NPC.oldPos.Length;

                // 残影位置：oldPos是左上角坐标，加centerOffset得到中心，再转成屏幕坐标
                Vector2 drawPos = NPC.oldPos[i] + centerOffset - screenPos;

                Color color = trailColor * progress * 0.7f; // 透明度随progress衰减

                spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    color,
                    NPC.oldRot[i], // 用我们手动滚存的旋转，和位置完全匹配
                    origin,
                    // 缩放：越新的残影越大，越旧的越小
                    NPC.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // 画本体
            Vector2 centerDraw = NPC.Center - screenPos;
            spriteBatch.Draw(
                texture,
                centerDraw,
                null,
                drawColor,
                NPC.rotation,
                origin,
                NPC.scale,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}