using MortalDao.Content.Items.Placeables.Ores;
using MortalDao.Content.Tiles.CraftStation;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Placeables.CraftStations 
{ 
    public class AlchemyFurnaceItem : ModItem
	{
		public override void SetDefaults() {
			Item.width = 70;
			Item.height = 70;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(gold: 2);
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.consumable = true;
			Item.createTile = ModContent.TileType<AlchemyFurnance>();
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.CopperBar, 30)
                .AddIngredient(ItemID.IronBar, 10)
                .AddIngredient(ItemID.LavaBucket, 3)
				.AddIngredient(ModContent.ItemType<DownGrade_LingShi>(),20)
				.Register();
		}
	}
}
