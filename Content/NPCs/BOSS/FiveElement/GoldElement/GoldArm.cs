using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement
{
    public class GoldArm : ModNPC
    {
        //public override string Texture => "Terraria/Images/Item_" + ItemID.Boulder;
        private int NPC_Damage = 28;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
            NPC.noTileCollide = true;
            NPC.damage = NPC_Damage;
            NPC.friendly = false;
            NPC.width = 40;
            NPC.height = 40;
            NPC.scale = 2.5f;
            NPC.defense = 10;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.Item14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            // ✅ 受伤时生成一些金色的尘埃
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone);
            }
        }
        public override void AI()
        {
            // 新增：检测母体GoldBody是否存活
            if (NPC.ai[0] > 0)
            {
                NPC body = Main.npc[(int)NPC.ai[0]];
                // 如果母体不存在、不活跃、不是GoldBody类型或已死亡，则自身也消失
                if (!body.active || body.type != ModContent.NPCType<GoldBody>() || body.life <= 0)
                {
                    NPC.active = false;  // 直接消失
                    return;
                }
            }
            // 原有的AI逻辑...
        }
        public override void OnKill()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -2f)), Mod.Find<ModGore>("GoldArmGore1").Type, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-5f, -1.5f)), Mod.Find<ModGore>("GoldArmGore2").Type, NPC.scale * Main.rand.NextFloat(0.85f, 1.05f));
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) //难度和玩家数量
        {
            if (Main.expertMode)//专家模式
            {
                NPC.lifeMax = 4000;
            }
            if (Main.masterMode)//大师模式
            {
                NPC.lifeMax = 6200;
            }
        }
    }
}