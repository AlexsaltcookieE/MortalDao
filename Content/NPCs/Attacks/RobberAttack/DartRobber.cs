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
        public override void OnKill()
        {
            RobberAttackEvent.RemainingRobbers = RobberAttackEvent.RemainingRobbers + 1;
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
            }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1.0f)), Mod.Find<ModGore>("DartRobberGore1").Type, NPC.scale * Main.rand.NextFloat(0.8f, 1.0f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -2f)), Mod.Find<ModGore>("DartRobberGore2").Type, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-5f, -1.5f)), Mod.Find<ModGore>("DartRobberGore3").Type, NPC.scale * Main.rand.NextFloat(0.85f, 1.05f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-3.5f, 3.5f), Main.rand.NextFloat(-7f, -2.5f)), Mod.Find<ModGore>("DartRobberGore4").Type, NPC.scale * Main.rand.NextFloat(0.95f, 1.15f));
            
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            // ✅ 受伤时生成一些金色的尘埃
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }

            Player target = Main.player[targetID];
            NPC.direction = NPC.Center.X >  target.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            switch (currentState)
            {
                case AttackState.Normal:
                    // 正常状态：播放第1-3帧（索引0-2）
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 6) // 每10帧切换一次
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 4) // 超过第4帧时回到第1帧
                        {
                            NPC.frame.Y = 0;
                        }
                    }
                    break;
                case AttackState.JumpingUp:
                    NPC.frame.Y = 0;
                    break;
                case AttackState.Darts:
                    NPC.frame.Y = 0;
                    break;
            }
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // 设置NPC的帧数
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
            NPC.width = 30;
            NPC.height = 44;
            NPC.scale = 1.2f;
            NPC.damage = 15;
            NPC.lifeMax = 40;
            NPC.defense = 2;
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
        private bool HasStepInFront()
        {
            int direction = NPC.direction;
            float checkX = NPC.Center.X + direction * (NPC.width / 2f + 4f);

            // 从脚底往上数 3 格（含脚那一格），看连续 solid 有几格
            int wallHeight = 0;
            for (int yOffset = 0; yOffset < 3; yOffset++)
            {
                Vector2 checkPos = new Vector2(checkX, NPC.Bottom.Y - 2 - yOffset * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    wallHeight++;
                else
                    break; // 遇到空气就停，只统计"连续"的
            }

            // ≥ 2 格高才算墙，需要跳；1 格高当台阶，直接走过去
            return wallHeight >= 1;
        }
        private bool HasWallInFront()
        {
            int direction = NPC.direction;
            float checkX = NPC.Center.X + direction * (NPC.width / 2f + 4f);

            // 从脚底往上数 3 格（含脚那一格），看连续 solid 有几格
            int wallHeight = 0;
            for (int yOffset = 0; yOffset < 3; yOffset++)
            {
                Vector2 checkPos = new Vector2(checkX, NPC.Bottom.Y - 2 - yOffset * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    wallHeight++;
                else
                    break; // 遇到空气就停，只统计"连续"的
            }

            // ≥ 2 格高才算墙，需要跳；1 格高当台阶，直接走过去
            return wallHeight >= 2;
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

                float moveSpeed = 3f;
                NPC.velocity.X = dirToPlayer.X * moveSpeed;

                NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                if (HasWallInFront() && NPC.velocity.Y == 0)
                {
                    NPC.velocity.Y = -9f;
                }
                if (HasStepInFront() && NPC.velocity.Y == 0)
                {
                    NPC.velocity.Y = -4f;
                }
            }
            else
            {
                NPC.velocity.X *= 0.9f;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height / 2f);

            // 计算NPC中心的偏移量（避免碰撞箱和视觉大小不一致的问题）
            Vector2 centerOffset = NPC.Center - NPC.position;

            // 画残影：i=0是最新的（离本体最近），i越大越旧
            if (currentState ==AttackState.JumpingUp || currentState == AttackState.Darts)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    if (NPC.oldPos[i] == Vector2.Zero) continue; // 跳过未初始化的帧

                    // ✅ 进度计算：最新的残影最亮最大，最旧的几乎透明
                    float progress = 1f - (float)i / NPC.oldPos.Length;

                    // 残影位置：oldPos是左上角坐标，加centerOffset得到中心，再转成屏幕坐标
                    Vector2 drawPos = NPC.oldPos[i] + centerOffset - screenPos;

                    Color color = drawColor * progress * 0.7f; // 透明度随progress衰减

                    spriteBatch.Draw(
                        texture,
                        drawPos,
                        NPC.frame,
                        color,
                        NPC.oldRot[i], // 用我们手动滚存的旋转，和位置完全匹配
                        origin,
                        // 缩放：越新的残影越大，越旧的越小
                        NPC.scale,
                        effects,
                        0f
                    );
                }
            }

            // 画本体
            Vector2 centerDraw = NPC.Center - screenPos;
            spriteBatch.Draw(
                texture,
                centerDraw,
                NPC.frame,
                drawColor,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }
    }
}