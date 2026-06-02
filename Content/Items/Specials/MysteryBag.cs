using MortalDao.Content.Items.MeleeWeapons;
using MortalDao.Content.Items.Specials.Document;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
namespace MortalDao.Content.Items.Specials
{
    //新手锦囊
    public class MysteryBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;//需要3个解锁复制
            ItemID.Sets.OpenableBag[Type] = true; //可以打开
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.rare = ItemRarityID.Red;
            Item.consumable = true;//消耗品
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.value = 30;
        }
        public override bool CanRightClick()
        {
            return true;
        }
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<RoughPeachWoodSword>()));//战利品
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<OldPlayerDiary>()));
        }

        
        
    }
}

