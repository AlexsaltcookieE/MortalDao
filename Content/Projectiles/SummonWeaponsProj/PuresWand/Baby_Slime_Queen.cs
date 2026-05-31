using Microsoft.Xna.Framework;
using MortalDao.Content.Buffs.SummonWeaponsBuffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand
{
    public class Baby_Slime_Queen : ModProjectile
    {
        private const float Gravity = 0.3f;
        private const float MaxFallSpeed = 10f;
        private const float BounceVelY = -5.5f;
        private const float SearchRange = 800f; // 寻敌范围
        private const float FollowRange = 300f; // 跟随起始距离
        public AIState CurrentAIState = AIState.Idle;
        private int animationTimer = 0;
        private Vector2 FollowOffsetX;
        private int attackTimer = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 11;               // 史莱姆一般有 12 帧动画
            Main.projPet[Type] = true;                // 标记为宠物/仆从类
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }
        public enum AIState
        {
            Idle = 0,
            FlyFollow = 1,
            Attack = 2,
        }
        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;// 不消失
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = true;// 史莱姆要落地弹跳
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true; // 接触伤害
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            // 基础检查
            if (player == null || !player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            // Buff 检查：缓存类型，避免每帧获取
            int rainbowSlimeBuffType = ModContent.BuffType<RainBowSlime>();
            if (!player.HasBuff(rainbowSlimeBuffType))
            {
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 40;
            NPC target = FindTarget(player);
            UpdateAIState(player, target);
            ExecuteAI(player, target);
            ApplyGravityAndPhysics();
        }
        private void UpdateAIState(Player player, NPC target)
        {
            int total;
            int myIndex = GetMinionIndexAndTotal(out total);
            if (total <= 0) total = 1;
            float distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);
            float dynamicFollowRange = FollowRange + (50f * myIndex);
            // 优先级2：离玩家太远，进入跟随状态
            if (target != null && target.active)
            {
                DoAttack(player, target);
            }
            if (distanceToPlayer > dynamicFollowRange)
            {
                CurrentAIState = AIState.FlyFollow;
            }
            // 优先级3：在玩家附近，进入闲置状态（但不要太近）
            else if (CurrentAIState == AIState.FlyFollow)
            {
                if (player.Center.Y >= Projectile.Center.Y && ((player.Center.X - Projectile.Center.X) >= 10 || (Projectile.Center.X - player.Center.X) >= 10) && distanceToPlayer < dynamicFollowRange - 20f)
                {
                    CurrentAIState = AIState.Idle;
                }
            }
            else
            {
                CurrentAIState = AIState.Idle;
            }
        }
        private void ExecuteAI(Player player, NPC target)
        {
            switch (CurrentAIState)
            {
                case AIState.Idle:
                    DoIdle(player);
                    break;
                case AIState.FlyFollow:
                    DoFollow(player);
                    break;
            }
            // 更新动画
            UpdateAnimation();
        }
        private void DoAttack(Player player, NPC target)
        {
            if (target == null || !target.active)
                return;
            Vector2 dir = target.Center - Projectile.Center;
            // 攻击逻辑：每 60 帧发射一次子弹
            attackTimer++;
            if (attackTimer >= 30)
            {
                attackTimer = 0;
                if (dir != Vector2.Zero)
                    dir.Normalize();
                float projectileSpeed = 30f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * projectileSpeed, ModContent.ProjectileType<Baby_Slime_Gel>(), Projectile.damage, 1f, Projectile.owner);
            }
            int total;
            int myIndex = GetMinionIndexAndTotal(out total);
            if (total <= 0) total = 1;
            if (myIndex % 2 == 1)
            {
                FollowOffsetX = new Vector2(player.Center.X - (30 * (myIndex + 1)), player.Center.Y - 35);
            }
            // 飞向目标
        }
        private void DoIdle(Player player)
        {
            Projectile.direction = player.Center.X >= Projectile.Center.X ? -1 : 1;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.rotation = 0;
            //Walk
            int total;
            int myIndex = GetMinionIndexAndTotal(out total);
            if (total <= 0) total = 1;
            float spacing = 36f;
            float lineOffsetX = (myIndex - (total - 1f) / 2f) * spacing;
            Vector2 targetPos = player.Center + new Vector2(lineOffsetX, -20f);
            Vector2 moveTo = targetPos - Projectile.Center;
            float idleSpeed = 3f;
            float inertia = 20f;
            if (moveTo.Length() > 30f)
            {
                Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + moveTo.X * 0.05f) / inertia;
                if (Projectile.velocity.Y == 0)
                {
                    Projectile.velocity.Y = -2f;
                }
            }
            else
            {
                // 到达目标附近，减速停止
                Projectile.velocity.X *= 0.9f;
            }
        }
        private void DoFollow(Player player)
        {
            int total;
            int myIndex = GetMinionIndexAndTotal(out total);
            if (total <= 0) total = 1;
            // 飞行跟随时直接向玩家飞行
            if (player.Center.X >= Projectile.Center.X)
            {
                FollowOffsetX = new Vector2(player.Center.X - (30 * (myIndex + 1)), player.Center.Y - 35);
            }
            else
            {
                FollowOffsetX = new Vector2(player.Center.X + (30 * (myIndex + 1)), player.Center.Y - 35);
            }
            Vector2 dir = FollowOffsetX - Projectile.Center;
            if (dir != Vector2.Zero)
                dir.Normalize();
            float speed = 10f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.1f);
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;
        }
        private void SwitchState(Player player)
        {
        }
        private void ApplyGravityAndPhysics()
        {
            // 只有在没有强力向上速度时才受重力影响（防止飞行时一直下坠）
            if (Projectile.velocity.Y < MaxFallSpeed)
            {
                Projectile.velocity.Y += Gravity;
            }
            // 限制最大下落速度
            if (Projectile.velocity.Y > MaxFallSpeed)
            {
                Projectile.velocity.Y = MaxFallSpeed;
            }
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (CurrentAIState != AIState.FlyFollow)
            {
                fallThrough = false;// 史莱姆不穿平台
                return true;
            }
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 只有向下碰撞时才弹跳
            if (oldVelocity.Y > 0 && Projectile.velocity.Y == 0)
            {
                Projectile.velocity.Y = BounceVelY;
                Projectile.velocity.X *= 0.8f; // 水平摩擦
            }
            return false; // 不消失
        }
        private NPC FindTarget(Player Owner)
        {
            NPC closest = null;
            float minDist = SearchRange * SearchRange; // 距离平方
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this.Projectile, ignoreDontTakeDamage: true))
                {
                    continue;
                }
                float d = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                if (d < minDist)
                {
                    minDist = d;
                    closest = npc;
                }
            }
            return closest;
        }
        private int GetMinionIndexAndTotal(out int total)
        {
            int owner = Projectile.owner;
            total = 0;
            int index = 0;
            int kingType = ModContent.ProjectileType<Baby_Slime_King>();
            int queenType = ModContent.ProjectileType<Baby_Slime_Queen>();
            int PuerType = ModContent.ProjectileType<Puer_Slime_baby>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != owner)
                    continue;
                if (p.type == kingType || p.type == queenType || p.type == PuerType)
                {
                    if (p.whoAmI == Projectile.whoAmI)
                    {
                        index = total;
                    }
                    total++;
                }
            }
            return index; // 返回 0-based 序号
        }
        private void UpdateAnimation()
        {
            animationTimer++;
            // 根据状态选择动画区间
            int startFrame = 0, endFrame = 4, speed = 5;
            if (CurrentAIState == AIState.FlyFollow)
            {
                startFrame = 6; endFrame = 10; speed = 8; // 飞行动画快一点
            }
            else if (CurrentAIState == AIState.Idle)
            {
                startFrame = 0; endFrame = 4; speed = 5; // 行走/闲置动画慢一点
            }
            if (animationTimer >= speed)
            {
                animationTimer = 0;
                Projectile.frame++;
                if (Projectile.frame > endFrame || Projectile.frame < startFrame)
                {
                    Projectile.frame = startFrame;
                }
            }
        }
    }
}