using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Specials
{
    public class Bloody_key : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Red;
            Item.value = 10;
        }
    }
}
