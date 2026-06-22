using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Marterials
{
    public class PeachWood : ModItem
    {
        public override void SetDefaults()
        {
            Item.material = true;
            Item.width = 24;
            Item.height = 22;
            Item.maxStack = 9999;
            Item.value = 3;
            Item.rare = ItemRarityID.Pink;
            Item.material = true;
        }

    }
}