using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{
    public class CameraLockSystem : ModSystem
    {
        // ✅ 改用实例字段 + 本地缓存，避免 static 在多客户端冲突
        private static Vector2? _lockedCenter;
        public static Vector2? LockedCenter
        {
            get => _lockedCenter;
            set
            {
                // 只在值变化时打印日志，方便调试
                if (_lockedCenter != value)
                {
                    _lockedCenter = value;
                    // 可选：Debug用
                    // if (value.HasValue)
                    //     Main.NewText($"Camera locked to: {value.Value}");
                    // else
                    //     Main.NewText("Camera unlocked");
                }
            }
        }

        public override void ModifyScreenPosition()
        {
            // ✅ 基础安全检查
            if (!Main.LocalPlayer.active || Main.LocalPlayer.dead)
            {
                LockedCenter = null;
                return;
            }

            // ✅ 没有锁定目标，直接返回
            if (!LockedCenter.HasValue)
                return;

            // ✅ 正确检查：场上是否还有活跃的 GoldElementWarning
            bool warningExists = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active &&
                    proj.type == ModContent.ProjectileType<GoldElementWarning>())
                {
                    warningExists = true;
                    break;
                }
            }

            // ✅ 警告弹幕消失了，解锁视角
            if (!warningExists)
            {
                LockedCenter = null;
                return;
            }

            // ✅ 应用相机锁定
            float zoom = Main.GameZoomTarget;
            Vector2 screenHalf = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f / zoom;
            Main.screenPosition = LockedCenter.Value - screenHalf;
        }
    }
}