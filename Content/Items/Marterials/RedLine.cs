using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Marterials
{
    public class RedLine : ModItem
    {
        public override void SetDefaults()
        {
            Item.material = true;
            Item.width = 17;
            Item.height = 15;
            Item.scale = 1.5f;
            Item.maxStack = 9999;
            Item.value = 3;
            Item.rare = ItemRarityID.Red;
            Item.material = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(5);
            recipe.AddIngredient(ItemID.RedHusk);
            recipe.AddIngredient(ItemID.Cobweb,20);
            recipe.Register();
        }
    }
}