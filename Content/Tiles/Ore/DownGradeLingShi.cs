using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Placeables.Ores;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace MortalDao.Content.Tiles.Ore
{
    public class DownGradeLingShi : ModTile
    {
        // 显式指定贴图路径，避免因命名/目录差异导致加载到空贴图（显示透明）
        public override string Texture => "MortalDao/Content/Tiles/Ore/DownGradeLingShi";
        private const int CELL = 16;
        private const int STEP = 18;
        private static bool IsMe(int i, int j)
        {
            var t = Main.tile[i, j];
            return t.HasTile && t.TileType == ModContent.TileType<DownGradeLingShi>();
        }
        private static int NeighborMask(int i, int j)
        {
            int m = 0;
            if (IsMe(i, j - 1)) m |= 1; // U
            if (IsMe(i, j + 1)) m |= 2; // D
            if (IsMe(i - 1, j)) m |= 4; // L
            if (IsMe(i + 1, j)) m |= 8; // R
            return m;
        }
        private static (int col, int row) MaskToCell(int mask)
        {
            bool u = (mask & 1) != 0; // Up
            bool d = (mask & 2) != 0; // Down
            bool l = (mask & 4) != 0; // Left
            bool r = (mask & 8) != 0; // Right

            // 孤立方块（没有任何邻居）
            if (!u && !d && !l && !r)
                return (1, 3);

            // 四向全连
            if (u && d && l && r)
                return (1, 1);

            // 十字型（缺一边）
            if (u && d && l && !r) return (1, 2);
            if (u && d && !l && r) return (1, 0);
            if (u && !d && l && r) return (2, 1);
            if (!u && d && l && r) return (0, 1);

            // 直线型
            if (u && d && !l && !r) return (0, 3); // 竖线
            if (!u && !d && l && r) return (2, 3); // 横线

            // L 型
            if (u && !d && l && !r) return (2, 2);
            if (u && !d && !l && r) return (2, 0);
            if (!u && d && l && !r) return (0, 2);
            if (!u && d && !l && r) return (0, 0);

            // 端点
            if (u && !d && !l && !r) return (3, 0);
            if (!u && d && !l && !r) return (3, 3);
            if (!u && !d && l && !r) return (3, 2);
            if (!u && !d && !l && r) return (3, 1);

            // 兜底（理论上不会走到这里）
            return (1, 1);
        }
        public static void ApplyConnectFrame(int i, int j)
        {
            var t = Main.tile[i, j];
            if (!t.HasTile || t.TileType != ModContent.TileType<DownGradeLingShi>())
                return;

            int mask = NeighborMask(i, j);
            var (col, row) = MaskToCell(mask);

            t.TileFrameX = (short)(col * STEP);
            t.TileFrameY = (short)(row * STEP);
        }
        public override void SetStaticDefaults()
        {
            // 基本物理属性
            Main.tileSolid[Type] = true;          // 实心
            Main.tileBlockLight[Type] = true;     // 挡光（矿石通常挡光）
            Main.tileSpelunker[Type] = true;      // 洞穴探险药水高亮（矿石常用）
            Main.tileOreFinderPriority[Type] = 500; // 金属探测器优先级（数值越大越“高级”）
            Main.tileShine2[Type] = true;         // 发亮效果（可选）
            Main.tileShine[Type] = 900;           // 闪烁频率（可选）   
            AddMapEntry(new Color(60, 220, 220), CreateMapEntryName());
            DustType = DustID.Stone; // 没有专用尘埃就先用现成的
            HitSound = SoundID.Tink;
            MinPick = 0;
            MineResist = 0.8f;
            Main.tileFrameImportant[Type] = true;
            //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = CELL;
            TileObjectData.newTile.CoordinateHeights = new[] { CELL };
            TileObjectData.newTile.CoordinatePadding = 0; // 你这张如果是严丝合缝 3x3，填0或2都行，建议先0
            TileObjectData.newTile.Origin = Point16.Zero;
            TileObjectData.newTile.DrawYOffset = 0;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);
            RegisterItemDrop(ModContent.ItemType<DownGrade_LingShi>());
        }
        public override bool KillSound(int i, int j, bool fail)
            => true; // 用默认音效即可
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail) return;
            // 挖掉后：四邻少了一个邻格，要重算
            RefreshNeighborFrames(i, j);
        }
        private static void RefreshNeighborFrames(int i, int j)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    ApplyConnectFrame(i + dx, j + dy);
                }
        }
    }
}