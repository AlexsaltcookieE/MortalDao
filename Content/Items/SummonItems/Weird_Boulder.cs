using MortalDao.Content.Items.Marterials;
using MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement;
using MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj;
using MortalDao.Content.Tiles.CraftStation;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Items.SummonItems
{
    public class Weird_Boulder : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Boulder;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.DamageType = DamageClass.Throwing; // 投掷伤害
            Item.useTime = 50;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.consumable = true;
            Item.maxStack = 20;
            Item.shoot = ModContent.ProjectileType<WeirdBoulderProj>();
            Item.shootSpeed = 12f;
            Item.rare = ItemRarityID.Purple;
            Item.damage = 100;
            Item.knockBack = 8f;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.position.Y < Main.worldSurface * 16.0)
                return false;
            foreach (NPC npc in Main.npc)
            {
                if(npc.active && (npc.type == ModContent.NPCType<GoldArm>() || npc.type == ModContent.NPCType<GoldBody>()))
                {
                    return false;
                }
            }
            return base.CanUseItem(player);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Boulder, 1);
            recipe.AddIngredient(ModContent.ItemType<resentment>(), 5);
            recipe.AddTile(ModContent.TileType<AlchemyFurnance>());
            recipe.Register();
        }
    }
}
