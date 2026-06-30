using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class YueMaidenSwordStab : ModProjectile
    {
        //帧段
        private const int Extend_duration = 8;
        private const int Pause_duration = 2;
        private const int Retract_duration = 8;
        //偏移
        private const float Start_Offset = 40f;
        //总持续时间
        private const int Total_duration = Extend_duration + Pause_duration + Retract_duration;
        //刺出距离
        private const float Max_Distance = 45f;
        //
        private bool ShootBlade = false;
        //
        private Vector2 drawPosSave;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            
            Projectile.penetrate = -1;
            Projectile.timeLeft = Total_duration;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // 无敌帧时间
            Projectile.velocity = Vector2.Zero; // 手动控制位置，不使用物理速度
        }
        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(3))
                {
                    // 粒子位置在弹幕中心附近随机偏移
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                    // 粒子速度：跟随弹幕飞行方向+随机扩散
                    Vector2 dustVel = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);

                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.WhiteTorch, dustVel.X, dustVel.Y,
                        100, Color.White, Main.rand.NextFloat(3f, 4f));
                    dust.noGravity = true; // 关闭重力，粒子不会下落
                    dust.noLight = true;   // 不受环境光影响，始终保持纯白
                    dust.alpha = 50;       // 透明度，数值越低越亮（0=完全不透明，255=全透明）
                }
            }
            Player owner = Main.player[Projectile.owner];
            Vector2 ownerHandPos = new Vector2(owner.Center.X, owner.Center.Y - 1);
            owner.heldProj = Projectile.whoAmI;
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            //第一帧锁定位置
            if (Projectile.ai[0] == 0)
            {
                Vector2 mouseWorld = Main.MouseWorld;//鼠标位置
                Vector2 stabDirection = mouseWorld - ownerHandPos;
                if (stabDirection != Vector2.Zero)
                {
                    stabDirection.Normalize();
                }
                else
                {
                    stabDirection = Vector2.UnitX * owner.direction;
                }
                //存方向
                Projectile.ai[1] = stabDirection.X;
                Projectile.ai[2] = stabDirection.Y;
                Projectile.rotation = stabDirection.ToRotation() + MathHelper.PiOver2;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                Projectile.ai[0] = 1;
            }
            Vector2 fixedDirection = new Vector2(Projectile.ai[1], Projectile.ai[2]);//锁定刺向
            int elapsedFrames = Total_duration - Projectile.timeLeft;
            float progress = 0;
            if (elapsedFrames < Extend_duration)
            {
                // 刺出阶段：0~14帧，从玩家位置线性移动到最远点
                progress = (float)elapsedFrames / Extend_duration;
            }
            else if (elapsedFrames < Extend_duration + Pause_duration)
            {
                // 停顿阶段：15~19帧，保持在最远点不动
                progress = 1f;
                if (!ShootBlade)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),drawPosSave, Projectile.velocity, ModContent.ProjectileType<YueMaidenSwordBlade>(),Projectile.damage, 1f, owner.whoAmI);
                    ShootBlade = true;
                }
            }
            else
            {
                // 收回阶段：20~34帧，从最远点线性移回玩家位置
                int retractFrames = elapsedFrames - Extend_duration - Pause_duration;
                progress = 1f - (float)retractFrames / Retract_duration;
            }
            Vector2 basePosition = ownerHandPos - fixedDirection * Start_Offset;
            drawPosSave = basePosition + fixedDirection * Max_Distance * progress;

            Projectile.Center = basePosition + fixedDirection * (Max_Distance * progress + 110f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            // 原点设为贴图底部中心：对应剑柄位置，保证剑柄对齐玩家
            Vector2 drawOrigin = new Vector2(texture.Width / 2f, texture.Height);
            Vector2 fixedDirection = new Vector2(Projectile.ai[1], Projectile.ai[2]);
            // 绘制位置要减去屏幕偏移
            Vector2 drawPos = drawPosSave - Main.screenPosition;
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false; // 阻止默认绘制（默认原点是贴图中心，会偏移）
        }

    }
}
