using Microsoft.Xna.Framework;
using MortalDao.Content.Buffs.SummonWeaponsBuffs;
using MortalDao.Content.Projectiles.SummonWeaponsProj.PuresWand;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.SummonWeapons
{ 
    public class Puers_Slime_Wand : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Expert;
            //面板
            Item.damage = 7;//伤害
            Item.DamageType = DamageClass.Summon;//召唤类型
            Item.mana = 10;//耗蓝
            Item.useTime = 15;//攻速
            Item.useAnimation = 15;
            //绘制
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Swing;//挥舞
            //特性
            Item.noMelee = true;//无近战
            Item.value = Item.sellPrice(gold: 99);//售价
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Baby_Slime_King>();//发射宝宝史莱姆
            Item.buffType = ModContent.BuffType<RainBowSlime>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = Main.MouseWorld;//鼠标位置
            player.AddBuff(Item.buffType, 300);//史莱姆宝宝
            var projectile = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Gel, 20);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (var line in tooltips)
            {
                    float t = (float)(Main.GlobalTimeWrappedHourly * 0.8 % 1.0);
                    line.OverrideColor = Main.hslToRgb(t, 1f, 0.75f, 0);
            }
        }
    }
}