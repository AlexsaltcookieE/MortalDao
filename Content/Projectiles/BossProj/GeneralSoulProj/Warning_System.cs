using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Warning_System : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: World UI"));
            if (index == -1) index = layers.Count - 1; // 如果找不到，就加到最后
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("MortalDao: Global Warning Lines", delegate 
                {
                    DrawGlobalWarnings();   
                        return true;
                },
                InterfaceScaleType.Game)
                );
            }
        }

        private void DrawGlobalWarnings()
        {
            List<int> toRemove = new List<int>();
            if (Milion_Arrow.ActiveWarnings.Count == 0) return;
            Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;
            foreach (var kvp in Milion_Arrow.ActiveWarnings)
            {
                int projId = kvp.Key;
                // 如果弹幕不存在了，标记为删除
                if (projId >= Main.maxProjectiles || !Main.projectile[projId].active || Main.projectile[projId].type != ModContent.ProjectileType<Milion_Arrow>())
                {
                    toRemove.Add(projId);
                }
            }
            foreach (int id in toRemove)
            {
                Milion_Arrow.ActiveWarnings.Remove(id);
            }
            if (Milion_Arrow.ActiveWarnings.Count == 0) return;
            foreach (var warning in Milion_Arrow.ActiveWarnings)
            {
                var warningData = warning.Value;
                Vector2 startPoint = warningData.Pos;
                Vector2 direction = warningData.Dir;
                float lineLength = 10000f; // 超长线，确保穿透屏幕

                sb.Draw(
                    tex,
                    startPoint - Main.screenPosition, // 转换到屏幕坐标
                    new Rectangle(0, 0, 1, 1),
                    Color.LightGreen * 0.5f,
                    direction.ToRotation(),
                    new Vector2(0f, 0.5f),
                    new Vector2(lineLength, 10f),
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}
