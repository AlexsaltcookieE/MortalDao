using MortalDao.Content.Tiles.Ore;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace MortalDao.Content.Items.Placeables.Ores
{
    public class DownGrade_LingShi : ModItem
    {
        public override void SetDefaults()
        {
            Item.height = 49;
            Item.width = 112;
            Item.scale = 0.25f;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Green;
        }
    }
}