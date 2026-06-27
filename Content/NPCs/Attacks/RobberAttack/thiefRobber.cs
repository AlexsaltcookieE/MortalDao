using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class thiefRobber : ModNPC
    {
        public override void OnKill()
        {
            RobberAttackEvent.RemainingRobbers = RobberAttackEvent.RemainingRobbers + 1;
            if (Main.netMode != NetmodeID.Server)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-1f, 1.5f), Main.rand.NextFloat(-4f, -1.0f)), Mod.Find<ModGore>("thiefRobberGore1").Type, NPC.scale * Main.rand.NextFloat(0.8f, 1.0f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-1.25f, 1.75f), Main.rand.NextFloat(-6f, -2f)), Mod.Find<ModGore>("thiefRobberGore2").Type, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            // ✅ 受伤时生成一些金色的尘埃
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
                }
            }
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
        }
        public override void SetDefaults()
        {
            NPC.daybreak = false;
            NPC.width = 40;
            NPC.height = 56;
            NPC.scale = 1f;
            NPC.damage = 15;
            NPC.lifeMax = 100;
            NPC.defense = 5;
            NPC.knockBackResist = 1f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.aiStyle = 3;
            AIType = NPCID.GoblinThief;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
        }
        public override void AI()
        {
            if (hitStunTimer > 0)
            {
                hitStunTimer--;
                // 只让击退自然生效，不做任何主动位移
                NPC.velocity.X *= 0.9f; // 可选：稍微减速，看起来更自然
                return;
            }

            float moveSpeed = 2f;
            NPC.velocity.X = NPC.direction * moveSpeed;
            base.AI();
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6) // 每10帧切换一次
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 3) // 超过第3帧时回到第1帧
                {
                    NPC.frame.Y = 0;
                }
            }
        }
        private int hitStunTimer = 0;

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            hitStunTimer = 8;
            base.OnHitByItem(player, item, hit, damageDone);
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            hitStunTimer = 8;
            base.OnHitByProjectile(projectile, hit, damageDone);
        }
    }
}
