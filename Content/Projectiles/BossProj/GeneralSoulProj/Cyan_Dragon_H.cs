using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Cyan_Dragon_H : ModProjectile
    {
        private const int thornDistance = 16;
        private int ThornSpeed;
        private bool Cal = false;
        private int CalNumb;
        private bool Lock = false;
        private bool PlaySound = false;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 450;
            Projectile.height = 85;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1500;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {

            if (Projectile.ai[1] == 2f)
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                NPC OwnerNpc = Main.npc[npcIndex];
                if (OwnerNpc.active && OwnerNpc.target >= 0 && OwnerNpc.target < Main.maxPlayers)
                {
                    Player targetPlayer = Main.player[OwnerNpc.target];
                    // 现在 targetPlayer 就是 NPC 的目标玩家
                    if (targetPlayer.active && !targetPlayer.dead)
                    {
                        if (!Cal)
                        {
                            ThornSpeed = 15;
                            CalNumb = targetPlayer.Center.X > OwnerNpc.Center.X ? 0 : 1;
                            Cal = true;
                            Lock = false;
                            Projectile.rotation = CalNumb == 0 ? -MathHelper.Pi*2 : MathHelper.Pi;
                        }

                        int direction = CalNumb == 0 ? 1 : -1;
                        if (!Lock)
                        {
                            if (!PlaySound)
                            {
                                SoundEngine.PlaySound(SoundID.Item1, targetPlayer.Center);
                                PlaySound = true;
                            }
                            Projectile.velocity = new Vector2(direction * ThornSpeed, 0);
                            if (Math.Abs(Projectile.Center.X - OwnerNpc.Center.X) >= thornDistance * 20)
                            {
                                Lock = true;
                            }
                        }
                        else
                        {
                            Projectile.velocity = new Vector2(-direction * ThornSpeed, 0);
                            if (Math.Abs(Projectile.Center.X - OwnerNpc.Center.X) <= 16f)
                            {
                                Projectile.Kill();
                            }
                        }
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

            target.AddBuff(ModContent.BuffType<BreakLeg>(),60 * 2);
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            // Make sprite size match hitbox size (width/height) exactly.
            Vector2 drawScale = new Vector2
            (
                Projectile.width / (float)frame.Width,
                Projectile.height / (float)frame.Height
            );
            
            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, drawScale, effects);
            return false;
        }
    }
}
