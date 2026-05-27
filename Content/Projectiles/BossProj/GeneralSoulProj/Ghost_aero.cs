using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Ghost_aero : ModProjectile
    {
        private int ProjectileTimer;
        public override void SetStaticDefaults()
        {
            // һ֡��Ļ
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 3000;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
            }
            if (Projectile.ai[1] == 1f)
            {
                if (Projectile.ai[2] == 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    return;
                }
                int npcIndex = (int)Projectile.ai[0];
                ProjectileTimer++;
                if (ProjectileTimer < 70)
                {
                    Projectile.velocity = -Vector2.UnitY * 6f;
                }
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                float ProjDistance = Vector2.Distance(Projectile.Center, target.Center);
                if (ProjDistance < 100 && Projectile.ai[2] == 0f)
                {
                    Projectile.ai[2] = 1f;

                    // 锁定当前方向（防止 0 速度）
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = Vector2.UnitX;

                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;
                }
                if (ProjDistance > 100)
                {
                    Vector2 dir = target.Center - Projectile.Center;
                    if (dir != Vector2.Zero)
                        dir.Normalize();

                    float speed = 10f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.1f);
                }
            }
            if (Projectile.ai[1] == 2f)
            {
                if (Projectile.ai[2] == 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    return;
                }
                int npcIndex = (int)Projectile.ai[0];
                ProjectileTimer++;
                if (ProjectileTimer < 70)
                {
                    Projectile.velocity = Vector2.UnitY * 6f;
                }
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                float ProjDistance = Vector2.Distance(Projectile.Center, target.Center);
                if (ProjDistance < 100 && Projectile.ai[2] == 0f)
                {
                    Projectile.ai[2] = 1f;

                    // 锁定当前方向（防止 0 速度）
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = Vector2.UnitX;

                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;
                }
                if (ProjDistance > 100)
                {
                    Vector2 dir = target.Center - Projectile.Center;
                    if (dir != Vector2.Zero)
                        dir.Normalize();

                    float speed = 10f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.1f);
                }
            }
            if (Projectile.ai[1] == 3f)
            {
                if (Projectile.ai[2] == 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    return;
                }
                int npcIndex = (int)Projectile.ai[0];
                ProjectileTimer++;
                if (ProjectileTimer < 70)
                {
                    Projectile.velocity = Vector2.UnitX * 6f;
                }
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                float ProjDistance = Vector2.Distance(Projectile.Center, target.Center);
                if (ProjDistance < 100 && Projectile.ai[2] == 0f)
                {
                    Projectile.ai[2] = 1f;

                    // 锁定当前方向（防止 0 速度）
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = Vector2.UnitX;

                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;
                }
                if (ProjDistance > 100)
                {
                    Vector2 dir = target.Center - Projectile.Center;
                    if (dir != Vector2.Zero)
                        dir.Normalize();

                    float speed = 10f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.1f);
                }
            }
            if (Projectile.ai[1] == 4f)
            {
                if (Projectile.ai[2] == 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    return;
                }
                int npcIndex = (int)Projectile.ai[0];
                ProjectileTimer++;
                if (ProjectileTimer < 70)
                {
                    Projectile.velocity = -Vector2.UnitX * 6f;
                }
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                float ProjDistance = Vector2.Distance(Projectile.Center, target.Center);
                if (ProjDistance < 100 && Projectile.ai[2] == 0f)
                {
                    Projectile.ai[2] = 1f;

                    // 锁定当前方向（防止 0 速度）
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = Vector2.UnitX;

                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;
                }
                if (ProjDistance > 100)
                {
                    Vector2 dir = target.Center - Projectile.Center;
                    if (dir != Vector2.Zero)
                        dir.Normalize();

                    float speed = 10f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill();
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            // 拖尾颜色（你可以随便改）
            Color trailColor = new Color(100, 200, 255, 80);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float rotation = Projectile.velocity.ToRotation() + MathHelper.Pi;

                Color color = trailColor * progress * 0.7f;

                Main.spriteBatch.Draw(
                    texture,
                    drawPos,
                    null,
                    color,
                    rotation,
                    origin,
                    Projectile.scale * (0.8f + progress * 0.4f),
                    SpriteEffects.None,
                    0f
                );
                
            }

            // 绘制本体
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
