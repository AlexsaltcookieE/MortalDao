using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TestShader
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class TestShader : Mod
	{
		// Effect asset for the ImpFlameTrail shader
		public static ReLogic.Content.Asset<Effect> ImpFlameEffect;
		public static ReLogic.Content.Asset<Texture2D> ScarletDevilStreak;
		public override void Load()
		{
			if (!Main.dedServ)
			{
				ImpFlameEffect = ModContent.Request<Effect>("TestShader/Effects/ImpFlameTrail");
				ScarletDevilStreak = ModContent.Request<Texture2D>("TestShader/Content/Textures/ScarletDevilStreak");
			}
		}

		public override void Unload()
		{
			ImpFlameEffect = null;
			ScarletDevilStreak = null;
		}
    }
}
