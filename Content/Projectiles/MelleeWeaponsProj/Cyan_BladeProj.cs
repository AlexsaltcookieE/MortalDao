using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.RendererSystem;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.MelleeWeaponsProj
{
    public class PeachWoodBladeProj : ModProjectile
    {
        public override string Texture => "MortalDao/Content/Items/MeleeWeapons/RoughPeachWoodSword";
        Player player => Main.player[Projectile.owner];//获取玩家
        private int MaxRotation;
        private bool CanForce = false;
        private int ProjectileTimer = 0;
        private int BladeTime = 0;
        private bool PlaySound;
        private int hitbox;
        private float RAmount = 0.32f;

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
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            List<Vertex> ve = new List<Vertex>();
            for (int i = 0; i < 15; i++)
            {
                Color b = (Color.Lerp(Color.SandyBrown, Color.Brown, i / 15f));
                ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -70).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction), new Vector3(i / 15f, 1, 1), b));
                ve.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -20).RotatedBy(Projectile.oldRot[i]) * (1 + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction), new Vector3(i / 15f, 0, 1), b));
                if (ve.Count >= 3)//因为顶点需要围成一个三角形才能画出来 所以需要判顶点数>=3 否则报错
                {
                    gd.Textures[0] = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/MelleeWeaponsProj/Cyan_BladeProj").Value;//设置纹理
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, ve.ToArray(), 0, ve.Count - 2);
                }
            }
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value,
                         Projectile.Center - Main.screenPosition,
                         null,
                         lightColor,
                         Projectile.rotation - MathHelper.PiOver4,
                         new Vector2(0, 30),
                         2f,
                         SpriteEffects.None,
                         0);
            return false;//让弹幕不画原来的样子
        }
    }
}
