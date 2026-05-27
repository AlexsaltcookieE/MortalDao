using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics; // 引入绘图命名空间
using MortalDao.Content.Projectiles.RangeWeaponsProj;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.RangeWeapons
{
    public class Overlord_Bow : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 1. 开启挥动动画逻辑（这是必须的）
            // 2. 注册动画：指定动画类型为 Swing（挥动），并设置帧数为 8
            // 注意：这里的 8 代表总帧数，你需要确保你的贴图是竖排的 8 帧动画
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(8, 8));

            // 显示名称 & 提示
        }

        public override void SetDefaults()
        {
            Item.width = 33;
            Item.height = 88; // 注意：这里的 height 应该是单帧的高度，而不是整张图的高度
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.autoReuse = true;
            Item.scale = 2f;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 55;
            Item.knockBack = 3.5f;
            Item.crit = 8;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Arrow;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
            Item.shoot = AmmoID.Arrow;
            Item.noMelee = true;
            Item.channel = false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 忽略原本的 type（箭矢），强制发射日暮弹幕
            int realType = ModContent.ProjectileType<OverLord_Arrow>();
            // 生成弹幕
            Projectile.NewProjectile(
                source,
                position,
                velocity,
                realType,
                damage,
                knockback,
                player.whoAmI
            );
            // false = 阻止原版弹药发射
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-9f, -1); // ← 自己微调
        }
        // 可选：如果你需要更精细地控制每一帧的绘制（通常不需要，除非有特殊偏移）
        // public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor) { return true; }
        // public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { return true; }
    }
}