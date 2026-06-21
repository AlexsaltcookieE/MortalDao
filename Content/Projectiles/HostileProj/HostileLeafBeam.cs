
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.RendererSystem;
using System.Collections.Generic;
using System.Configuration;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.HostileProj
{
    public class HostileLeafBeam : ModProjectile
    {
        public override string Texture => "MortalDao/Content/ExtraTextures/InVisibleProj";
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1; // Infinite penetration
            Projectile.timeLeft = 300; // Lasts for 5 seconds
            Projectile.tileCollide = false; // Doesn't collide with tiles
            Projectile.ignoreWater = true; // Ignores water
        }
        public override void AI()
        {
            if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = Vector2.UnitX * 8f;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.AnisotropicClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
            Texture2D tex = ModContent.Request<Texture2D>("MortalDao/Content/ExtraTextures/ScarletDevilStreak").Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float rotation = dir.ToRotation();
            float length = 800f;
            float width = 200f;
            sb.Draw(
                tex,
                center,
                null,
                Color.White,
                rotation,
                new Vector2(0.5f, 0.5f),
                new Vector2(length / tex.Width, width / tex.Height),
                SpriteEffects.None,
                0
            );
            sb.End();
            sb.Begin(); // 恢复默认
            return false;
        }
    }
}
