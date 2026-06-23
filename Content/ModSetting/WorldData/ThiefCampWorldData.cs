using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MortalDao.Content.ModSetting.WorldData
{
    public class ThiefCampWorldData : ModSystem
    {
        public static int CampCenterX { get; set; } = -1;
        public static int CampCenterY { get; set; } = -1;
        public static bool ThiefInvasionTriggered { get; set; } = false;
        public override void ClearWorld()
        {
            CampCenterX = -1;
            CampCenterY = -1;
            ThiefInvasionTriggered   = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["CampCenterX"] = CampCenterX;
            tag["CampCenterY"] = CampCenterY;
            tag["GoblinTriggered"] = ThiefInvasionTriggered;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            CampCenterX = tag.GetInt("CampCenterX");
            CampCenterY = tag.GetInt("CampCenterY");
            ThiefInvasionTriggered = tag.GetBool("GoblinTriggered");
        }
    }
}
