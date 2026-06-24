using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.NPCs.Attacks.RobberAttack;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.AttacksProj.RobberAttackProj
{
    public class PlumBlossomDart : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.tileCollide = true; // 不撞墙，避免回收前意外消失
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1; // 无限穿透，减速/悬停阶段可以多次命中玩家
            Projectile.width = 13;
            Projectile.height = 13;
            Projectile.scale = 1.5f;
            Projectile.timeLeft = 300; // 延长到4秒，足够覆盖整个流程（原200可能临界）
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
        }
        public override void OnKill(int timeLeft)
        {
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
            Color trailColor = Color.White;
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