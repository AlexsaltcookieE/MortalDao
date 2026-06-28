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
    public class PeachWoodBladeProj : ModProjectile
    {
        private class BladeTrail
        {
            public float angle;      // 这一帧刀身的角度（绝对）
            public float time;       // 用于 fade
        }
        //private List<BladeTrail> _trail = new List<BladeTrail>();
        //private const int MaxTrail = 14;   // 你想要多长


        public override string Texture => "MortalDao/Content/Items/MeleeWeapons/RoughPeachWoodSword";
        Player player => Main.player[Projectile.owner];//获取玩家
        private int MaxRotation;
        private bool CanForce = false;
        private int ProjectileTimer = 0;
        private int BladeTime = 0;
        private bool PlaySound;
        private int hitbox;
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
            ProjectileID.Sets.TrailCacheLength[Type] = 15;//这一项代表记录的轨迹最多能追溯到多少帧以前
            base.SetStaticDefaults();
        }

        public override void AI()//模拟&quot;刀&quot;的挥舞逻辑
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 80;      // 关键
            player.itemAnimation = 80; // 关键
            Projectile.Center = player.Center;//绑定玩家和弹幕的位置
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
            if (MaxRotation < 9 && !CanForce)
            {
                Hit(player);
                Projectile.rotation += RAmount * player.direction;//弹幕旋转角度
                MaxRotation++;
            }
            else if(ProjectileTimer < 60 && !CanForce)
            {
                ProjectileTimer++;
            }
            else if(ProjectileTimer >= 60 && !CanForce)
            {
                MaxRotation = 0;
                ProjectileTimer = 0;
                CanForce = true;
            }
            else if(CanForce)
            {
                ProjectileTimer++;
                if(BladeTime == 0 && ProjectileTimer > 30) 
                {
                    if (!PlaySound)
                    {
                        Hit(player);
                        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                        PlaySound = true;
                    }
                    Projectile.rotation -= RAmount * player.direction;
                    MaxRotation++;
                    if (MaxRotation > 7)
                    {
                        PlaySound = false;
                        BladeTime++;
                        ProjectileTimer = 0;
                        MaxRotation = 0;
                        PForce(player);
                    }
                }
                if (BladeTime == 1 && ProjectileTimer > 20)
                {
                    if (!PlaySound)
                    {
                        Hit(player);
                        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                        PlaySound = true;
                    }
                    Projectile.rotation += RAmount * player.direction;
                    MaxRotation++;
                    if (MaxRotation > 7)
                    {
                        PlaySound = false;
                        BladeTime++;
                        ProjectileTimer = 0;
                        MaxRotation = 0;
                        PForce(player);
                    }
                }
                if (BladeTime == 2 && ProjectileTimer > 10)
                {
                    if (!PlaySound)
                    {
                        Hit(player);
                        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                        PlaySound = true;
                    }
                    Projectile.rotation -= 0.30f * player.direction;
                    MaxRotation++;
                    if (MaxRotation > 7)
                    {
                        PlaySound = false;
                        BladeTime++;
                        ProjectileTimer = 0;
                        MaxRotation = 0;
                        PForce(player);
                    }
                }
                if (BladeTime == 3 && ProjectileTimer > 10)
                {
                    Projectile.Kill();
                }
            }
            player.heldProj = Projectile.whoAmI;//使弹幕的贴图画出来后 夹 在角色的身体和手之间
            if (player.controlUseItem)
                Projectile.timeLeft = 2;//让弹幕一直转圈圈的方法之一
            else
            {
                Projectile.Kill();
            }
            base.AI();
        }
        public override bool ShouldUpdatePosition()
        {
            return false;//让弹幕位置不受速度影响
        }
        private void Hit(Player player)
        {
            int Offset = 0;
            if (player.direction > 0)
            {
                Offset = 70;
            }
            else
            {
                Offset = -70;
            }
            Vector2 HitBoxPos = new Vector2(player.Center.X + Offset, player.Center.Y);
                hitbox = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    HitBoxPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<PeachWoodBladeHitbox>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
        }
        private void PForce(Player player)
        {
            player.velocity += new Vector2(player.direction * 3f, 0);
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
            float radius = 81f;
            float width = 12f;
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
                    Color c = Color.Lerp(Color.SandyBrown, Color.Brown, progress) * (0.8f + 0.8f * progress);
                    ve.Add(new Vertex(screenPos + new Vector2(0, -width).RotatedBy(a),new Vector3(progress, 1, 1), c));
                    ve.Add(new Vertex(screenPos + new Vector2(0, width).RotatedBy(a),new Vector3(progress, 0, 1), c));
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
                Projectile.rotation - MathHelper.PiOver4,
                new Vector2(0, 33.2f),
                2f,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
