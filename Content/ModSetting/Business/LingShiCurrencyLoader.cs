using MortalDao.Content.ModSetting.UI;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.Business
{
    public class LingShiCurrencyLoader : ModSystem
    {

        public override void PostSetupContent()
        {
            var currency = new DownGradeLingShiBusiness();DownGradeLingShiBusiness.CurrencyID = CustomCurrencyManager.RegisterCurrency(currency) ;
        }
    }
}