using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting
{
    public class RecipeSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            RecipeGroup CopperGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.CopperBar), ItemID.CopperBar, ItemID.TinBar);
            RecipeGroup.RegisterGroup("MortalDao:CopperOrTin", CopperGroup);
            RecipeGroup SliverGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.SilverBar), ItemID.SilverBar, ItemID.TungstenBar);
            RecipeGroup.RegisterGroup("MortalDao:SilverOrTungsten", SliverGroup);
            RecipeGroup GoldGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.GoldBar), ItemID.GoldBar, ItemID.PlatinumBar);
            RecipeGroup.RegisterGroup("MortalDao:GoldOrPlatinum", GoldGroup);
            RecipeGroup DemoniteGroup = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.DemoniteBar), ItemID.DemoniteBar, ItemID.CrimtaneBar);
            RecipeGroup.RegisterGroup("MortalDao:DemoniteOrCrimtane", DemoniteGroup);
        }
    }
}
