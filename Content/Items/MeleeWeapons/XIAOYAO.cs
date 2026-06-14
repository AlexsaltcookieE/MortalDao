using Microsoft.Xna.Framework;
using MortalDao.Content.Projectiles.MelleeWeaponsProj;
using MortalDao.Content.Items.MeleeWeapons;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MortalDao.Content.Items.Placeables.Ores;

namespace MortalDao.Content.Items.MeleeWeapons
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class XIAOYAO : ModItem
    {
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.YINGYANG.hjson' file.
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.crit = 20;
            Item.useTime = 20;
            Item.useAnimation = 1;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = true;//自动挥舞
            Item.noUseGraphic = true;
            Item.shoot = Main.rand.Next(ModContent.ProjectileType<LXYIron>(), ModContent.ProjectileType<LXYPeachWood>());
            Item.shootSpeed = 20;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<FreezeIronSword>(),1);
            recipe.AddIngredient(ModContent.ItemType<RoughPeachWoodSword>(), 1);
            recipe.AddIngredient(ModContent.ItemType<DownGrade_LingShi>(), 20);

            recipe.Register();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 让弹幕从玩家上方生成，再朝鼠标位置飞行，效果类似“从天而降”的剑气
            position = player.Center + new Vector2(Main.rand.NextFloat(-80f, 80f), -480f);
            Vector2 target = Main.MouseWorld;
            velocity = target - position;

            if (velocity.LengthSquared() < 0.001f)
            {
                velocity = Vector2.UnitY;
            }

            velocity.Normalize();
            velocity *= Item.shootSpeed;
        }
    }
}
