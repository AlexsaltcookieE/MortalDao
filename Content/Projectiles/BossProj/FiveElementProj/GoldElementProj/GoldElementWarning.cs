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
        public override string Texture => "MortalDao/Content/ExtraTextures/InVisibleProj";
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
            Projectile.netImportant = true;
        }
        public override void AI()
        {
                if (Timer < WarningTime)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Dust dust = Dust.NewDustDirect(
                                Projectile.Center + Main.rand.NextVector2Circular(90f, 90f),
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
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnBoss();
                    }
                CameraLockSystem.LockedCenter = null;

                // ✅ 所有客户端都播放结束特效
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDustDirect(
                            Projectile.position,
                            Projectile.width * 4,
                            Projectile.height * 4,
                            DustID.Stone,
                            Scale: 3,
                            SpeedX: Main.rand.Next(-5, 5),
                            SpeedY: Main.rand.Next(-5, 5)
                        );
                    }
                    SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);
                }

                Projectile.Kill();
            }    
        }
        private void SpawnBoss()
        {
            // ✅ 确保只在服务端生成Boss
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
                
            int bossType = ModContent.NPCType<GoldBody>();
            int npcIndex = NPC.NewNPC(
                NPC.GetSource_NaturalSpawn(),
                (int)Projectile.Center.X,
                (int)Projectile.Center.Y,
                bossType
            );
            
            // ✅ 通知所有客户端Boss已生成
            if (Main.netMode == NetmodeID.Server && npcIndex < Main.maxNPCs)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
            }
        }
    }
}
