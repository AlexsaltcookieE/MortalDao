// Systems/RealmUISystem.cs
using Microsoft.Xna.Framework;
using MortalDao.Content.ModSetting;
using MortalDao.Content.ModSetting.UI;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
namespace MortalDao.Systems
{
    public class RealmUISystem : ModSystem
    {
        private bool HasRenderedText = false;
        //
        public UserInterface RealmInterface;
        internal Realm RealmUI;
        internal MainLevelSys mainlevelsys;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                RealmInterface = new UserInterface();
                RealmUI = new Realm();
                RealmUI.Activate();          // 触发 OnInitialize()
                RealmInterface.SetState(null); // 默认隐藏
            }
        }
        public override void Unload()
        {
            RealmUI = null;
            RealmInterface = null;
        }

        // 检测热键（官方推荐放这里）
        public override void PostUpdatePlayers()
        {
            if (Main.dedServ)
                return;
            var player = Main.LocalPlayer;
            var levelSys = player.GetModPlayer<MainLevelSys>();
            if (levelSys.UINeedsRefresh && RealmInterface?.CurrentState != null)
            {
                RealmUI.Refresh(levelSys);
                levelSys.UINeedsRefresh = false;
            }
            if (KeyBindSystem.OpenRealmMenu.JustPressed)
            {
                ToggleUI();
                RealmUI.RenderPlayerLayer();
                RealmUI.RenderTextLayer();
                RealmUI.RenderEXPBar();
                RealmUI.Refresh(levelSys);
            }
        }
        private void ToggleUI()
        {
            if (RealmInterface.CurrentState == null)
            {
                RealmInterface.SetState(RealmUI);
            }
            else
            {
                RealmInterface.SetState(null);
                RealmUI.ClearPlayerModel();
                RealmUI.ClearText();
            }
        }
        // 每帧 Update
        public override void UpdateUI(GameTime gameTime)
        {
            RealmInterface?.Update(gameTime);
        }
        // 插入绘制层
        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx == -1) return;

            layers.Insert(idx, new LegacyGameInterfaceLayer(
                "MortalDao:RealmMenu",
                () =>
                {
                    if (RealmInterface?.CurrentState != null)
                        RealmInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI));
        }
    }
}