using Microsoft.Xna.Framework;
using MortalDao.Content.Buffs.MeleeWeapons;
using MortalDao.Content.Items.Marterials;
using MortalDao.Content.Items.MeleeWeapons;
using MortalDao.Content.Items.Placeables.Ores;
using MortalDao.Content.Projectiles.MelleeWeaponsProj;
using MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand;
using MortalDao.Content.Tiles.CraftStation;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.MeleeWeapons
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class YueMaidenSword : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.YINGYANG.hjson' file.
        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.crit = 20;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = false;
            Item.noUseGraphic = true;
            Item.shootSpeed = 25;
            Item.shoot = ModContent.ProjectileType<YueMaidenSwordStab>();
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<RedLine>(), 10);
            recipe.AddIngredient(ModContent.ItemType<PeachWood>(), 10);
            recipe.AddRecipeGroup("MortalDao:SilverOrTungsten", 15);
            recipe.AddTile(ModContent.TileType<AlchemyFurnance>());
            recipe.Register();
        }
        public override bool AltFunctionUse(Player player)
        {
            return true; // 允许右键
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2 && !player.HasBuff(ModContent.BuffType<LightSpeedCoolDown>()) && !player.HasBuff(ModContent.BuffType<LightSpeed>()))
            {
                float baseRotation = velocity.ToRotation();
                float[] angleOffsets = new float[]
                                    {
                                    -((MathHelper.PiOver4)/2)/2,
                                    0f,
                                    (MathHelper.PiOver4/2)/2
                                    };
                foreach (float offset in angleOffsets)
                {
                    velocity = Vector2.UnitX.RotatedBy(baseRotation + offset) * 12;
                    Projectile.NewProjectileDirect(source,Main.MouseWorld,velocity, Item.shoot, damage, knockback, player.whoAmI);
                    player.AddBuff(ModContent.BuffType<LightSpeed>(), 60 * 30);
                }
                return false;
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
}