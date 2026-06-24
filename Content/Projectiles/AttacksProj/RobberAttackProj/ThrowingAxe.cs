using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.NPCs.Attacks.RobberAttack;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj
{
    public class ThrowingAxe : ModProjectile
    {
        private int ProjectileTimer;

        // 弹幕三个阶段的状态枚举，比用数值判断好维护
        private enum AxePhase
        {
            Decelerating, // 飞行减速阶段
            Hovering,     // 空中悬停2秒阶段
            Returning     // 返回NPC手中阶段
        }
        private AxePhase currentPhase = AxePhase.Decelerating;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = false; // 不撞墙，避免回收前意外消失
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1; // 无限穿透，减速/悬停阶段可以多次命中玩家
            Projectile.width = 34;
            Projectile.height = 28;
            Projectile.timeLeft = 300; // 延长到4秒，足够覆盖整个流程（原200可能临界）
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            // 先获取所属的NPC，判断NPC是否存活，避免空引用报错
            NPC ownerNPC = Main.npc[(int)Projectile.ai[0]];
            if (!ownerNPC.active || ownerNPC.type != ModContent.NPCType<AxeRobber>()) // ⚠️ 这里的AxeRobber要换成你自己的NPC类名
            {
                Projectile.Kill();
                return;
            }

            switch (currentPhase)
            {
                // ===== 阶段1：飞行逐渐减速 =====
                case AxePhase.Decelerating:
                    // 每帧速度乘以减速系数，越飞越慢，0.97f可调整（越小减速越快）
                    Projectile.velocity *= 0.98f;
                    // 飞行时持续旋转，保持斧头投掷的动感
                    Projectile.rotation += 0.4f;

                    // 速度足够小时切换到悬停阶段，0.5f是速度阈值，可调整
                    if (Projectile.velocity.Length() < 0.5f)
                    {
                        Projectile.velocity = Vector2.Zero; // 完全停下
                        currentPhase = AxePhase.Hovering;
                        ProjectileTimer = 0; // 重置计时器，用来计2秒悬停时间
                    }
                    break;

                // ===== 阶段2：空中悬停2秒 =====
                case AxePhase.Hovering:
                    Projectile.velocity = Vector2.Zero; // 保持静止
                    Projectile.rotation += 0.3f; // 悬停时缓慢旋转，比飞行时稍慢
                    ProjectileTimer++;

                    // 泰拉瑞亚默认60帧/秒，120帧=2秒，想停更久就调大这个值
                    if (ProjectileTimer >= 120)
                    {
                        currentPhase = AxePhase.Returning;
                        // 可选：返回阶段关闭对玩家的伤害，避免回收时误触
                        // Projectile.hostile = false;
                    }
                    break;

                // ===== 阶段3：返回NPC手中 =====
                case AxePhase.Returning:
                    Vector2 toNPC = ownerNPC.Center - Projectile.Center;

                    // 足够接近NPC时，触发回收效果并销毁弹幕（会触发OnKill的音效和铁屑）
                    if (toNPC.Length() < 20f)
                    {
                        Projectile.Kill();
                        // 可选：加个回收音效，比如SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
                    }
                    else
                    {
                        // 两种返回方式选一种，注释掉另一种即可：
                        // 方式1：匀速飞回，速度12f可调整，想快就调大
                        // Projectile.velocity = Vector2.Normalize(toNPC) * 12f;

                        // 方式2：平滑插值飞回（推荐），越靠近NPC速度越快，更有手感
                        // 0.08f是插值系数，越大返回越快
                        Projectile.Center = Vector2.Lerp(Projectile.Center, ownerNPC.Center, 0.08f);

                        // 旋转朝向NPC，+MathHelper.PiOver4是斧头贴图的朝向偏移，方向不对就调整这个值
                        Projectile.rotation = toNPC.ToRotation() + MathHelper.PiOver4;
                    }
                    break;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 原逻辑不变，刚好作为回到手上的特效
            SoundEngine.PlaySound(SoundID.Item127, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.t_LivingWood);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 原拖尾逻辑不变，返回阶段移动时会自动显示拖尾
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = Color.Gray;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color color = trailColor * progress * 0.7f;
                Main.spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale * (0.8f + progress * 0.4f), SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}