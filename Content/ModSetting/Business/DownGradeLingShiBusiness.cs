using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Placeables.Ores;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.Business
{
    public class DownGradeLingShiBusiness : CustomCurrencySingleCoin
    {
        public static int CurrencyID;

        public DownGradeLingShiBusiness()
            : base(
                ModContent.ItemType<DownGrade_LingShi>(),
                long.MaxValue
            )
        {
            CurrencyTextColor = Color.LimeGreen;
        }
        public override void GetPriceText(string[] lines, ref int currentLine, long price)
        {
            Color color = CurrencyTextColor * ((float)Main.mouseTextColor / 255f);
            lines[currentLine++] = string.Format(
                "[c/{0:X2}{1:X2}{2:X2}:{3} {4}]", // 冒号后用 {3}（价格）和 {4}（灵石）
                color.R, color.G, color.B,
                price,
                "下品灵石"
            );
        }
    }
}