using Terraria.ModLoader;
namespace MortalDao.Content.ModSetting
{
    public class KeyBindSystem : ModSystem
    {
        public static ModKeybind OpenRealmMenu { get; set; }
        public override void Load()
        {
            OpenRealmMenu = KeybindLoader.RegisterKeybind(Mod, "Open Realm Menu", "P");
        }
        public override void Unload()
        {
            OpenRealmMenu = null;
        }
    }
}
