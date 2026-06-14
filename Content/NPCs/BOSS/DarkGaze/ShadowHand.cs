using Microsoft.Xna.Framework;
using MortalDao.Content.Projectiles.BossProj.DarkGazeProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.BOSS.DarkGaze
{
    public class ShadowHand : ModNPC
    {
        private const int FrameSpeed = 6;
        private int FrameCounter;


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.width = 46;
            NPC.height = 30;
            NPC.damage = 40;
            NPC.defense = 8;
            NPC.lifeMax = 180;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.25f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            NPC.scale = 2;
        }

        public override void AI()
        {
            // 选择最近的玩家作为目标
            NPC.TargetClosest(faceTarget: true);
            Player player = Main.player[NPC.target];
            NPC.direction = player.Center.X >= NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            if (!player.active || player.dead)
            {
                NPC.velocity *= 0.9f;
                return;
            }
            // 检测 Shadow_hand 和玩家距离（像素）
            float PlayerDistance = Vector2.Distance(NPC.Center, player.Center);
            if (NPC.localAI[0] > 0f)
                NPC.localAI[0]--;

            // 示例：800 像素内视为“接近”
            const float DetectRange = 800f;
            const float SelfExposeRange = 100f;
            const int SelfExposeCooldownTicks = 1000; // 60=1秒，防止每帧都生成
            if (PlayerDistance <= DetectRange)
            {
                // TODO：接近后的行为（追踪/攻击/生成弹幕等）
                Vector2 toPlayer = player.Center - NPC.Center;
                if (toPlayer != Vector2.Zero)
                    toPlayer.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 15f, 0.08f);
                if(PlayerDistance <= SelfExposeRange)
                {
                    if (NPC.localAI[0] <= 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<Shadow_hand_EXPOSE>(),
                            NPC.damage,
                            0f,
                            Main.myPlayer,
                            NPC.whoAmI, // ai[0] = owner npc index
                            1f          // ai[1] = enable follow owner
                        );
                        NPC.localAI[0] = SelfExposeCooldownTicks;
                        NPC.netUpdate = true;
                    }
                }
            }
            else
            {
                // TODO：远离时行为
                NPC.velocity *= 0.95f;
            }
        }

        // 禁用 NPC 的接触伤害（碰到玩家不掉血）
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override void FindFrame(int frameHeight)
        {
            FrameCounter++;
            if (FrameCounter >= FrameSpeed)
            {
                FrameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                {
                    NPC.frame.Y = 0;
                }
            }
        }
    }      
}
