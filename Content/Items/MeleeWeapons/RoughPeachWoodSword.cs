using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.MeleeWeapons
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class RoughPeachWoodSword : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.YINGYANG.hjson' file.
        public override void SetDefaults()
        {
            Item.damage = 20;//伤害
            Item.DamageType = DamageClass.Melee;//近战
            Item.width = 70;//掉落在地上的大小
            Item.height = 70;//掉落在地上的大小
            Item.useTime = 20;//攻速
            Item.useAnimation = 20;//动画速度
            Item.useStyle = ItemUseStyleID.Swing;//挥舞
            Item.knockBack = 10;//击退
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;//自动挥舞
            Item.scale = 1.5f;
        }
    }
}
