using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.RendererSystem;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class LongSwordProj : ModProjectile
    {
        private class BladeTrail
        {
            public float angle;      // 这一帧刀身的角度（绝对）
            public float time;       // 用于 fade
        }
        public override string Texture => "MortalDao/Content/Items/MeleeWeapons/LongSwordProj";
        Player player => Main.player[Projectile.owner];//获取玩家
        private float RAmount = 0.32f;
        //
        //
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.tileCollide = false;//穿墙
            Projectile.aiStyle = -1;//不使用原版AI
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;//无限穿透
            Projectile.ignoreWater = true;//无视液体
            Projectile.timeLeft = 20;//弹幕 趋势 的时间
            base.SetDefaults();
        }
        public override void SetStaticDefaults()//以下照抄
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//这一项赋值2可以记录运动轨迹和方向（用于制作拖尾）
            ProjectileID.Sets.TrailCacheLength[Type] = 8;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }

        public override void AI()//模拟&quot;刀&quot;的挥舞逻辑
        {
            Player player = Main.player[Projectile.owner];
            if (player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = player.Center;//绑定玩家和弹幕的位置
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
            Projectile.rotation += RAmount * player.direction;
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
            float radius = 200f;
            float width = 30f;
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
                    Vector2 pos = player.Center + new Vector2(0, -radius).RotatedBy(a);
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
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation - MathHelper.PiOver2,
                new Vector2(0,33.2f),
                2f,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
