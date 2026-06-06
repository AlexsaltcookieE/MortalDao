using MortalDao.Content.Tiles.Ore;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace MortalDao.Content.Items.Placeables.Ores
{
    public class Cyan_Fe_Item : ModItem
    {
        public override string Texture => "MortalDao/Content/Items/Placeables/Ores/Cyan_Fe_Item";
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.Cyan;
            Item.createTile = ModContent.TileType<Cyan_Fe>();
            Item.material = true;
        }
    }
}