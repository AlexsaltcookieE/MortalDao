using MortalDao.Content.Items.Placeables.Ores;
using MortalDao.Content.Tiles.CraftStation;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace MortalDao.Content.Tiles.Ore
{
    public class Cyan_Fe_Ingot_item : ModItem
    {
        public override string Texture => "MortalDao/Content/Items/Placeables/Ingots/Cyan_Fe_Ingot_item";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
            Item.material = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Cyan_Fe_Item>(), 5)
                .AddTile(ModContent.TileType<AlchemyFurnance>())
                .Register();
                
        }
    }
}
