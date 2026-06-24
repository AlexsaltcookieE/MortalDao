using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class RobberAttackEvent : ModSystem
    {
        public static bool EventActive = false;

        public static int RemainingRobbers = 0;
        public override void PostUpdateNPCs()
        {
            if (!EventActive) return;
            if (RemainingRobbers <= 0)
            {
                EventActive = false;
                Main.NewText("盗斧贼团伙已被击退！", 255, 200, 50);
                return;
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["RobberEventActive"] = EventActive;
            tag["RobberEventRemaining"] = RemainingRobbers;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            EventActive = tag.GetBool("RobberEventActive");
            RemainingRobbers = tag.GetInt("RobberEventRemaining");
        }
    }
}
