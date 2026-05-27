using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.NPCs.BOSS.GeneralSoul;
using Terraria;
using Terraria.ModLoader;
namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Bow_of_OverLord : ModProjectile
    {
        private const int TotalFrames = 8;   // 总帧数
        private const int FrameSpeed = 5;    // 每几帧切一次

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;          // 对玩家友好
            Projectile.hostile = false;          // 不对玩家造成伤害
            Projectile.penetrate = 1;
            Projectile.timeLeft = 700;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.scale = 3f;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 1f)
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                NPC npc = Main.npc[npcIndex];
                if(npc.type == ModContent.NPCType<GeneralSoul>() && npc.active)
                {
                    Vector2 targetPos = new Vector2(npc.Center.X,npc.Center.Y);
                    Projectile.Center = targetPos;
                    foreach(Projectile proj in Main.projectile)
                    {
                        if(proj.active && proj.type == ModContent.ProjectileType<Black_Hole>())
                        {
                            Vector2 Dir = proj.Center - npc.Center;
                            Projectile.rotation = Dir.ToRotation();
                        }
                    }
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }
            // 帧动画逻辑
            if (++Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= TotalFrames)
                    Projectile.frame = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / TotalFrames;
            Rectangle sourceRect = new Rectangle(
                0,
                Projectile.frame * frameHeight,
                texture.Width,
                frameHeight
            );

            Vector2 origin = sourceRect.Size() / 2f;

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRect,
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