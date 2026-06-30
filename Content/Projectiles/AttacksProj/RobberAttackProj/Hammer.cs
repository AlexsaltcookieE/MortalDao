using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj
{
    public class Hammer : ModProjectile
    {
        public override void SetStaticDefaults()
        { 
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = true; // 不撞墙，避免回收前意外消失
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1; // 无限穿透，减速/悬停阶段可以多次命中玩家
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 4f;
            Projectile.timeLeft = 300; // 延长到4秒，足够覆盖整个流程（原200可能临界）
            Projectile.scale = 2.5f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D Glowtexture = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/AttacksProj/RobberAttackProj/HammerGlow").Value;
            Vector2 GlowOrigin = Glowtexture.Size() / 2f;
            Vector2 origin = texture.Size() / 2f;
            Color trailColor = Color.Cyan;
            Main.spriteBatch.Draw(
                Glowtexture,
                Projectile.Center - Main.screenPosition,
                null,
                trailColor,
                Projectile.rotation,
                GlowOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );
            return false;
        }
    }
}