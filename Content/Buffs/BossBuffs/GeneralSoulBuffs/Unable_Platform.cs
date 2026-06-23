using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs
{
    public class Unable_Platform : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<Unable_PlatformPlayer>().HasUnable_Platform = true;
        }
    }
    public class Unable_PlatformPlayer : ModPlayer
    {
        public int ignorePlatformsTimer;
        public bool HasUnable_Platform;
        public override void ResetEffects()
        {
            HasUnable_Platform = false;
        }
        public override void PostUpdate()
        {
            if (HasUnable_Platform)
            {
                if (Player.velocity.Y >= 0) // 玩家正在下落或静止
                {
                    Point tileBelow = Player.Bottom.ToTileCoordinates();
                    Tile tile = Framing.GetTileSafely(tileBelow.X, tileBelow.Y);

                    if (tile.HasTile && Main.tileSolidTop[tile.TileType])
                    {
                        // 如果脚底下是平台，强制位移
                        Player.position.Y += 2f; // 稍微往下拉一点
                    }
                }
            }
        }
    }
}
