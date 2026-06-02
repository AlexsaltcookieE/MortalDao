using MortalDao.Content.Items.Specials.Document;
using MortalDao.Content.ModSetting.UI.DocumentUI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.Specials.Document
{
    public class OldPlayerDiary : ModItem
    {
        //public virtual string DiaryContent => "*!&@(!@)#*!$*!((#))!&#*(&)(#\n!&#*(!##)(#_(#*(#!#*\n#*!&#))!#(#&!*$$&*!$()$!)!#(\n";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.value = 1000;
            Item.rare = ItemRarityID.Quest;
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var ui = ModContent.GetInstance<OldPlayerDiaryUISystem>();
                if (ui != null)
                {
                    ui.ToggleUI();
                }
            }
            return true;
        }
    }
}
//public class OldPlayerDiary_FirstEntry : OldPlayerDiary
//{
//    public override string DiaryContent => "第一天：终于踏上了这片大陆。\n\n" +
//                                         "空气中弥漫着未知的气息，\n" +
//                                         "我的冒险才刚刚开始...";
//    public override void SetDefaults()
//    {
//        base.SetDefaults();
//        Item.rare = ItemRarityID.Blue;
//    }
//}