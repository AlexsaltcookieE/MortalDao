using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.RendererSystem;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class LongSwordProj : ModProjectile
    {
        public override string Texture => "MortalDao/Content/Items/MeleeWeapons/LongSwordProj";
        Player player => Main.player[Projectile.owner];//获取玩家
        private float RAmount = 0.32f;
        //
        //碰撞逻辑
        private float swingRadius = 170f;
        private float fanAngle = MathHelper.PiOver2; // 扇形张角，比如 90°，你自己换
        //
        private Vector2 DrawCenter;
        //
        private int soundTimer = 0;

        public override void SetDefaults()
        {
            Projectile.ownerHitCheck = true;
            Projectile.localNPCHitCooldown = -1;
            //
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 5;
            Projectile.tileCollide = false;//穿墙
            Projectile.aiStyle = -1;//不使用原版AI
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;//无限穿透
            Projectile.ignoreWater = true;//无视液体
            Projectile.timeLeft = 40;//弹幕 趋势 的时间
            base.SetDefaults();
        }
        public override void SetStaticDefaults()//以下照抄
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = 8;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = 60f;
            float halfW = 15f;
            Vector2 origin = Projectile.Center;
            int segPer = 6;

            for (int j = 0; j < Projectile.oldRot.Length - 1; j++)
            {
                float a0 = MathHelper.WrapAngle(Projectile.oldRot[j]);
                float a1 = MathHelper.WrapAngle(Projectile.oldRot[j + 1]);
                if (Math.Abs(a1 - a0) > MathHelper.Pi)
                    a1 += Math.Sign(a0 - a1) * MathHelper.TwoPi;

                for (int k = 0; k < segPer; k++)
                {
                    float t0 = k / (float)segPer;
                    float t1 = (k + 1) / (float)segPer;
                    float aStart = MathHelper.Lerp(a0, a1, t0);
                    float aEnd = MathHelper.Lerp(a0, a1, t1);

                    Vector2 p0 = origin + new Vector2(0, -radius).RotatedBy(aStart);
                    Vector2 p1 = origin + new Vector2(0, -radius).RotatedBy(aEnd);

                    float hitDist = 0f; // 6参版本要 ref float，不能 out _
                    if (Collision.CheckAABBvLineCollision(
                        targetHitbox.TopLeft(),     // aabbPosition  ✅ Vector2
                        targetHitbox.Size(),        // aabbDimensions ✅ Vector2（替换原来的 Width, Height 两个 int）
                        p0, p1,
                        halfW * 2f,                  // 注意：这个参数名叫 lineWidth（全宽），不是 halfW
                        ref hitDist))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // 假设你有一个 OnHitNPC 方法
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 1. 定义闪光颜色 (根据你的刀身颜色调整)
            Color flashColor = Color.White; // 青白色，或者换成 Color.Purple
            int starCount = 5;
            for (int i = 0; i < starCount; i++)
            {
                // 随机生成十字形的方向
                Vector2 direction = Main.rand.NextVector2Unit();
                Dust d = Dust.NewDustPerfect(
                    target.Center,
                    DustID.ShimmerSpark,
                    direction * Main.rand.NextFloat(3f, 6f), // 稍微带点推力
                    100,
                    Color.White, // 星星通常用纯白色最亮
                    1.7f
                );
                d.noGravity = true;
            }
            // 3. 环境光照染色 (让背景也闪一下)
            Lighting.AddLight(target.Center, flashColor.R / 255f * 1.5f, flashColor.G / 255f * 1.5f, flashColor.B / 255f * 1.5f);
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0)  // ai[1] 当"是否已初始化"标志
            {
                Projectile.ai[0] = Projectile.rotation; // 起始角
                Projectile.ai[1] = 1;
            }
            //
            Player player = Main.player[Projectile.owner];
            if (player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            
            player.heldProj = Projectile.whoAmI;
            Vector2 handPos = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full,Projectile.rotation - MathHelper.Pi);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi);
            DrawCenter = handPos;
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);
            Projectile.rotation += RAmount * player.direction;
            
            float radius = 170f; // 与 swingRadius 一致
            Vector2 offset = new Vector2(0, -radius).RotatedBy(Projectile.rotation);
            Projectile.Center = handPos + offset;

            //声音
            soundTimer++;
            if (soundTimer >= 20)
            {
                SoundEngine.PlaySound(SoundID.Item19, Projectile.position); // Item19 是回旋镖投掷的典型音效
                soundTimer = 0;
            }
        }
        public override bool ShouldUpdatePosition()
        {
            return false;//让弹幕位置不受速度影响
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp, // 圆形纹理一定要 Linear
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
            float radius = 170f;
            float width = 60f;
            int trailLen = Projectile.oldRot.Length;
            int segmentsPerOldRot = 6; // 每个 oldRot 插 6 段
            var ve = new List<Vertex>();
            for (int j = 0; j < Projectile.oldRot.Length - 1; j++)
            {
                float a0 = MathHelper.WrapAngle(Projectile.oldRot[j]);
                float a1 = MathHelper.WrapAngle(Projectile.oldRot[j + 1]);
                // 防止跨 ±π 翻转
                if (Math.Abs(a1 - a0) > MathHelper.Pi)
                    a1 += Math.Sign(a0 - a1) * MathHelper.TwoPi;
                for (int k = 0; k <= segmentsPerOldRot; k++)
                {
                    float t = k / (float)segmentsPerOldRot;
                    float a = MathHelper.Lerp(a0, a1, t);
                    float progress = (j + t) / (Projectile.oldRot.Length - 1);
                    Vector2 pos = DrawCenter + new Vector2(0, -radius).RotatedBy(a);
                    Vector2 screenPos = pos - Main.screenPosition;
                    Color c = Color.Lerp(Color.Black, Color.Gray, progress) * (0.8f + 0.8f * progress);
                    ve.Add(new Vertex(screenPos + new Vector2(0, -width).RotatedBy(a), new Vector3(progress, 1, 1), c));
                    ve.Add(new Vertex(screenPos + new Vector2(0, width).RotatedBy(a), new Vector3(progress, 0, 1), c));
                }
            }
            if (ve.Count >= 3)
            {

                gd.Textures[0] = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/MelleeWeaponsProj/Cyan_BladeProj").Value;
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);
            }
            sb.End();
            // 原刀身
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            Main.spriteBatch.Draw(
                TextureAssets.Projectile[Type].Value,
                DrawCenter - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation - MathHelper.PiOver4,
                new Vector2(0,68),
                2f,
                SpriteEffects.None,
                0
            );
            return false;
        }
      
    }
}
