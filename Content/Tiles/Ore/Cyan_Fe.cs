using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Placeables.Ores;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace MortalDao.Content.Tiles.Ore
{
    public class Cyan_Fe : ModTile
    {
        // 显式指定贴图路径，避免因命名/目录差异导致加载到空贴图（显示透明）
        public override string Texture => "MortalDao/Content/Tiles/Ore/Cyan_Fe";

        public override void SetStaticDefaults()
        {
            // 基本物理属性
            Main.tileSolid[Type] = true;          // 实心
            Main.tileBlockLight[Type] = true;     // 挡光（矿石通常挡光）
            Main.tileSpelunker[Type] = true;      // 洞穴探险药水高亮（矿石常用）
            Main.tileOreFinderPriority[Type] = 450; // 金属探测器优先级（数值越大越“高级”）
            Main.tileShine2[Type] = true;         // 发亮效果（可选）
            Main.tileShine[Type] = 900;           // 闪烁频率（可选）
            // 地图显示
            AddMapEntry(new Color(60, 220, 220),CreateMapEntryName());
            // 挖掘相关
            DustType = DustID.BlueMoss; // 没有专用尘埃就先用现成的
            HitSound = SoundID.Tink;
            MinPick = 0;       // 需要的镐力（0=任意镐）
            MineResist = 1.2f; // 挖掘抗性（越大越难挖）
            Main.tileFrameImportant[Type] = true;
            // 物块掉落（1.4 新写法常用）
            // 如果你这版 tML 报错，我再按旧版 drop 写法给你改。
            // RegisterItemDrop(ModContent.ItemType<你的物品>());
            RegisterItemDrop(ModContent.ItemType<Cyan_Fe_Item>());
        }
    }
}