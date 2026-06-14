using Microsoft.Xna.Framework;
using MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using YourModName.Projectiles;
//USING 引用
namespace MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement
{
    [AutoloadBossHead]//加载BOSS头
    public class GoldBody : ModNPC
    {
        // ===== 状态和时间 =====
        public BossState CurrentBossState = BossState.Idle;
        private int BossEscapeDelay = 180;//逃脱倒数计时器
        private int ProjectileTimer = 0;//弹幕时间
        private int BossEscapeTimer = 0;
        private int BossStateTimer = 0;//状态时间累加
        private int BossCoolDown = 0;
        private int Time = 0;
        //===== 距离 =====
        private int BossEscapeDistance = 3200;
        // ===== 拳击 =====
        private Vector2 PunchDir1;
        private Vector2 PunchDir2;
        private int RockRelease = 0;
        private bool PunchCharging1 = false;
        private bool PunchCharging2 = false;
        //=====手部撞击=====
        private const float LimbPunchTriggerDistance = 160f;
        //=====头部撞击=====
        private int DashCount = 0;
        private Vector2 dashDir = Vector2.Zero;
        //=====发射巨石=====
        private float CrimtaneBoulderSpeed = 10f;
        // ===== 预测位置 =====
        private Vector2 PredictPos;
        public enum BossState
        {
            Punch = 0,
            HeadDashCool = 1,
            CrimtaneBoulderCoolDown = 2,
            //不要随机
            Idle = 3,
            HeadDash = 4,
            CrimtaneBoulders = 5
        }
        public override void SetStaticDefaults()//BOSS预设值
        {
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.friendly = false;
            NPC.width = 32;
            NPC.height = 32;
            NPC.scale = 4f;
            NPC.lifeMax = 3200;
            NPC.defense = 10;
            NPC.damage = 25;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnSpawn(IEntitySource source)
        {
            //肢体生成逻辑
            if (NPC.ai[0] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!NPC.AnyNPCs(ModContent.NPCType<GoldArm>()))
                {
                    int limb1 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GoldArm>(), NPC.whoAmI);
                    int limb2 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GoldArm>(), NPC.whoAmI);
                    NPC.ai[0] = limb1; // 存肢体 whoAmI
                    NPC.ai[1] = limb2;
                    Main.npc[limb1].ai[0] = NPC.whoAmI; // 肢体存本体 whoAmI
                    Main.npc[limb2].ai[0] = NPC.whoAmI;
                    NPC.netUpdate = true;
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void AI()
        {
            NPC.rotation += 0.4f;
            //脱战逻辑和寻找玩家
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1) return;
            bool NoVaildPlayer = targetID == -1 || !Main.player[targetID].active || Main.player[targetID].dead;
            if (NoVaildPlayer)
            {
                Despawn();
                return;
            }
            Player target = Main.player[targetID];
            NPC.target = targetID;
            //距离脱战逻辑
            float distance = Vector2.Distance(NPC.Center, target.Center);
            if (distance > BossEscapeDistance)
            {
                BossEscapeTimer++;
                if (BossEscapeTimer >= BossEscapeDelay)
                {
                    Despawn();
                    return;
                }
            }
            else
            {
                BossEscapeTimer = 0;
            }
            //状态切换机制
            StateSwitcher(target);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void DoHeadDashCoolDown(Player target)
        {
            if (BossStateTimer < 60)
            {
                limb1Follow(target);
                limb2Follow(target);
                Vector2 Dir = NPC.Center - target.Center;
                BodyMove(Dir, 1f, 0.25f);
            }
            else
            {
                DashCount = 0;
                dashDir = Vector2.Zero;
                RandomBossState(NextState: BossState.HeadDash);
            }
        }
        private void DoCrimtaneBoulderCoolDown(Player target)
        {
            if (BossStateTimer < 60)
            {
                limb1Follow(target);
                limb2Follow(target);
                Vector2 Dir = Vector2.Zero;
                BodyMove(Dir, 0f, 0.0f);
            }
            else
            {
                RandomBossState(NextState: BossState.CrimtaneBoulders);
            }
        }
        
        private void DoHeadDash(Player target)
        {
            limb1Follow(target);
            limb2Follow(target);
            int DashDuration = 40;
            int cooldown = 30;
            int totalCycle = DashDuration + cooldown;
            int phase = BossStateTimer % totalCycle;
            if (phase == 0 && DashCount < 3)
            {
                DashCount++;
                PredictPlayerVelocity(target, 5f);
                dashDir = PredictPos - NPC.Center;
                if (dashDir != Vector2.Zero)
                    dashDir.Normalize();

                NPC.velocity = dashDir * 18f;
            }
            if (phase < DashDuration)
            {
                // 冲刺期间保持速度
                NPC.velocity = dashDir * 18f;
            }
            else
            {
                // 冷却期间减速
                NPC.velocity *= 0.9f;
            }
            if (DashCount >= 3)
            {
                RandomBossState(NextState: BossState.Idle);
            }
        }
        private void DoCrimtaneBoulder(Player target)
        {
            limb1Follow(target);
            limb2Follow(target);
            Vector2 Dir = Vector2.Zero;
            BodyMove(Dir, 0f, 0.0f);
            //射出弹幕

            Vector2 UpVelocity = new Vector2(0, -CrimtaneBoulderSpeed);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.TopLeft,
                UpVelocity,
                ModContent.ProjectileType<CrimtaneBoulder>(),
                NPC.damage,
                2f, NPC.target
            );

            Vector2 leftUpVelocity = new Vector2(-CrimtaneBoulderSpeed, -CrimtaneBoulderSpeed);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.TopLeft,
                leftUpVelocity,
                ModContent.ProjectileType<CrimtaneBoulder>(),
                NPC.damage,
                2f,NPC.target
            );
            Vector2 rightUpVelocity = new Vector2(CrimtaneBoulderSpeed, -CrimtaneBoulderSpeed);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.TopRight,
                rightUpVelocity,
                ModContent.ProjectileType<CrimtaneBoulder>(),
                NPC.damage,
                2f, NPC.target
            );
            RandomBossState(NextState:BossState.Idle);
        }
        private void DoPunch(Player target)
        {
            if(BossStateTimer < 200)
            {
                Vector2 Dir = target.Center - NPC.Center;
                BodyMove(Dir,4f,0.8f);
                bool hasLimb1 = TryGetLimb((int)NPC.ai[0], out _);
                bool hasLimb2 = TryGetLimb((int)NPC.ai[1], out _);
                if (hasLimb1)
                {
                    NPC limb1 = Main.npc[(int)NPC.ai[0]];
                    if (TryGetLimb((int)NPC.ai[0], out limb1))
                    {
                        if (ProjectileTimer == 60 && !PunchCharging1)
                        {
                            PredictPlayerVelocity(target, 15f);
                            PunchDir1 = PredictPos - limb1.Center;
                            if (PunchDir1 != Vector2.Zero)
                                PunchDir1.Normalize();

                            PunchCharging1 = true;
                        }

                        if (PunchCharging1)
                        {
                            limb1.velocity = PunchDir1 * 12f;
                            if (RockRelease < 3 && ProjectileTimer % 30 == 0)
                            {
                                float baseRotation = PunchDir1.ToRotation();
                                float[] angleOffsets = new float[]
                                {
                                    -(MathHelper.PiOver4)/2,
                                    0f,
                                    MathHelper.PiOver4/2
                                };
                                foreach (float offset in angleOffsets)
                                {
                                    Vector2 velocity = Vector2.UnitX.RotatedBy(baseRotation + offset) * 12f;
                                    Projectile.NewProjectile(
                                        limb1.GetSource_FromAI(),
                                        limb1.Center,
                                        velocity,
                                        ProjectileID.RockGolemRock, // 可替换成你的自定义激光
                                        40,
                                        1f,
                                        NPC.target
                                    );
                                }
                            }
                        }
                        if (ProjectileTimer > 120)
                        {
                            PunchCharging1 = false;
                            limb1Follow(target);
                        }
                    }
                    if (hasLimb2)
                    {
                        NPC limb2 = Main.npc[(int)NPC.ai[0]];
                        if (TryGetLimb((int)NPC.ai[1], out limb2))
                        {
                            if (ProjectileTimer == 120 && !PunchCharging2)
                            {
                                PredictPlayerVelocity(target, 30f);
                                PunchDir2 = PredictPos - limb2.Center;
                                if (PunchDir2 != Vector2.Zero)
                                    PunchDir2.Normalize();

                                PunchCharging2 = true;
                            }
                            if (PunchCharging2)
                            {
                                limb2.velocity = PunchDir2 * 12f;
                                if (RockRelease < 3 && ProjectileTimer % 30 == 0)
                                {
                                    float baseRotation = PunchDir2.ToRotation();
                                    float[] angleOffsets = new float[]
                                    {
                                    -(MathHelper.PiOver4)/2,
                                    0f,
                                    MathHelper.PiOver4/2
                                    };
                                    foreach (float offset in angleOffsets)
                                    {
                                        Vector2 velocity = Vector2.UnitX.RotatedBy(baseRotation + offset) * 12f;
                                        Projectile.NewProjectile(
                                            limb2.GetSource_FromAI(),
                                            limb2.Center,
                                            velocity,
                                            ModContent.ProjectileType<GoldOreProj>(), // 可替换成你的自定义激光
                                            40,
                                            1f,
                                            NPC.target
                                            , limb2.whoAmI
                                        );
                                    }
                                    RockRelease++;
                                }
                            }
                            if (ProjectileTimer > 180)
                            {
                                RockRelease = 0;
                                PunchCharging2 = false;
                                limb2Follow(target);
                            }
                        }
                    }
                }
            }
            else
            {
                RandomBossState(NextState:BossState.Idle);//让关节返回
            }
        }
        private void DoIDLE(Player target)
        {
            if (BossStateTimer < 200)
            {
                limb1Follow(target);
                limb2Follow(target);
                Vector2 Dir = target.Center - NPC.Center;
                BodyMove(Dir,4f,0.8f);
            }
            else
            {
                RandomBossState();
            }
        }
        private void BodyMove(Vector2 Dir,float HeadChaseSpeed,float HeadPlusSpeed)
        {
            if (Dir != Microsoft.Xna.Framework.Vector2.Zero)
            {
                Dir.Normalize();
            }
            NPC.velocity = Vector2.Lerp(NPC.velocity, Dir * HeadChaseSpeed, HeadPlusSpeed);
        }
        private void limb1Follow(Player target)
        {
            if (!TryGetLimb((int)NPC.ai[0], out var limb1))
                return;
            float distToPlayer = Vector2.Distance(limb1.Center, target.Center);
            if (CurrentBossState != BossState.Punch && distToPlayer < LimbPunchTriggerDistance)
            {
                Vector2 Dir = target.Center - limb1.Center;
                if (Dir != Vector2.Zero)
                    Dir.Normalize();
                limb1.velocity = Vector2.Lerp(limb1.velocity, Dir * 4f, 0.4f);
            }
            Vector2 limb1Pos = new Vector2(NPC.Center.X - 100, NPC.Center.Y + 25);
            Vector2 dir = limb1Pos - limb1.Center;
            if (dir != Vector2.Zero)
                dir.Normalize();
            limb1.velocity = Vector2.Lerp(limb1.velocity, dir * 4f, 0.4f);
        }
        private void limb2Follow(Player target)
        {
            if (!TryGetLimb((int)NPC.ai[1], out var limb2))
                return;
            if (CurrentBossState != BossState.Punch)
            {
                float distToPlayer = Vector2.Distance(limb2.Center, target.Center);
                if (CurrentBossState != BossState.Punch && distToPlayer < LimbPunchTriggerDistance)
                {
                    Vector2 Dir = target.Center - limb2.Center;
                    if (Dir != Vector2.Zero)
                        Dir.Normalize();
                    limb2.velocity = Vector2.Lerp(limb2.velocity, Dir * 4f, 0.4f);
                    return;
                }
            }
            Vector2 limb2Pos = new Vector2(NPC.Center.X + 100, NPC.Center.Y + 25);
            Vector2 dir = limb2Pos - limb2.Center;
            if (dir != Vector2.Zero)
                dir.Normalize();
            limb2.velocity = Vector2.Lerp(limb2.velocity, dir * 4f, 0.4f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            // ✅ 受伤时生成一些金色的尘埃
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone);
                }
            }
        }
        public void Despawn()//脱战
        {
            NPC.velocity.Y = -10f;
            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
        }
        //状态机切换器
        private void RandomBossState(BossState? blockedState = null, BossState? blockedState2 = null, BossState? NextState = null)
        {
            if (Main.netMode == NetmodeID.Server)
                NPC.netUpdate = true;
            BossStateTimer = 0;
            ProjectileTimer = 0;
            BossCoolDown = 15;
            BossState next;
            do
            {
                next = (BossState)Main.rand.Next(0,3);
                if (NextState != null)
                {
                    next = (BossState)NextState;
                }
            }
            while ((next == CurrentBossState || (blockedState.HasValue && next == blockedState.Value || blockedState2.HasValue && next == blockedState2)));
            CurrentBossState = next;
        }
        private void StateSwitcher(Player target)
        {
            if (BossCoolDown > 0)
            {
                BossCoolDown--;
            }
            else
            {
                BossStateTimer++;
                if(CurrentBossState == BossState.Punch)
                {
                    ProjectileTimer++;
                }
                switch (CurrentBossState)
                {
                    case BossState.Idle:
                        DoIDLE(target);
                        break;
                    case BossState.Punch:
                        DoPunch(target);
                        break;
                    case BossState.HeadDashCool:
                        DoHeadDashCoolDown(target);
                        break;
                    case BossState.HeadDash:
                        DoHeadDash(target);
                        break;
                    case BossState.CrimtaneBoulderCoolDown:
                        DoCrimtaneBoulderCoolDown(target);
                        break;
                    case BossState.CrimtaneBoulders:
                        DoCrimtaneBoulder(target);
                        break;
                }
            }
        }
        private void PredictPlayerVelocity(Player target,float PredictTime)
        {
            PredictPos = target.Center + target.velocity * PredictTime;
        }
        private bool TryGetLimb(int aiSlot, out NPC limb)//安全逻辑
        {
            limb = null;
            if (aiSlot <= 0) return false;
            limb = Main.npc[aiSlot];
            return limb.active && limb.type == ModContent.NPCType<GoldArm>();
        }
    }
}
