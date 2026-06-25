using Microsoft.Xna.Framework;
using MortalDao.Content.ModSetting.Utilities;
using MortalDao.Content.NPCs.Attacks.RobberAttack;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.WorldData
{
    public class ThiefCampPlayer : ModPlayer
    {
        private static LocalizedText GetSpawnInfo(string entryName) => MortalDaoUtils.GetText($"OnspawnMessage.{entryName}");

        const int DetectRadius = 1000;
        public override void PostUpdate()
        {
            var w = ThiefCampWorldData.CampCenterX;
            var h = ThiefCampWorldData.CampCenterY;
            if (w == -1 || h == -1 || ThiefCampWorldData.ThiefInvasionTriggered)
                return;
            Vector2 CampPos = new Vector2(w * 16 + 8, h * 16 + 8); // +8 居中到格中心
            float distance = Vector2.Distance(CampPos, Player.Center);
            if(distance < DetectRadius)
            {
                TriggerThiefInvasion();
            }
        }
        private void TriggerThiefInvasion()
        {
            ThiefCampWorldData.ThiefInvasionTriggered = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                RobberAttackEvent.EventActive = true;
                RobberAttackEvent.RemainingRobbers = 0;
                Main.NewText(GetSpawnInfo("Robber").Value, 255, 200, 50);
            }
            else
            {
            }
        }
    }
}
