using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace MortalDao.Content.Projectiles.BossProj.FiveElementProj.GoldElementProj
{
    public class CameraLockSystem : ModSystem
    {
        public static Vector2? LockedCenter;

        public override void ModifyScreenPosition()
        {
            if (!LockedCenter.HasValue || !Main.LocalPlayer.active)
                return;
            float zoom = Main.GameZoomTarget;
            Vector2 target = LockedCenter.Value - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f / zoom;
            Main.screenPosition = target;
        }
    }
}