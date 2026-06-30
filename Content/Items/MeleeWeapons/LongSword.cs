using MortalDao.Content.Projectiles.MelleeWeaponsProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.MeleeWeapons
{
    public class LongSword : ModItem
    {
        public override void SetDefaults()
        {
            //尺寸
            Item.scale = 1.5f;
            //伤害
            Item.knockBack = 1.3f;
            Item.crit = 8;
            Item.damage = 32;
            Item.DamageType = DamageClass.Melee;
            //速度
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            //动画
            Item.noMelee = true;
            Item.noUseGraphic = true;
            //稀有度
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.LightPurple;
            //其他
            Item.autoReuse = false;
            //弹幕
            Item.shoot = ModContent.ProjectileType<LongSwordProj>();
            Item.shootSpeed = 45;
        }
    }
}