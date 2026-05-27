using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Placeables.CraftStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace MortalDao.Content.Tiles.CraftStation 
{ 
    public class AlchemyFurnance : ModTile
    {
        public override string Texture => "MortalDao/Content/Tiles/CraftStation/AlchemyFurnace";

        public override void SetStaticDefaults()
        {
            //特性
            Main.tileSolid[Type] = false;//物块整体是否实心
            Main.tileNoAttach[Type] = true;//物块是否可以与其他物块连接
            Main.tileLighted[Type] = true;//物块是否可以发光               //
            Main.tileLavaDeath[Type] = false;//物块是否可以被熔岩烧毁
            Main.tileWaterDeath[Type] = false;//物块是否可以被水淹没
            Main.tileBlockLight[Type] = false;//物块会阻挡光源
            Main.tileNoFail[Type] = true;//物块是否可以被其他物块覆盖
            Main.tileFrameImportant[Type] = true; // 多格家具必须开启，否则会显示成一格一格的
            DustType = DustID.Copper;//破坏时粒子效果
            HitSound = SoundID.Tink;//破坏时声音

            // 放置数据（3x3 制作站）
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 255, 255), CreateMapEntryName());

            RegisterItemDrop(ModContent.ItemType<AlchemyFurnaceItem>());
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}