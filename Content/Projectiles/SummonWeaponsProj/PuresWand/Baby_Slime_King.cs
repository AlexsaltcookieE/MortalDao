using Microsoft.Xna.Framework;
using MortalDao.Content.Buffs.SummonWeaponsBuffs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand
{
    public class Baby_Slime_King : ModProjectile
    {
        const float Gravity = 0.3f; //重力
        const float MaxFallSpeed = 10f; //最大下落速度
        const float BounceVelY = -5.5f; //弹跳衰减
        const float MoveSpeed = 15f; //水平移动速度
        const float FollowDistance = 120f; //跟随距离
        const float TeleportDistance = 800f; //瞬移距离
        const int SearchRange = 400; // 寻敌范围
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;               // 史莱姆一般有 6 帧动画
            Main.projPet[Type] = true;                // 标记为宠物/仆从类
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
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
            DoWalkAnimation();
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
            Projectile.timeLeft = 20;
            if (Math.Abs(Projectile.velocity.X) > 0.1f)
            {
                Projectile.spriteDirection = Math.Sign(-Projectile.velocity.X);
            }
            //重力
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxFallSpeed)
            {
                Projectile.velocity.Y = MaxFallSpeed;
            }
            //寻敌系统
            NPC target = FindTarget(player);
            if (target != null)
            {
                //有敌人时朝敌人方向跳过去
                float DirToTarget = Math.Sign(target.Center.X - Projectile.Center.X);
                // 只有在接近地面（或正在上升很少）时才给新的水平推力
                // 这样可以避免空中反复"修正方向"导致抖动
                float xDist = Math.Abs(target.Center.X - Projectile.Center.X);
                if (xDist > 20f)
                {
                    // 渐进转向
                    Projectile.velocity.X += DirToTarget * 0.55f;
                    if (Math.Abs(Projectile.velocity.X) > MoveSpeed)
                    {
                        Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * MoveSpeed;
                    }
                }
            }
            else
            {
                //没有敌人的时候
                float Dist = Vector2.Distance(player.Center, Projectile.Center);
                if (Dist > TeleportDistance)
                {
                    //距离过远 → 瞬移到玩家附近
                    Projectile.Center = player.Center + new Vector2(0, -30f);
                    Projectile.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f);
                    Projectile.netUpdate = true;
                }
                else if (Dist > FollowDistance)
                {
                    //距离较远 → 加速向玩家靠近
                    float dir = Math.Sign(player.Center.X - Projectile.Center.X);
                    Projectile.velocity.X += dir * 0.45f;
                    if (Math.Abs(Projectile.velocity.X) > MoveSpeed * 0.8f)
                        Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * MoveSpeed * 0.8f;
                }
                else
                {
                    Projectile.velocity.X *= 0.96f;
                }
            }
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;// 史莱姆不穿平台
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(oldVelocity.Y > 0 && Projectile.velocity.Y == 0) // 落地
            {
                Projectile.velocity.Y = BounceVelY; // 弹跳
                Projectile.velocity.X *= 0.8f; // 水平衰减
            }
            return false;
        }
        private NPC FindTarget(Player Owner)
        {
            NPC closest = null;
            float minDist = SearchRange * SearchRange; // 距离平方
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if(!npc.CanBeChasedBy(this.Projectile, ignoreDontTakeDamage: true))
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
        private void DoWalkAnimation()
        {
            //bool IsOnGround = Math.Abs(Projectile.velocity.Y) < 1f;
            //if(IsOnGround && Math.Abs(Projectile.velocity.X) > 0.5f)
            //{
            //    Projectile.frameCounter++;
            //    if (Projectile.frameCounter > 5)
            //    {
            //        Projectile.frameCounter = 0;
            //        Projectile.frame = (Projectile.frame + 1) % 6;
            //    }
            //}
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame == 5)
                {
                    Projectile.frame = 0;
                }
            }
        }
    }
}