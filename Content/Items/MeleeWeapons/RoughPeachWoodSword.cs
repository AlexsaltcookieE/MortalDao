using MortalDao.Content.Projectiles.MelleeWeaponsProj;
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
            Item.damage = 40;
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 120;
            Item.useAnimation = 120;
            Item.shootSpeed = 120;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<PeachWoodBladeProj>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }
    }
}