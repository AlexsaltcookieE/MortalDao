using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement
{
    public class GoldArm : ModNPC
    {
        public override string Texture =>
            "MortalDao/Content/NPCs/BOSS/FiveElement/GoldElement/GoldBody";
        public override void SetDefaults()
        {
            NPC.noTileCollide = true;
            NPC.damage = 25;
            NPC.friendly = false;
            NPC.width = 32;
            NPC.height = 32;
            NPC.scale = 2f;
            NPC.defense = 10;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath1;
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
    }
}