using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Marterials
{
    public class Blank_Talisman_Paper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = 3;
            Item.rare = ItemRarityID.White;
            Item.material = true;
        }
        
    }
}