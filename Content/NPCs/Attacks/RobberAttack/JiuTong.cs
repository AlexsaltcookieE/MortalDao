using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement;
using MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    [AutoloadBossHead]
    public class JiuTong : ModNPC
    {
        private int AttackTime;
        private enum AttackState
        {
            Normal,          // 正常巡逻
            Jump,            //跳跃
            CoolDown,
            Dash,
            DoJumpDash
        }
        private AttackState currentState = AttackState.DoJumpDash;
        public Vector2 LeftHandPos;
        public Vector2 RightHandPos;

        private Vector2 ProjectilePos;
        private int LightningCoolDown =0;

        //
        private int SpawnCoolDown = 0;
        //
        private const float BounceVelY = -15.5f;
        private int targetDirection = 0;       // 目标方向（玩家所在方向）
        private float currentSpeed = 0f;      // 当前实际水平速度
        private const float Acceleration = 0.05f; // 加速度（越小惯性越大）
        private const float MaxSpeed = 6f;    // 最大水平速度
        private const float InertiaThreshold = 0.5f; // 惯性结束阈值（速度低于此值时可转向）
                                                     //
        private int CoolTime = 40;
        private int DashDurationTime = 70;

        private bool LockDir = false;
        private Vector2 LockDirection;
        //
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
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7; // 设置NPC的帧数
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.width = 35;
            NPC.height = 40;
            NPC.scale = 3f;
            NPC.damage = 15;
            NPC.lifeMax = 1500;
            NPC.defense = 2;
            NPC.knockBackResist = 1f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
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
        public override void AI()
        {
            //脱战逻辑
            LeftHandPos = new Vector2(0, NPC.height / 2);
            RightHandPos = new Vector2(NPC.width, NPC.height / 2);
            //
            if(NPC.velocity.Y !=0 || currentState == AttackState.Dash)
            {
                NPC.knockBackResist = 0;
            }
            else
            {
                NPC.knockBackResist = 0.5f;
            }
            int targetID = NPC.FindClosestPlayer();
            bool NoVaildPlayer = targetID == -1 || !Main.player[targetID].active || Main.player[targetID].dead;
            if (NoVaildPlayer)
            {
                Despawn();
                return;
            }
            //------------------------------------------------------------------------------------------------
            Player target = Main.player[targetID];
            float distance = Vector2.Distance(NPC.Center, target.Center);
            if(distance > 1100)
            {
                Despawn();
                return;
            }
            switch (currentState)
            {
                case AttackState.Normal:
                    HandleNormalBehavior(target, distance);
                    break;
                case AttackState.Jump:
                    HandleJumpBehavior(target, distance);
                    break;
                case AttackState.CoolDown:
                    DoCoolDown();
                    break;
                case AttackState.Dash:
                    DoDash(target);
                    break;
                case AttackState.DoJumpDash:
                    DoJumpDash(target);
                    break;
            }
        }
        private void RecieveLightning(Player target)
        {
            
            if(LightningCoolDown > 120)
            {
                LightningCoolDown = 0;
                //
                for (int i = 0; i < 3; i++)
                {
                    int MaxX = 2000;
                    int MaxY = 2000;
                    int X = Main.rand.Next(0, MaxX + 1);
                    int Y = Main.rand.Next(0, MaxY + 1);
                    ProjectilePos = new Vector2((NPC.Center.X - MaxX / 2) + X, (NPC.Center.Y - MaxY / 2) + Y);
                    float ProjDistance = Vector2.Distance(ProjectilePos, target.Center);
                    if(ProjDistance < 400)
                    {

                        X = Main.rand.Next(400, MaxX - 400);
                        Y = Main.rand.Next(400,MaxX - 400);
                        bool NegativeX = Main.rand.NextBool();
                        bool NegativeY = Main.rand.NextBool();
                        if (NegativeX)
                        {
                            X = X * -1;
                        }
                        if (NegativeY)
                        {
                            Y = Y * -1;
                        }
                        ProjectilePos = new Vector2(ProjectilePos.X + X, ProjectilePos.Y + Y);
                    }
                    Vector2 ProjDir = Vector2.Normalize(NPC.Center - ProjectilePos);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), ProjectilePos, ProjDir * 12, ModContent.ProjectileType<LightningBallProj>(), 10, 1f, Main.myPlayer);
                }
            }
            else
            {
                LightningCoolDown++;
            }
        }
        private void HandleJumpBehavior(Player target , float distance)
        {
            if (AttackTime < 360)
            {
                AttackTime++;
                if (distance < 1500f)
                {
                    RecieveLightning(target);
                    SpawnNPC(target);
                    targetDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                    //
                    bool playerBelow = target.Center.Y > NPC.Center.Y + NPC.height;
                    // 检测脚下 Tile
                    Point tileBelow = NPC.Bottom.ToTileCoordinates();
                    Tile belowTile = Framing.GetTileSafely(tileBelow.X, tileBelow.Y);
                    bool standingOnPlatform =
                        belowTile.HasTile &&
                        TileID.Sets.Platforms[belowTile.TileType];
                    if (standingOnPlatform && playerBelow)
                    {
                        NPC.noTileCollide = true;
                        NPC.velocity.Y = 4f; // 向下掉落速度（可调）
                    }
                    else
                    {
                        NPC.noTileCollide = false;
                    }
                    float targetSpeed = targetDirection * MaxSpeed;
                    if (Math.Abs(currentSpeed - targetSpeed) > InertiaThreshold)
                    {
                        currentSpeed = MathHelper.Lerp(currentSpeed, targetSpeed, Acceleration);
                    }
                    else
                    {
                        currentSpeed = targetSpeed;
                    }
                    NPC.velocity.X = currentSpeed;
                    if (NPC.velocity.Y == 0)
                    {
                        NPC.velocity.Y = BounceVelY;
                    }
                }
            }
            else
            {
                AttackTime = 0;
                if(Main.rand.Next(0,2)== 0)
                {
                    currentState = AttackState.CoolDown;
                }
                else
                {
                    currentState = AttackState.Normal;
                }
            }
        }
        private void HandleNormalBehavior(Player target, float distance)
        {
            if (AttackTime < 360)
            {
                AttackTime++;
                if (distance < 1500f)
                {
                    RecieveLightning(target);
                    SpawnNPC(target);
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
                    float Xdistance = Math.Abs(target.Center.X - NPC.Center.X);
                    if (Xdistance < 150 && target.Center.Y < NPC.Center.Y && NPC.velocity.Y == 0)
                    {
                        NPC.velocity.Y = -16f;
                    }
                    Vector2 dirToPlayer = target.Center - NPC.Center;
                    dirToPlayer.Normalize();
                    float moveSpeed = 4f;
                    NPC.velocity.X = dirToPlayer.X * moveSpeed;
                    NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;
                    if (HasStepInFront() && NPC.velocity.Y == 0)
                    {
                        NPC.velocity.Y = -16f;
                    }
                }
                else
                {
                    NPC.velocity.X *= 0.9f;
                }
            }
            else
            {
                AttackTime = 0;
                if (Main.rand.Next(0, 2) == 0)
                {
                    currentState = AttackState.CoolDown;
                }
                else
                {
                    currentState = AttackState.DoJumpDash;
                }
            }
        }
        private void DoCoolDown()
        {
            if (CoolTime > 0)
            {
                CoolTime--;
                NPC.velocity.X *= 0;
            }
            else
            {
                currentState = AttackState.Dash;
                CoolTime = 40;
                return;
            }
        }
        private void DoDash(Player target)
        {
            if (DashDurationTime > 0)
            {
                DashDurationTime--;
            }
            else
            {
                if(Main.rand.Next(0, 2) == 1)
                {
                    currentState = AttackState.Normal;
                }
                else
                {
                    currentState = AttackState.Jump;
                }
                DashDurationTime = 70;
                LockDir = false;
                return;
            }
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
            //-------------------------------------------------------------------------------------------------------------------------------------------------
            if (!LockDir)
            {
                int DirInt = NPC.Center.X > target.Center.X ? -1 : 1;
                LockDirection = new Vector2(DirInt, NPC.velocity.Y);
                LockDir = true;
            }
            LockDirection.Normalize();

            float moveSpeed = 10f;

            NPC.velocity.X = LockDirection.X * moveSpeed;
            if (HasStepInFront() && NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = -4f;
            }
        }
        private void SpawnNPC(Player target)
        {
            if(SpawnCoolDown > 200)
            {
                SpawnCoolDown = 0;
                int NPCCount = 0;
                List<int> NPCtype = new List<int>()
                {
                ModContent.NPCType<thiefRobber>(),
                ModContent.NPCType<RobberWolf>(),
                ModContent.NPCType<AxeRobber>(),
                ModContent.NPCType<DartRobber>()
                };
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && (npc.type == ModContent.NPCType<thiefRobber>() || npc.type == ModContent.NPCType<RobberWolf>() || npc.type == ModContent.NPCType<AxeRobber>() || npc.type == ModContent.NPCType<DartRobber>()))
                    {
                        float distance = Vector2.Distance(npc.Center, target.Center);
                        if(distance < 1000)
                        {
                            NPCCount++;
                        }
                    }
                }
                if (NPCCount < 3)
                {
                    int NPCRandtype = Main.rand.Next(NPCtype);
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCRandtype);
                }
            }
            else
            {
                SpawnCoolDown++;
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
            NPC.direction = NPC.Center.X > target.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            //
            if (NPC.velocity.X == 0 && NPC.velocity.Y == 0)
            {
                NPC.frame.Y = NPC.frame.Height * 5;
            }
            else if(NPC.velocity.Y != 0)
            {
                NPC.frame.Y = NPC.frame.Height * 6;
            }
            else
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 6) // 每10帧切换一次
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= frameHeight * 5) // 超过第4帧时回到第1帧
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
        }
        private void DoJumpDash(Player target)
        {
            if (DashDurationTime > 0)
            {
                DashDurationTime--;
            }
            else
            {
                if (Main.rand.Next(0, 2) == 1)
                {
                    currentState = AttackState.Normal;
                }
                else
                {
                    currentState = AttackState.Jump;
                }
                DashDurationTime = 70;
                LockDir = false;
                return;
            }
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
            //-------------------------------------------------------------------------------------------------------------------------------------------------
            if (!LockDir)
            {
                int DirInt = NPC.Center.X > target.Center.X ? -1 : 1;
                LockDirection = new Vector2(DirInt, NPC.velocity.Y);
                LockDir = true;
            }
            LockDirection.Normalize();
            float moveSpeed = 10f;
            NPC.velocity.X = LockDirection.X * moveSpeed;
            if (NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = BounceVelY;
            }
            if (HasStepInFront() && NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = -4f;
            }
        }
        public void Despawn()//脱战
        {
            NPC.velocity.X = -10f;
            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
        }

        private int hitStunTimer = 0;
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            hitStunTimer = 8;
            base.OnHitByItem(player, item, hit, damageDone);
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            hitStunTimer = 8;
            base.OnHitByProjectile(projectile, hit, damageDone);
        }

        public override void OnKill()
        {
            if (NPC.type == ModContent.NPCType<JiuTong>())
                BossesDowned.DownedJiuTong = true;
        }
    }
}