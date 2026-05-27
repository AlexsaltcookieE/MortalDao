using Microsoft.Xna.Framework;
using MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs;
using MortalDao.Content.Projectiles.BossProj.GeneralSoulProj;
using MortalDao.Content.Projectiles.RangeWeaponsProj;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.BOSS.GeneralSoul
{
    [AutoloadBossHead]
    public class GeneralSoul : ModNPC
    {
        private int GhostAeraWhoAmI = -1;
        private bool LockNum = false;
        private bool Teleport_Sound = false;
        private bool StateEnd = false;
        private int ProjectTime = 3;
        private int ProjectCoolTimer;
        private bool Thorn = false;
        private int BossEscapeTimer;//逃脱计时器
        private int BossEscapeDelay = 180;//倒数计时器
        private int BossEscapeDistance = 3200;//逃脱距离
        private const int FrameSpeed = 6;
        private int FrameCounter;
        public BossState CurrentBossState = BossState.Idle;
        private int BossStateTimer = 0;
        private int BossCoolDown = 0;
        private bool SummonRes = false;
        private bool Cool;
        public override void SetStaticDefaults()//BOSS预设值
        {
            // Ensure Terraria treats this ModNPC as a real boss for UI/progression behavior.
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            Main.npcFrameCount[Type] = 3;
        }
        public enum BossState
        {
            Idle = 0,
            Aero = 1,
            Hook = 2,
            Black_Hole_Phase = 3
        }
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.width = 20;
            NPC.height = 32;
            NPC.damage = 20;
            NPC.defense = 8;
            NPC.lifeMax = 1200;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 60f;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.scale = 2f;
            Music = MusicLoader.GetMusicSlot(Mod, "Content/Musics/Boss/GeneralSoul");

        }
        public override void AI()
        {
            NPC.netUpdate = true;
            //限制圈
            Player player = Main.player[NPC.target];
            NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            NPC.TargetClosest(faceTarget: false);//选最近玩家 同时不让NPC转向目标方向
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)//如果没有寻找到存活的玩家
            {
                Despawn();
                return;
            }
            float distance = Microsoft.Xna.Framework.Vector2.Distance(NPC.Center, player.Center);
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
            //----------------------------------Ai----------------------------------
            if (CurrentBossState == BossState.Idle || CurrentBossState == BossState.Aero || CurrentBossState == BossState.Hook|| CurrentBossState == BossState.Black_Hole_Phase)
            {
                ProjectCoolTimer++;
            }
            if(BossCoolDown > 0)
            {
                BossCoolDown--;
            }
            else
            {
                BossStateTimer++;
                switch (CurrentBossState)
                {
                    case BossState.Idle:
                        DoIdle(player);
                        break;
                    case BossState.Aero:
                        DoAero(player);
                        break;
                    case BossState.Hook:
                        DoHook(player);
                        break;
                    case BossState.Black_Hole_Phase:
                        SummonBlackHole(player);
                        break;
                }
            }
        }
        public void Despawn()
        {
            NPC.velocity.Y = -10f;
            if (NPC.timeLeft > 10)
            {
                NPC.timeLeft = 10;
            }
        }
        private void RandomBossState(BossState? blockedState = null,BossState? blockedState2 = null,BossState? NextState = null)
        {
            if (GhostAeraWhoAmI != -1 && GhostAeraWhoAmI < Main.maxProjectiles)
            {
                Projectile p = Main.projectile[GhostAeraWhoAmI];
                if (p.active && p.type == ModContent.ProjectileType<Ghost_Restriction_Ring>())
                {
                    p.Kill(); // 杀掉弹幕
                }
            }
            BossStateTimer = 0;
            ProjectCoolTimer = 0;
            BossCoolDown = 15;
            if(NextState != null)
            {
                CurrentBossState = NextState.Value;
                return;
            }
            BossState next;
            do
            {
                next = (BossState)Main.rand.Next(0, 4); // 0..3 for Idle, Aero, Hook, Arrow
            }
            while ((next == CurrentBossState || (blockedState.HasValue && next == blockedState.Value || blockedState2.HasValue && next == blockedState2)));
            CurrentBossState = next;
        }
        public override void OnKill()
        {
            //在BOSS死亡时执行的代码
            //例如：掉落物品、触发事件等
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            //在BOSS受到伤害时执行的代码
            //例如：生成特效、播放声音等
        }

        private void DoIdle(Player target)
        {
            //NPC.velocity.X = target.velocity.X;
            //NPC.velocity.Y = target.velocity.Y;
            //执行Idle状态的行为
            bool isAttacking = (BossStateTimer > 60 && BossStateTimer < 120 && StateEnd == false);
            if (!isAttacking && BossStateTimer <= 10)
            {
                if (!Teleport_Sound)
                {
                    SoundEngine.PlaySound(SoundID.Item6, NPC.Center);
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(10f, 10f);
                        Dust.NewDust(NPC.Center, 0, 0, DustID.GoldFlame, speed.X, speed.Y);
                    }
                    Teleport_Sound = true;
                }
            }
            if (!isAttacking && BossStateTimer >= 20 && BossStateTimer <60)
            {
                
                float offsetX = 140f;
                float offsetY = 0f;
                Vector2 targetPos = new Vector2(target.Center.X + offsetX, target.Center.Y + offsetY);
                NPC.Center = targetPos;
            }
            if (isAttacking)
            {
                if (Teleport_Sound)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(10f, 10f);
                        Dust.NewDust(NPC.Center, 0, 0, DustID.GoldFlame, speed.X, speed.Y);
                        Teleport_Sound = false;
                    }
                }
                NPC.velocity = Vector2.Zero;
                Microsoft.Xna.Framework.Vector2 ProjectileToTarget = target.Center - NPC.Center;
                Microsoft.Xna.Framework.Vector2 ThornVector;
                if (ProjectileToTarget != Microsoft.Xna.Framework.Vector2.Zero)
                {
                    ProjectileToTarget.Normalize();
                }
                if (ProjectCoolTimer >= 80 && !Thorn)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget, ModContent.ProjectileType<Cyan_Dragon_H>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 2f);
                    Thorn = true;

                }
                if (Thorn && ProjectCoolTimer >= 110)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget * 13f, ModContent.ProjectileType<Cyan_Dragon>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 1f);
                    ProjectCoolTimer = 0;
                    Thorn = false;
                    StateEnd = true;
                }
                //例如：站立、播放动画等
            }
            else if(StateEnd)
            {
                StateEnd = false;
                Teleport_Sound = false;
                RandomBossState(NextState: BossState.Black_Hole_Phase);

            }
        }
        private void DoAero(Player target)
        {
            SummonRing();
            if (BossStateTimer <= 2000)
            {
                if(ProjectCoolTimer >= 40 && ProjectTime >= 0)
                {
                    ProjectCoolTimer = 0;
                    ProjectTime--;
                    Microsoft.Xna.Framework.Vector2 ProjectileToTarget = Vector2.Zero;
                    Microsoft.Xna.Framework.Vector2 toTarget = target.Center - NPC.Center;
                    float chaseSpeed = 8f;//速度
                    float SpeedUP = 0.2f;//加速度
                    if (toTarget != Microsoft.Xna.Framework.Vector2.Zero)
                    {
                        toTarget.Normalize();
                    }
                    Microsoft.Xna.Framework.Vector2 ChaseVelocity = toTarget * chaseSpeed;//向量
                    NPC.velocity = Microsoft.Xna.Framework.Vector2.Lerp(NPC.velocity, ChaseVelocity, SpeedUP);//NPC的速度
                    Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center,ProjectileToTarget, ModContent.ProjectileType<Ghost_aero>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 1f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget, ModContent.ProjectileType<Ghost_aero>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 2f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget, ModContent.ProjectileType<Ghost_aero>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 3f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget, ModContent.ProjectileType<Ghost_aero>(), 10, 1f, Main.myPlayer, NPC.whoAmI, 4f);
                    foreach(Projectile proj in Main.projectile)
                    {
                        if(proj != null && proj.type == ModContent.ProjectileType<Ghost_aero>())
                        {
                            SoundEngine.PlaySound(SoundID.Item45, proj.Center);
                        }
                    }
                    if (ProjectTime == 0)
                    {
                        ProjectTime = 3;
                        SummonRes = false;
                        BossCoolDown = 60;
                        RandomBossState(blockedState:BossState.Black_Hole_Phase);
                    }
                }
            }
        }
        private void DoHook(Player target)
        {
            SummonRing();
            if (BossStateTimer < 400)
            {
                NPC.velocity = NPC.velocity * 0;
                if(ProjectTime != 6 && !LockNum)
                {
                    ProjectTime = 6;
                    LockNum = true;
                }
                if (ProjectCoolTimer > 80 && ProjectTime == 6 && !target.HasBuff(ModContent.BuffType<Ghost_Hooked>()))
                {
                    ProjectTime--;
                    RandArrow();
                }
                if(ProjectCoolTimer > 60 && ProjectTime == 4 && !target.HasBuff(ModContent.BuffType<Ghost_Hooked>()))
                {
                    ProjectTime--;
                    RandArrow();
                }
                if (ProjectCoolTimer > 60 && ProjectTime == 2 && !target.HasBuff(ModContent.BuffType<Ghost_Hooked>()))
                {
                    ProjectTime--;
                    RandArrow();
                }
                    if (ProjectCoolTimer > 100 && ProjectTime >= 1 && !target.HasBuff(ModContent.BuffType<Ghost_Hooked>()))        
                {
                    ProjectCoolTimer = 0;
                    ProjectTime--;
                    Microsoft.Xna.Framework.Vector2 ProjectileToTarget = Vector2.Normalize(target.Center - NPC.Center);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, ProjectileToTarget * 40f, ModContent.ProjectileType<Ghost_Hook>(), 10, -100f, Main.myPlayer, NPC.whoAmI, 1f);
                }
            }
            else
            {
                if (!target.HasBuff(ModContent.BuffType<Ghost_Hooked>()))
                {
                    ProjectTime = 3;
                    LockNum = false;
                    SummonRes = false;
                    RandomBossState(BossState.Idle);
                }
            }
        }
        public void SummonBlackHole(Player target)
        {
            SummonRing();

            // 1. 精准查找属于这个Boss的黑洞（通过 ai[0] 识别）
            Projectile blackHole = null;
            foreach (Projectile proj in Main.projectile)
            {
                // 关键点：proj.ai[0] == NPC.whoAmI 确保只认领自己的黑洞
                if (proj.active && proj.type == ModContent.ProjectileType<Black_Hole>() && proj.ai[0] == NPC.whoAmI)
                {
                    blackHole = proj;
                    break;
                }
            }
            // 2. 阶段进行中 (BossStateTimer < 700)
            if (BossStateTimer < 700)
            {
                // 如果黑洞不存在，且还没生成过，则生成它
                if (blackHole == null && ProjectTime == 3)
                {
                    // 生成黑洞，并把Boss的whoAmI传给黑洞的ai[0]
                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                        new Vector2(NPC.Center.X, NPC.Center.Y - 140f),
                        Vector2.Zero,
                        ModContent.ProjectileType<Black_Hole>(),
                        10, -100f, Main.myPlayer, NPC.whoAmI, 1f); // 注意最后的 1f 是 ai[1]
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X, NPC.Center.Y), Vector2.Zero, ModContent.ProjectileType<Bow_of_OverLord>(), 0, 0,Main.myPlayer,NPC.whoAmI,1f);
                    ProjectTime--;
                    return; // 生成后返回，下一帧再移动
                }

                // 如果黑洞存在，执行环绕逻辑
                if (blackHole != null)
                {   //发射弹幕
                    if(ProjectCoolTimer> 80 && Cool == false)
                    {
                        ProjectCoolTimer = 0;
                        Cool = true;
                    }
                    if(ProjectCoolTimer > 40)
                    {
                        ProjectCoolTimer = 0;
                        Vector2 baseDirection = blackHole.Center - NPC.Center;
                        if (baseDirection != Vector2.Zero)
                        {
                            baseDirection.Normalize();
                        }
                        float spreadAngle = MathHelper.ToRadians(30f);
                        float spreadAngle2 = MathHelper.ToRadians(60f);
                        Vector2 dirLeft = RotateVector(baseDirection, -spreadAngle);
                        Vector2 dirCenter = baseDirection;
                        Vector2 dirRight = RotateVector(baseDirection, spreadAngle);
                        Vector2 dirLeft2 = RotateVector(baseDirection, -spreadAngle2);
                        Vector2 dirRight2 = RotateVector(baseDirection, spreadAngle2);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dirLeft * 20f, ModContent.ProjectileType<OverLord_Arrow>(), 1, 0f, Main.myPlayer, 1f, 0f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dirCenter * 20f, ModContent.ProjectileType<OverLord_Arrow>(), 1, 0f, Main.myPlayer, 1f, 0f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dirRight * 20f, ModContent.ProjectileType<OverLord_Arrow>(), 1, 0f, Main.myPlayer, 1f, 0f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dirLeft2 * 20f, ModContent.ProjectileType<OverLord_Arrow>(), 1, 0f, Main.myPlayer, 1f, 0f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dirRight2 * 20f, ModContent.ProjectileType<OverLord_Arrow>(), 1, 0f, Main.myPlayer,1f, 0f);
                    }
                    // --- 环绕参数 ---
                    float orbitRadius = 600f;      // 固定环绕半径（离黑洞的距离）
                    float totalRotation = 360f;    // 转一圈（360度）
                    float duration = 700f;         // 700帧转完
                    // 计算当前进度对应的角度（弧度制）
                    float progress = BossStateTimer / duration;
                    float angle = MathHelper.ToRadians(progress * totalRotation);
                    // 计算目标位置：黑洞位置 + 偏移量
                    Vector2 orbitPosition = blackHole.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
                    // 平滑移动到目标位置
                    Vector2 moveTo = orbitPosition - NPC.Center;
                    float speed = 15f;    // 移动速度（不要太大，否则会冲过头）
                    float inertia = 20f;  // 惯性

                    // 如果距离非常近，直接瞬移到目标点，防止抖动
                    if (moveTo.Length() < speed)
                    {
                        NPC.Center = orbitPosition;
                        NPC.velocity = Vector2.Zero;
                    }
                    else
                    {
                        NPC.velocity = (NPC.velocity * (inertia - 1) + moveTo.SafeNormalize(Vector2.Zero) * speed) / inertia;
                    }
                }
                else
                {
                    // 如果黑洞意外消失，Boss停止不动
                    NPC.velocity = Vector2.Zero;
                }
            }
            // 3. 阶段结束 (BossStateTimer >= 700)
            else
            {
                // 杀掉属于这个Boss的黑洞
                if (blackHole != null)
                {
                    blackHole.Kill();
                }

                // 重置状态
                SummonRes = false;
                ProjectTime = 3;
                Cool = false;
                RandomBossState();
            }
        }
        public override void FindFrame(int frameHeight)
        {
            FrameCounter++;
            if (FrameCounter >= FrameSpeed)
            {
                FrameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type])
                {
                    NPC.frame.Y = 0;
                }

            }
        }
        private Vector2 RotateVector(Vector2 vector, float radians)
        {
            return new Vector2(
                vector.X * (float)Math.Cos(radians) - vector.Y * (float)Math.Sin(radians),
                vector.X * (float)Math.Sin(radians) + vector.Y * (float)Math.Cos(radians)
            );
        }
        public void SummonRing()
        {
            if (!SummonRes)
            {
                Projectile projR = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Ghost_Restriction_Ring>(), 0, 0f, Main.myPlayer);
                //projR.ai[0] = NPC.whoAmI;
                //GhostAeraWhoAmI = projR.whoAmI;
                if (projR != null && projR.active)
                {
                    projR.ai[0] = NPC.whoAmI;
                    GhostAeraWhoAmI = projR.whoAmI;
                }
                else
                {
                    GhostAeraWhoAmI = -1; // 生成失败则重置
                }
                SummonRes = true;
            }
        }
        public void RandArrow()
        {
            int RandDir = Main.rand.Next(0, 5);
            if (RandDir == 1)
            {
                Vector2 ArrowPos1 = new Vector2(NPC.Center.X + 1400, NPC.Center.Y + 1000);
                Vector2 ArrowPos15 = new Vector2(NPC.Center.X + 2400, NPC.Center.Y + 1000);
                Vector2 ArrowPos2 = new Vector2(NPC.Center.X + 1200, NPC.Center.Y + 1000);
                Vector2 ArrowPos3 = new Vector2(NPC.Center.X + 1000, NPC.Center.Y + 1000);
                Vector2 ArrowPos4 = new Vector2(NPC.Center.X + 800, NPC.Center.Y + 1000);
                Vector2 ArrowPos5 = new Vector2(NPC.Center.X + 600, NPC.Center.Y + 1000);
                Vector2 ArrowPos6 = new Vector2(NPC.Center.X + 400, NPC.Center.Y + 1000);
                Vector2 ArrowPos7 = new Vector2(NPC.Center.X + 200, NPC.Center.Y + 1000);
                Vector2 ArrowPos8 = new Vector2(NPC.Center.X + 0, NPC.Center.Y + 1000);
                Vector2 ArrowPos9 = new Vector2(NPC.Center.X + 1600, NPC.Center.Y + 1000);
                Vector2 ArrowPos10 = new Vector2(NPC.Center.X + 1800, NPC.Center.Y + 1000);
                Vector2 ArrowPos11 = new Vector2(NPC.Center.X + 2000, NPC.Center.Y + 1000);
                Vector2 ArrowPos12 = new Vector2(NPC.Center.X - 200, NPC.Center.Y + 1000);
                Vector2 ArrowPos13 = new Vector2(NPC.Center.X - 400, NPC.Center.Y + 1000);
                Vector2 ArrowPos14 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 1000);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos1, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos2, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos3, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos4, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos5, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos6, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos7, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos8, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos9, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos10, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos11, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos12, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos13, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos14, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos15, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, -15f);
            }
            else if(RandDir == 2)
            {       
                Vector2 ArrowPos1 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 1000);
                Vector2 ArrowPos2 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 800);
                Vector2 ArrowPos3 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 400);
                Vector2 ArrowPos4 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 200);
                Vector2 ArrowPos5 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 0);
                Vector2 ArrowPos6 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y - 200);
                Vector2 ArrowPos7 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y - 400);
                Vector2 ArrowPos8 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y - 600);
                Vector2 ArrowPos9 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y - 800);
                Vector2 ArrowPos10 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y - 1000);
                Vector2 ArrowPos11 = new Vector2(NPC.Center.X + 2200, NPC.Center.Y + 600);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos1, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos2, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos3, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos4, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos5, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos6, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos7, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos8, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos9, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos10, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos11, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, -15f, 0f);
            }
            else if(RandDir == 3)
            {
                Vector2 ArrowPos1 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 1000);
                Vector2 ArrowPos2 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 800);
                Vector2 ArrowPos3 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 400);
                Vector2 ArrowPos4 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 200);
                Vector2 ArrowPos5 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 0);
                Vector2 ArrowPos6 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y - 200);
                Vector2 ArrowPos7 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y - 400);
                Vector2 ArrowPos8 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y - 600);
                Vector2 ArrowPos9 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y - 800);
                Vector2 ArrowPos10 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y - 1000);
                Vector2 ArrowPos11 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 600);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos1, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos2, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos3, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos4, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos5, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos6, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos7, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos8, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos9, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos10, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos11, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, 0f);
            }
            else if (RandDir == 4)
            {
                Vector2 ArrowPos1 = new Vector2(NPC.Center.X - 1400, NPC.Center.Y + 1000);
                Vector2 ArrowPos2 = new Vector2(NPC.Center.X - 1200, NPC.Center.Y + 1000);
                Vector2 ArrowPos3 = new Vector2(NPC.Center.X - 1000, NPC.Center.Y + 1000);
                Vector2 ArrowPos4 = new Vector2(NPC.Center.X - 800, NPC.Center.Y + 1000);
                Vector2 ArrowPos5 = new Vector2(NPC.Center.X - 600, NPC.Center.Y + 1000);
                Vector2 ArrowPos6 = new Vector2(NPC.Center.X - 400, NPC.Center.Y + 1000);
                Vector2 ArrowPos7 = new Vector2(NPC.Center.X - 200, NPC.Center.Y + 1000);
                Vector2 ArrowPos8 = new Vector2(NPC.Center.X - 0, NPC.Center.Y + 1000);
                Vector2 ArrowPos9 = new Vector2(NPC.Center.X - 1600, NPC.Center.Y + 1000);
                Vector2 ArrowPos10 = new Vector2(NPC.Center.X - 1800, NPC.Center.Y + 1000);
                Vector2 ArrowPos11 = new Vector2(NPC.Center.X - 2000, NPC.Center.Y + 1000);
                Vector2 ArrowPos12 = new Vector2(NPC.Center.X + 200, NPC.Center.Y + 1000);
                Vector2 ArrowPos13 = new Vector2(NPC.Center.X + 400, NPC.Center.Y + 1000);
                Vector2 ArrowPos14 = new Vector2(NPC.Center.X - 2200, NPC.Center.Y + 1000);
                Vector2 ArrowPos15 = new Vector2(NPC.Center.X - 2400, NPC.Center.Y + 1000);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos1, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos2, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos3, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos4, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos5, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos6, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos7, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos8, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos9, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos10, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos11, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos12, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos13, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos14, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), ArrowPos15, Vector2.Zero, ModContent.ProjectileType<Milion_Arrow>(), 10, 1f, Main.myPlayer, 15f, -15f);
            }
        }

    }
}
