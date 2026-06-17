using Microsoft.Xna.Framework;
using MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
//USING 引用
namespace MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement
{
    [AutoloadBossHead]//加载BOSS头
    public class GoldBody : ModNPC
    {
        private int PlayerUpOrDown;
        //帧图逻辑
        private int _frameIndex = 0;
        private int Limb1FrameIndex = 0;
        private int Limb2FrameIndex = 0;
        private bool LockFrame = false;
        private BossState oldBossState;
        private int PlayerDirection;
        private int oldDirection;
        //伤害逻辑
        private int NPC_Damage = 32;
        private int GoldProj_Damage = 12;
        private int StoneProj_Damage = 10;
        private int CopperProj_Damage = 10;
        private int BoulderProj_Damage = 20;
        // ===== 状态和时间 =====
        public BossState CurrentBossState = BossState.Idle;
        private int BossEscapeDelay = 180;//逃脱倒数计时器
        private int ProjectileTimer = 0;//弹幕时间
        private int BossEscapeTimer = 0;
        private int BossStateTimer = 0;//状态时间累加
        private int BossCoolDown = 0;
        //===== 距离 =====
        private int BossEscapeDistance = 3200;
        // ===== 拳击 =====
        private Vector2 PunchDir1;
        private Vector2 PunchDir2;
        private int RockRelease = 0;
        private bool PunchCharging1 = false;
        private bool PunchCharging2 = false;
        //=====手部撞击=====
        private const float LimbPunchTriggerDistance = 170f;
        //=====头部撞击=====
        private int DashCount = 0;
        private Vector2 dashDir = Vector2.Zero;
        //=====手部冲击=====
        private int Limb1DashCount = 0;
        private Vector2 Limb1dashDir = Vector2.Zero;
        private int Limb2DashCount = 0;
        private Vector2 Limb2dashDir = Vector2.Zero;
        private bool limb1DoneDash = false;
        private bool limb2DoneDash = false;
        //=====发射巨石=====
        private float CrimtaneBoulderSpeed = 10f;
        //=====传送位置=====
        private Vector2 TelPos1;
        private Vector2 TelPos2;
        private Vector2 TelPos3;
        private Vector2 TelPos4;
        //=====发射铜矿=====
        private int CopperProjCount = 0;
        private int CopperArrowCount = 70;
        private float CopperArrowSpeed = 10f;
        // ===== 预测位置 =====
        private Vector2 PredictPos;
        public enum BossState
        {
            Punch = 0,
            LimbDashCool = 1,
            ClipTel = 2,
            //手部随机
            HeadDashCool = 3,
            CrimtaneBoulderCoolDown = 4,
            //头部随机
            CopperProjCoolDown =5,
            //不要随机
            Idle = 6,
            HeadDash = 7,
            CrimtaneBoulders = 8,
            LimbDash = 9,
            CopperProj = 10
        }
        public override void SetStaticDefaults()//BOSS预设值
        {
            Main.npcFrameCount[Type] = 13;
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.friendly = false;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            //
            NPC.width = 119;
            NPC.height = 127;
            //
            NPC.scale = 2f;
            //
            NPC.lifeMax = 2000;
            //
            NPC.defense = 10;
            //
            NPC.damage = NPC_Damage;
            //
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.Item14;
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/Boss_fight/GoldElement");
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnSpawn(IEntitySource source)
        {
            //肢体生成逻辑
            if (NPC.ai[0] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                bool limb1Valid = TryGetLimb((int)NPC.ai[0], out _);
                bool limb2Valid = TryGetLimb((int)NPC.ai[1], out _);
                if (!limb1Valid && !limb2Valid)
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
            if (AreBothLimbsDead())
            {
                NPC.dontTakeDamage = false;
            }
            else
            {
                NPC.dontTakeDamage = true;
            }
            if (Main.myPlayer == NPC.whoAmI || Main.LocalPlayer.active)
            {
                int myMusicFadeIndex = NPC.type % 500; // 取余，确保一定在范围内
                if (myMusicFadeIndex >= 0 && myMusicFadeIndex < Main.musicFade.Length) // 安全检查
                {
                    Main.musicFade[myMusicFadeIndex] = 1f;
                }
            }
            //NPC.rotation += 0.4f;

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
            //帧图方向
            if (target.Center.X > NPC.Center.X)
            {
                PlayerDirection = 1;
            }
            else
            {
                PlayerDirection = -1;
            }
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
        private void DoCopperProjCoolDown(Player target)
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
                RandomBossState(NextState: BossState.CopperProj);
            }
        }

        private void DoCopperProj(Player target)
        {
            NPC.velocity *= 0;
            if(CopperProjCount < 3 && BossStateTimer % 100 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item53, NPC.Center);
                CopperArrowCount = Main.rand.Next(68, 74);
                float angleStep = MathHelper.TwoPi / CopperArrowCount;
                for (int i = 0; i < CopperArrowCount; i++)
                {
                    // 从正右方向开始，顺时针旋转生成每一支箭的方向
                    float currentAngle = angleStep * i;
                    Vector2 arrowVelocity = Vector2.UnitX.RotatedBy(currentAngle) * CopperArrowSpeed;

                    // 发射弹幕：如果没有自定义铜箭弹幕，可替换为原版ProjectileID.CopperArrow
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,         // 以NPC中心为发射原点
                        arrowVelocity,
                        ModContent.ProjectileType<CopperOreProj>(), // 换成你的铜箭弹幕类
                        CopperProj_Damage,         // 伤害和NPC攻击力一致
                        2f,                 // 击退力
                        NPC.target          // 绑定目标玩家
                    );
                }
                CopperProjCount++;
            }
            else if(CopperProjCount >= 3)
            {
                CopperProjCount = 0;
                RandomBossState(NextState: BossState.Idle);
            }
            
        }
        private void DoLimbDashCoolDown(Player target)
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
                Limb1DashCount = 0;
                Limb1dashDir = Vector2.Zero;
                Limb2DashCount = 0;
                Limb2dashDir = Vector2.Zero;
                limb1DoneDash = false;
                limb2DoneDash = false;
                RandomBossState(NextState: BossState.LimbDash);
            }
        }
        private void DoLimbDash(Player target)
        {
            bool hasLimb1 = TryGetLimb((int)NPC.ai[0], out _);
            bool hasLimb2 = TryGetLimb((int)NPC.ai[1], out _);
            Vector2 Dir = Vector2.Normalize(target.Center - NPC.Center);
            BodyMove(Dir, 6f, 0.8f);
            if (hasLimb1)
            {
                NPC limb1 = Main.npc[(int)NPC.ai[0]];
                int DashDuration = 40;
                int cooldown = 30;
                int totalCycle = DashDuration + cooldown;
                int phase = BossStateTimer % totalCycle;
                if (phase == 0 && Limb1DashCount < 3)
                {
                    Limb1DashCount++;
                    PredictPlayerVelocity(target, 5f);
                    Limb1dashDir = PredictPos - limb1.Center;
                    if (Limb1dashDir != Vector2.Zero)
                        Limb1dashDir.Normalize();
                    limb1.velocity = Limb1dashDir * 18f;
                }
                if (phase < DashDuration)
                {
                    // 冲刺期间保持速度
                    limb1.velocity = Limb1dashDir * 18f;
                }
                else
                {
                    // 冷却期间减速
                    limb1.velocity *= 0.9f;
                }
                if (Limb1DashCount >= 3)
                {
                    limb1DoneDash = true;
                }
            }
            else
            {
                limb1DoneDash = true;
            }
            if (hasLimb2)
            {
                NPC limb2 = Main.npc[(int)NPC.ai[1]];
                int DashDuration = 40;
                int cooldown = 30;
                int totalCycle = DashDuration + cooldown;
                int phase = BossStateTimer % totalCycle;
                if (phase == 0 && Limb2DashCount < 3)
                {
                    Limb2DashCount++;
                    PredictPlayerVelocity(target, 10f);
                    Limb2dashDir = PredictPos - limb2.Center;
                    if (Limb2dashDir != Vector2.Zero)
                        Limb2dashDir.Normalize();
                    limb2.velocity = Limb2dashDir * 18f;
                }
                if (phase < DashDuration)
                {
                    // 冲刺期间保持速度
                    limb2.velocity = Limb2dashDir * 18f;
                }
                else
                {
                    // 冷却期间减速
                    limb2.velocity *= 0.9f;
                }
                if (Limb2DashCount >= 3)
                {
                    limb2DoneDash = true;
                }
            }
            else
            {
                limb2DoneDash = true;
            }
            if(limb1DoneDash && limb2DoneDash)
            {
                RandomBossState(NextState: BossState.Idle);
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
        private void DoClipTel(Player target)
        {
            if (BossStateTimer  < 100)
            {
                limb1Follow(target);
                limb2Follow(target);
                Vector2 Dir = NPC.Center - target.Center;
                BodyMove(Dir, 1f, 0.25f);
                TelPos1 = new Vector2(target.Center.X - 180, target.Center.Y);
                TelPos2 = new Vector2(target.Center.X + 180, target.Center.Y);
                TelPos3 = new Vector2(target.Center.X, target.Center.Y - 240);
                TelPos4 = new Vector2(target.Center.X, target.Center.Y + 240);
                bool hasLimb1 = TryGetLimb((int)NPC.ai[0], out _);
                bool hasLimb2 = TryGetLimb((int)NPC.ai[1], out _);
                //Warning 
                float pulse = (float)Math.Sin(BossStateTimer * 0.15f) * 0.5f + 0.5f;

                if (hasLimb1)
                {
                    NPC limb1 = Main.npc[(int)NPC.ai[0]];
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos1 + Main.rand.NextVector2Circular(30f, 30f),
                                0, 0,
                                DustID.Stone,
                                Scale: 2.5f + pulse
                            );
                            dust.noGravity = true;
                            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                            dust.alpha = (int)(100 * (1f - pulse)); // 透明度变化
                        }
                    }
                }
                if(hasLimb2)
                {
                    NPC limb2 = Main.npc[(int)NPC.ai[1]];
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos2 + Main.rand.NextVector2Circular(30f, 30f),
                                0, 0,
                                DustID.Stone,
                                Scale: 2.5f + pulse
                            );
                            dust.noGravity = true;
                            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                            dust.alpha = (int)(100 * (1f - pulse)); // 透明度变化
                        }
                    }
                }
                if (NPC.active)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos3 + Main.rand.NextVector2Circular(30f, 30f),
                                0, 0,
                                DustID.Stone,
                                Scale: 2.5f + pulse
                            );
                            dust.noGravity = true;
                            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                            dust.alpha = (int)(100 * (1f - pulse)); // 透明度变化
                        }
                        for (int i = 0; i < 7; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos4 + Main.rand.NextVector2Circular(30f, 30f),
                                0, 0,
                                DustID.Stone,
                                Scale: 2.5f + pulse
                            );
                            dust.noGravity = true;
                            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                            dust.alpha = (int)(100 * (1f - pulse)); // 透明度变化
                        }
                    }
                }
            }
            else
            {
                bool hasLimb1 = TryGetLimb((int)NPC.ai[0], out _);
                bool hasLimb2 = TryGetLimb((int)NPC.ai[1], out _);
                if (hasLimb1)
                {
                    NPC limb1 = Main.npc[(int)NPC.ai[0]];
                    limb1.Center = TelPos1;
                    limb1.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item14, limb1.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos1 + Main.rand.NextVector2Circular(20f, 20f),
                                limb1.width, limb1.height,
                                DustID.Stone,
                                Scale: 2.5f
                            );
                        }
                    }
                }
                if (hasLimb2)
                {
                    NPC limb2 = Main.npc[(int)NPC.ai[1]];
                    limb2.Center = TelPos2;
                    limb2.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item14, limb2.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos2 + Main.rand.NextVector2Circular(20f, 20f),
                                limb2.width, limb2.height,
                                DustID.Stone,
                                Scale: 2.5f
                            );
                        }
                    }
                }
                if(Main.rand.Next(0,2) == 0)
                {
                    NPC.Center = TelPos3;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos3 + Main.rand.NextVector2Circular(20f, 20f),
                                NPC.width, NPC.height,
                                DustID.Stone,
                                Scale: 2.5f
                            );
                        }
                    }
                    RandomBossState(NextState: BossState.Idle);
                }
                else
                {
                    NPC.Center = TelPos4;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                TelPos4 + Main.rand.NextVector2Circular(20f, 20f),
                                NPC.width, NPC.height,
                                DustID.Stone,
                                Scale: 2.5f
                            );
                        }
                    }
                    RandomBossState(NextState: BossState.Idle);
                }
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
                //Projectile
                bool hasLimb1 = TryGetLimb((int)NPC.ai[0], out _);
                bool hasLimb2 = TryGetLimb((int)NPC.ai[1], out _);
                if (hasLimb1)
                {
                    NPC limb1 = Main.npc[(int)NPC.ai[0]];
                    Vector2 targetDir = Vector2.Normalize(target.Center - limb1.Center);
                    Projectile.NewProjectile(limb1.GetSource_FromAI(), limb1.position, targetDir * 10f, ModContent.ProjectileType<GoldOreProj>(),GoldProj_Damage,2f,NPC.target,limb1.whoAmI);
                }
                if (hasLimb2)
                {
                    NPC limb2 = Main.npc[(int)NPC.ai[1]];
                    Vector2 targetDir = Vector2.Normalize(target.Center - limb2.Center);
                    Projectile.NewProjectile(limb2.GetSource_FromAI(), limb2.position, targetDir * 10f, ModContent.ProjectileType<GoldOreProj>(),GoldProj_Damage, 2f,NPC.target,limb2.whoAmI);
                }
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
                BoulderProj_Damage,
                2f, NPC.target
            );

            Vector2 leftUpVelocity = new Vector2(-CrimtaneBoulderSpeed, -CrimtaneBoulderSpeed);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.TopLeft,
                leftUpVelocity,
                ModContent.ProjectileType<CrimtaneBoulder>(),
                BoulderProj_Damage,
                2f,NPC.target
            );
            Vector2 rightUpVelocity = new Vector2(CrimtaneBoulderSpeed, -CrimtaneBoulderSpeed);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.TopRight,
                rightUpVelocity,
                ModContent.ProjectileType<CrimtaneBoulder>(),
                BoulderProj_Damage,
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
                    if (hasLimb1)
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
                                        StoneProj_Damage,
                                        2f,
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
                        NPC limb2 = Main.npc[(int)NPC.ai[1]];
                        if (hasLimb2)
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
                                            GoldProj_Damage,
                                            2f,
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
            if (CurrentBossState != BossState.Punch && CurrentBossState != BossState.ClipTel && distToPlayer < LimbPunchTriggerDistance)
            {
                Vector2 Dir = target.Center - limb1.Center;
                if (Dir != Vector2.Zero)
                    Dir.Normalize();
                limb1.velocity = Vector2.Lerp(limb1.velocity, Dir * 5f, 0.5f);
            }
            Vector2 limb1Pos = new Vector2(NPC.Center.X - 150, NPC.Center.Y + 50);
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
                if (CurrentBossState != BossState.Punch && CurrentBossState != BossState.ClipTel && distToPlayer < LimbPunchTriggerDistance)
                {
                    Vector2 Dir = target.Center - limb2.Center;
                    if (Dir != Vector2.Zero)
                        Dir.Normalize();
                    limb2.velocity = Vector2.Lerp(limb2.velocity, Dir * 5f, 0.5f);
                    return;
                }
            }
            Vector2 limb2Pos = new Vector2(NPC.Center.X + 150, NPC.Center.Y + 50);
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
            BossCoolDown = 0;
            if (NextState != null)
            {
                CurrentBossState = (BossState)NextState;
                return;
            }
            if(AreBothLimbsDead())
            {
                BossState next;
                do
                {
                    next = (BossState)Main.rand.Next(3, 6);
                }
                while ((next == CurrentBossState || (blockedState.HasValue && next == blockedState.Value || blockedState2.HasValue && next == blockedState2)));
                CurrentBossState = next;
            }
            else
            {
                BossState next;
                do
                {
                    next = (BossState)Main.rand.Next(0, 5);
                }
                while ((next == CurrentBossState || (blockedState.HasValue && next == blockedState.Value || blockedState2.HasValue && next == blockedState2)));
                CurrentBossState = next;
            }
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
                    case BossState.ClipTel:
                        DoClipTel(target);
                        break;
                    case BossState.LimbDashCool:
                        DoLimbDashCoolDown(target);
                        break;
                    case BossState.LimbDash:
                        DoLimbDash(target);
                        break;
                    case BossState.CopperProjCoolDown:
                        DoCopperProjCoolDown(target);
                        break;
                    case BossState.CopperProj:
                        DoCopperProj(target);
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
        private bool AreBothLimbsDead()
        {
            // 手臂尚未完成初始化时，不视为已死亡，保持本体无敌（避免刚生成就被误打）
            if (NPC.ai[0] <= 0 || NPC.ai[1] <= 0)
                return false;

            // 手臂1死亡条件：无法获取到有效手臂，或者手臂生命值≤0
            bool limb1Dead = !TryGetLimb((int)NPC.ai[0], out var limb1) || limb1.life <= 0;
            // 手臂2同理
            bool limb2Dead = !TryGetLimb((int)NPC.ai[1], out var limb2) || limb2.life <= 0;

            return limb1Dead && limb2Dead;
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) //难度和玩家数量
        {
            if (Main.expertMode)//专家模式
            {
                NPC.lifeMax = 6000;
                NPC_Damage = 68;
                NPC.damage = NPC_Damage;
                GoldProj_Damage = 20;
                StoneProj_Damage = 16;
                CopperProj_Damage = 16;
                BoulderProj_Damage = 22;
            }
            if (Main.masterMode)//大师模式
            {
                NPC.lifeMax = 8000;
                NPC_Damage = 112;
                NPC.damage = NPC_Damage;
                GoldProj_Damage = 24;
                StoneProj_Damage = 20;
                CopperProj_Damage = 20;
                BoulderProj_Damage = 26;
            }
        }
        public override void OnKill()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -2f)), Mod.Find<ModGore>("GoldBodyGore1").Type, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-5f, -1.5f)), Mod.Find<ModGore>("GoldBodyGore2").Type, NPC.scale * Main.rand.NextFloat(0.85f, 1.05f));
        }
        public override void FindFrame(int frameHeight)
        {
            if(TryGetLimb((int)NPC.ai[0], out var limb1))
            {
                NPC limb_1 = Main.npc[(int)NPC.ai[0]];
                if (CurrentBossState == BossState.Punch || CurrentBossState == BossState.LimbDash)
                {
                    Limb1FrameIndex = 0;
                }
                else
                {
                    Limb1FrameIndex = 1;
                }
                limb_1.frame.Y = Limb1FrameIndex * 40;
                if (CurrentBossState == BossState.Punch)
                {
                    limb_1.spriteDirection = PunchDir1.X > 0 ? 1 : -1;
                }
                if (CurrentBossState == BossState.LimbDash)
                {
                    limb_1.spriteDirection = Limb1dashDir.X > 0 ? 1 : -1;
                }
            }
            if (TryGetLimb((int)NPC.ai[0], out var limb2))
            {
                NPC limb_2 = Main.npc[(int)NPC.ai[1]];
                if (CurrentBossState == BossState.Punch || CurrentBossState == BossState.LimbDash)
                {
                    Limb2FrameIndex = 0;
                }
                else
                {
                    Limb2FrameIndex = 2;
                }
                limb_2.frame.Y = Limb2FrameIndex * 40;
                if(CurrentBossState == BossState.Punch)
                {
                    limb_2.spriteDirection = PunchDir2.X > 0 ? 1 : -1;
                }
                if(CurrentBossState == BossState.LimbDash)
                {
                    limb_2.spriteDirection = Limb2dashDir.X > 0 ? 1 : -1;
                }
            }
            if (oldBossState != CurrentBossState)
            {
                LockFrame = false;
                oldDirection = _frameIndex;
            }
            // HeadDashCool：播 11, 12 两帧循环（13 是非法索引，去掉）
            if (CurrentBossState == BossState.HeadDashCool)
            {
                if (!LockFrame)
                {
                    _frameIndex = 11;
                    NPC.frameCounter = 0;
                    oldBossState = CurrentBossState;
                    LockFrame = true;
                }
                if (PlayerDirection == 1)
                {
                    _frameIndex = 12;
                }
                else
                {
                    _frameIndex = 11;
                }
                NPC.frame.Y = _frameIndex * frameHeight;
                return;
            }
            else if (CurrentBossState == BossState.HeadDash)
            {
                if (!LockFrame)
                {
                    _frameIndex = oldDirection;
                    NPC.frameCounter = 0;   // 状态进入时重置计数器
                    oldBossState = CurrentBossState;
                    LockFrame = true;
                }
                NPC.frameCounter++;
                if (NPC.frameCounter >= 5)
                {
                    NPC.frameCounter = 0;
                    _frameIndex++;
                    if (_frameIndex > 12)    // ✅ 13 帧不存在，回 11
                        _frameIndex = 10;
                    NPC.frame.Y = _frameIndex * frameHeight;
                }
                return;
            }
            else
            {
                if(PlayerUpOrDown == 1)
                {
                    if (_frameIndex > 10)
                        _frameIndex = 0;
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 5)
                    {
                        NPC.frameCounter = 0;
                        _frameIndex++;
                        if (_frameIndex > 9)
                            _frameIndex = 0;
                        NPC.frame.Y = _frameIndex * frameHeight;
                    }
                }
                else
                {
                    if (_frameIndex > 10)
                        _frameIndex = 0;
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 5)
                    {
                        NPC.frameCounter = 0;
                        _frameIndex++;
                        if (_frameIndex > 9)
                            _frameIndex = 0;
                        NPC.frame.Y = _frameIndex * frameHeight;
                    }
                }
            }
        }
    }
}
