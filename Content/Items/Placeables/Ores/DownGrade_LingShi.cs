using MortalDao.Content.ModSetting.Business;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Placeables.Ores
{
    public class DownGrade_LingShi : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;       // 最大堆叠
            Item.value = 0;             // 禁用原版金币计价
            Item.rare = ItemRarityID.Green;
            Item.shopSpecialCurrency = DownGradeLingShiBusiness.CurrencyID;  // 使用注册的货币ID
        }
    }
}