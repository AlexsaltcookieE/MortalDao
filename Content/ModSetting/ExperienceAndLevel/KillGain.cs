using MortalDao.Content.ModSetting.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.ExperienceAndLevel
{
    public class KillGain : GlobalNPC
    {
        private static LocalizedText GetRealmUIText(string entryName) => MortalDaoUtils.GetText($"Level.UI.Name.Level.{entryName}");
        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            if (npc.friendly == true) return;
            if (npc.damage < 1) return;
            var mainLevelSys = Main.LocalPlayer.GetModPlayer<MainLevelSys>();
            //
            if (mainLevelSys.Level >= 0)
            {
                long exp = 10;
                foreach (var player in Main.player)
                {
                    if (player.active)
                    {
                        var expPlayer = player.GetModPlayer<MainLevelSys>();
                        if(expPlayer.EXP <= expPlayer.EXPNeed)
                        {
                            expPlayer.AddExperience(exp);
                        }
                        if (Main.myPlayer == player.whoAmI)
                        { // 只在本机飘字
                            CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Gold,GetRealmUIText("OnUpdateEXP").Value + exp);
                        }
                    }
                }
            }
        }
    }
}
