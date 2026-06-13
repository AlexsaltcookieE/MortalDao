using Microsoft.Xna.Framework;
using MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj;
using MortalDao.Content.Projectiles.BossProj.GeneralSoulProj;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement
{
    [AutoloadBossHead]
    public class GoldBody : ModNPC
    {
        //肢体状态
        public bool Limb1Active = false;
        public bool Limb2Active = false;
        //状态机和时间
        public BossState CurrentBossState = BossState.Idle;
        private int BossEscapeDelay = 180;//逃脱倒数计时器
        private int ProjectileTimer = 0;//弹幕时间
        private int BossEscapeTimer = 0;
        private int BossStateTimer = 0;//状态时间累加
        private int BossCoolDown = 0;
        private int Time = 0;
        //距离
        private int BossEscapeDistance = 3200;
        //拳击状态
        private Vector2 PunchDir1;
        private Vector2 PunchDir2;
        private int RockRelease = 0;
        private bool PunchCharging1 = false;
        private bool PunchCharging2 = false;
        //位置
        private Vector2 PredictPos;
        public enum BossState
        {
            Idle = 0,
            Punch = 1,
            LimbClipTel =2,
            ClipPlayer = 3
        }
        public override void SetStaticDefaults()//BOSS预设值
        {
            // Ensure Terraria treats this ModNPC as a real boss for UI/progression behavior.
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
            NPC.noTileCollide = true;
            NPC.damage = 25;
            NPC.friendly = false;
            NPC.width = 32;
            NPC.height = 32;
            NPC.scale = 4f;
            NPC.defense = 10;
            NPC.lifeMax = 3200;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnSpawn(IEntitySource source)
        {
            //肢体生成逻辑
            if (NPC.ai[0] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!NPC.AnyNPCs(ModContent.NPCType<GoldArm>()))
                {

                    int limb1 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
                        ModContent.NPCType<GoldArm>(), NPC.whoAmI);
                    int limb2 = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
                        ModContent.NPCType<GoldArm>(), NPC.whoAmI);
                    NPC.ai[0] = limb1; // 存肢体 whoAmI
                    NPC.ai[1] = limb2;
                    Main.npc[limb1].ai[0] = NPC.whoAmI; // 肢体存本体 whoAmI
                    Main.npc[limb2].ai[0] = NPC.whoAmI;
                    NPC.netUpdate = true;
                    Limb1Active = true;
                    Limb2Active = true;
                }
            }
        }
        public override void AI()
        {
            NPC.rotation += 0.4f;
            //检测肢体状态
            DetectLimb();
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
        private void DoIDLE(Player target)
        {
            if(BossStateTimer < 600)
            {
                MoveIDLE(target);
            }
            else
            {
                RandomBossState();
            }
        }
        private void DoPunch(Player target)
        {
            if(BossStateTimer < 200)
            {
                BodyMove(target);
                if(Limb1Active || Limb2Active)
                {
                    NPC limb1 = Main.npc[(int)NPC.ai[0]];
                    NPC limb2 = Main.npc[(int)NPC.ai[1]];
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
                            if(RockRelease < 3 && ProjectileTimer % 30 == 0)
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
                                        ,limb2.whoAmI
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
            else
            {
                RandomBossState();
            }
        }
        private void DoLimbClipTel(Player target)
        {
            if (BossStateTimer < 100)
            {
                NPC.velocity = Vector2.Zero;

                return;
            }
            else if (BossStateTimer < 140)
            {
                Vector2 TelPos1 = new Vector2(target.Center.X - 100, target.Center.Y);
                if(Time < 30)
                {
                    Time++;
                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDust(TelPos1,NPC.width/2, NPC.height/2, DustID.Stone,Scale:2f);
                    }
                }
                
            }
        }
        private void MoveIDLE(Player target)
        {
            if (BossStateTimer < 200)
            {
                limb1Follow(target);
                limb2Follow(target);
                BodyMove(target);
            }
            else
            {
                RandomBossState();
            }
        }
        private void BodyMove(Player target)
        {
            Vector2 Dir = target.Center - NPC.Center;
            if (Dir != Microsoft.Xna.Framework.Vector2.Zero)
            {
                Dir.Normalize();
            }
            float ChaseSpeed = 4f;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Dir * ChaseSpeed, 0.8f);
        }
        private void limb1Follow(Player player)
        {
            if (Limb1Active)
            {
                NPC limb1 = Main.npc[(int)NPC.ai[0]];
                if (TryGetLimb((int)NPC.ai[0], out limb1))
                {
                    Vector2 limb1Pos = new Vector2(NPC.Center.X - 100, NPC.Center.Y + 25);
                    float LerpSpeed = 4f;
                    Vector2 LerpVel1 = (limb1Pos - limb1.Center);
                    if (LerpVel1 != Vector2.Zero)
                    {
                        LerpVel1.Normalize();
                    }
                    limb1.velocity = Vector2.Lerp(limb1.velocity, LerpVel1 * LerpSpeed, 0.4f);
                }
            }
        }
        private void limb2Follow(Player player)
        {
            if (Limb2Active)
            {
                NPC limb2 = Main.npc[(int)NPC.ai[1]];
                if (TryGetLimb((int)NPC.ai[1], out limb2))
                {
                    Vector2 limb2Pos = new Vector2(NPC.Center.X + 100, NPC.Center.Y + 25);
                    float LerpSpeed = 4f;
                    Vector2 LerpVel1 = (limb2Pos - limb2.Center);
                    if (LerpVel1 != Vector2.Zero)
                    {
                        LerpVel1.Normalize();
                    }
                    limb2.velocity = Vector2.Lerp(limb2.velocity, LerpVel1 * LerpSpeed, 0.4f);
                }
            }
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
            BossStateTimer = 0;
            ProjectileTimer = 0;
            BossCoolDown = 15;
            if (NextState != null)
            {
                CurrentBossState = NextState.Value;
                return;
            }
            BossState next;
            do
            {
                next = (BossState)Main.rand.Next(0, 2);
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
                }
            }
        }
        private void DetectLimb()
        {
            if (NPC.ai[0] > 0 && NPC.ai[1] > 0 && Limb1Active == true && Limb2Active == true)
            {
                NPC limb1 = Main.npc[(int)NPC.ai[0]];
                NPC limb2 = Main.npc[(int)NPC.ai[1]];
                if (!limb1.active)
                {
                    NPC.ai[0] = 0;
                    Limb1Active = false;
                }
                if (!limb2.active)
                {
                    NPC.ai[1] = 0;
                    Limb2Active = false;
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
