using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.MeleeWeapons
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class FreezeIronSword : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.YINGYANG.hjson' file.
        public override void SetDefaults()
        {
            Item.damage = 27;//伤害
            Item.DamageType = DamageClass.Melee;//近战
            Item.width = 70;//掉落在地上的大小
            Item.height = 70;//掉落在地上的大小
            Item.useTime = 20;//攻速
            Item.useAnimation = 20;//动画速度
            Item.crit = 10;
            Item.useStyle = ItemUseStyleID.Swing;//挥舞
            Item.knockBack = 10;//击退
            Item.value = Item.buyPrice(silver: 15);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;//自动挥舞
            Item.scale = 2f;
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Crit)
            {
                target.AddBuff(BuffID.Poisoned,300);
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 20);
            recipe.AddIngredient(ItemID.IceBlock, 20);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
}
