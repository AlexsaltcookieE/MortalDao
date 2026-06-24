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
   public class AxeRobber : ModNPC
    {
        public override string Texture => "MortalDao/Content/NPCs/Attacks/RobberAttack/DartRobber";
        private int JumpStateDir;
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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1.0f)), Mod.Find<ModGore>("AxeRobberGore1").Type, NPC.scale * Main.rand.NextFloat(0.8f, 1.0f));
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -2f)), Mod.Find<ModGore>("AxeRobberGore2").Type, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-5f, -1.5f)), Mod.Find<ModGore>("AxeRobberGore3").Type, NPC.scale * Main.rand.NextFloat(0.85f, 1.05f));
        }
        //生成逻辑
        private enum AttackState
        {
            Normal,          // 正常巡逻
            JumpingUp,       // 跳向天空
            DivingDown       // 俯冲攻击
        }
        private AttackState currentState = AttackState.Normal;

        private int jumpTimer = 0;

        private Vector2 diveTarget; // 俯冲目标位置

        private float diveSpeed = 15f; // 俯冲速度

        private int DashCool = 120;

        private bool ThrownAxe = false;

        private int ProjectileCool = 240;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3; // 设置NPC的帧数
            // ✅ 只需要设置轨迹长度即可，NPC的TrailingMode不会自动滚存，留着也没用可以删掉
            NPCID.Sets.TrailCacheLength[NPC.type] = 4;
        }
        public override void SetDefaults()
        {
            
            NPC.width = 55;
            NPC.height = 50;
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

        #region 手动滚存轨迹（核心！必须每帧调用）
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
        #endregion

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
                    HandleJumpingUp(target);
                    break;

                case AttackState.DivingDown:
                    HandleDivingDown(target);
                    break;
            }
        }
        private void HandleNormalBehavior(Player target, float distance)
        {
            if(ProjectileCool < 240)
            {
                ProjectileCool++;
            }
            else
            {
                ProjectileCool = 0;
                ThrownAxe = false;
            }
            if(distance < 500)
            {
                // 撞击效果
                if (!ThrownAxe)
                {
                    Vector2 ActPos = new Vector2(NPC.Center.X, NPC.Center.Y + 3f);
                    Vector2 DirToPlayer = target.Center - NPC.Center;
                    DirToPlayer.Normalize();
                    DirToPlayer *= 12;
                    Projectile.NewProjectile(

                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            DirToPlayer,
                            ModContent.ProjectileType<ThrowingAxe>(), // 可替换成你的自定义激光
                            10,
                            2f,
                            Main.myPlayer,
                            NPC.whoAmI
                    );
                    ThrownAxe = true;
                }
            }
            if(DashCool > 0)
            {
                DashCool--;
            }
            if (distance < 200f && NPC.velocity.Y == 0 && DashCool == 0)
            {
                // 触发跳跃攻击
                currentState = AttackState.JumpingUp;
                jumpTimer = 0;
                NPC.velocity.Y = -14f; // 强力跳跃
                JumpStateDir = NPC.velocity.X > 0 ? 1 : -1;
                NPC.velocity.X = 0; // 停止水平移动
                // 播放音效
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
        public override void FindFrame(int frameHeight)
        {
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
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
                        if (NPC.frame.Y >= frameHeight * 3) // 超过第3帧时回到第1帧
                        {
                            NPC.frame.Y = 0;
                        }
                    }
                    break;
                case AttackState.JumpingUp:
                    NPC.direction = JumpStateDir;
                    NPC.spriteDirection = NPC.direction;
                    break;
                case AttackState.DivingDown:
                    NPC.frame.Y = frameHeight; // 第2帧
                    break;
            }
        }
        private void HandleJumpingUp(Player target)
        {
            jumpTimer++;
            // 上升阶段：垂直向上，水平轻微晃动
            NPC.velocity.X = (float)Math.Sin(jumpTimer * 0.1f) * 2f;
            // 当达到最高点开始下落时，切换到俯冲状态
            if (NPC.velocity.Y >= 0 && jumpTimer > 20)
            {
                currentState = AttackState.DivingDown;
                diveTarget = target.Center; // 锁定玩家当前位置
                NPC.velocity.Y = 0; // 重置垂直速度

                // 计算俯冲方向
                Vector2 diveDirection = diveTarget - NPC.Center;
                diveDirection.Normalize();
                NPC.velocity = diveDirection * diveSpeed;

                // 播放俯冲音效
                SoundEngine.PlaySound(SoundID.Item73, NPC.position);
            }
        }

        private void HandleDivingDown(Player target)
        { 
            // 持续朝目标位置俯冲
            Vector2 diveDirection = diveTarget - NPC.Center;
            // 如果接近目标或超过一定距离，恢复正常
            if (Vector2.Distance(NPC.Center, diveTarget) < 80f ||NPC.velocity.Y > 16f || NPC.velocity.Y == 0) // 落地检测
            {
                CreateImpactEffect();
                CreateTileImpactEffect();
                currentState = AttackState.Normal;
                NPC.velocity *= 0.5f; // 撞击后减速
                DashCool = 120;
            }
            else
            {
                // 继续俯冲，稍微调整方向追踪玩家
                diveDirection.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, diveDirection * diveSpeed, 0.05f);
            }
        }
        private void CreateTileImpactEffect()
        {
            // 主粒子：碎石效果，向上飞
            for (int i = 0; i < 15; i++)
            {
                // 粒子生成位置在NPC底部接触点附近，更符合物理逻辑
                Vector2 dustPos = new Vector2(
                    NPC.Center.X + Main.rand.NextFloat(-NPC.width / 2, NPC.width / 2),
                    NPC.Bottom.Y - 4
                );
                Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Stone, 0f, 0f, 100, default, 1.2f);
                // 速度：向上为主，带随机水平偏移，模拟撞击溅射
                dust.velocity = new Vector2(
                    Main.rand.NextFloat(-2f, 2f),  // 随机左右散开
                    Main.rand.NextFloat(-5f, -2f)  // 负的Y值=向上飞行（Terraria坐标系Y轴向下为正）
                );
                dust.noGravity = false; // 让粒子受重力影响慢慢下落，更真实
                dust.scale = Main.rand.NextFloat(1f, 1.5f); // 随机大小增加层次感
            }
            // 辅助粒子：灰尘效果，混合更自然
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-NPC.width / 2, NPC.width / 2), -2);
                Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.Dirt, 0f, 0f, 100, default, 1f);
                dust.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f));
                dust.noGravity = false;
            }
            // 撞Tile音效，用挖掘石头的声音更贴合场景
            SoundEngine.PlaySound(SoundID.Item127, NPC.position);
        }
        private void CreateImpactEffect()
        {
            // 创建撞击粒子效果
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Stone, 0f, 0f, 100, default, 1.5f);
                dust.velocity *= 3f;
                dust.noGravity = true;
            }
        }
        //
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = NPC.spriteDirection == -1 ?SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height / 2f);
            //Vector2 origin = texture.Size() / 2f;
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