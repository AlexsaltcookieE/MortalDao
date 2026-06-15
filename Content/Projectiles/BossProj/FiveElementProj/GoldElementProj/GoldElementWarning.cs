using Microsoft.Xna.Framework;
using MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{
    public class GoldElementWarning : ModProjectile
    {
        public override string Texture => "BoulderBoss/Content/ExtraTextures/InVisibleProj";
        private const int WarningTime = 600;
        //
        private const float LockDistance = 1000f;
        //
        private int Timer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 4f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = WarningTime + 30;
            Projectile.alpha = 255; // 完全透明（不画本体）
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Timer < WarningTime)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                Projectile.Center + Main.rand.NextVector2Circular(70f, 70f),
                                0, 0,
                                DustID.Stone,
                                Scale: 2.5f
                            );
                            dust.noGravity = true;
                            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
                        }
                        if (Timer % 60 == 0)
                        {

                            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                            for (int i = 0; i < 5; i++)
                            {
                                Dust ExposeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, Scale: 2, SpeedX: Main.rand.Next(-5, 5), SpeedY: Main.rand.Next(-5, 5));
                            }
                        }
                        Player localPlayer = Main.LocalPlayer;
                        float distSq = Vector2.DistanceSquared(localPlayer.Center, Projectile.Center);
                        bool playerInRange = localPlayer.active && !localPlayer.dead && distSq <= LockDistance * LockDistance;
                        Vector2 Dir = Projectile.Center - localPlayer.Center;
                        if (playerInRange)
                        {
                            CameraLockSystem.LockedCenter = Projectile.Center;
                        }
                        else
                        {
                            CameraLockSystem.LockedCenter = null;
                        }
                    }
                    Timer++;
                }
                else
                {
                    SpawnBoss();
                    Projectile.Kill();
                }
            }
        }
        private void SpawnBoss()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            CameraLockSystem.LockedCenter = null;
            int bossType = ModContent.NPCType<GoldBody>();
            NPC.NewNPC(
                NPC.GetSource_NaturalSpawn(),
                (int)Projectile.Center.X,
                (int)Projectile.Center.Y,
                bossType
            );
            for (int i = 0; i < 20; i++)
            {
                Dust ExposeDust = Dust.NewDustDirect(Projectile.position, Projectile.width * 4, Projectile.height * 4, DustID.Stone, Scale: 3, SpeedX: Main.rand.Next(-5, 5), SpeedY: Main.rand.Next(-5, 5));
            }
            SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);
        }
    }
}
