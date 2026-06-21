using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.Utilities;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MortalDao.Content.ModSetting.UI
{
    public class Realm : UIState
    {
        //
        private UIPanel panel;
        //
        private UIText TextRealm;
        private UIText TextEXP;
        private float PosBelowPlayerModel;
        //
        private RealmUICharacter PlayerModel;
        private bool modelInitialized;
        //
        private RealmEXPBar EXPBar;
        //LOC
        private static LocalizedText GetRealmUIText(string entryName) => MortalDaoUtils.GetText($"Level.UI.Name.{entryName}");
        public override void OnInitialize()
        {
            //Main Panel
            panel = new UIPanel();
            panel.BackgroundColor = (new Color(232, 220, 200)) * 0.7f ;
            panel.BorderColor = Color.Gray * 0;
            panel.Width.Set(0,0.7f);
            panel.Height.Set(0,0.7f);
            panel.HAlign = 0.5f;
            panel.VAlign = 0.5f;
            Append(panel); //这样没问题
        }
        public void RenderPlayerLayer()
        {
            // 每次打开 UI 都强制重建
            ClearPlayerModel();
            if (Main.LocalPlayer == null)
                return;
            PlayerModel = new RealmUICharacter(Main.LocalPlayer,animated:false, useAClone: true)
            {
                Width = { Percent = 0 },
                Height = { Percent = 0 }
            };
            PlayerModel.Width.Set(50, 0);
            PlayerModel.Height.Set(70, 0);
            PlayerModel.Left.Set(0, 0.01f);
            PlayerModel.Top.Set(20, 0);
            panel.Append(PlayerModel);
            modelInitialized = true;
        }
        public void RenderTextLayer()
        {
            ClearText();
            TextRealm = new UIText("等级：1");
            TextEXP = new UIText("修为：0 / 100");
            TextEXP.TextColor = Color.White;
            TextRealm.TextColor = Color.White;
            if (PlayerModel != null)
            {
                PosBelowPlayerModel = PlayerModel.Top.Pixels + PlayerModel.Height.Pixels + 10f;
            }
            SetupLeftText(TextEXP, ref PosBelowPlayerModel);
            SetupLeftText(TextRealm, ref PosBelowPlayerModel);
            panel.Append(TextRealm);
            panel.Append(TextEXP);
        }
        private void SetupLeftText(UIText text, ref float topOffset)
        {
            text.Left.Set(0, 0.01f);     // ← 左边距
            text.Top.Set(topOffset, 0);
            text.TextColor = Color.Black;
            topOffset += 28f;        // 行间距
        }
        public void RenderEXPBar()
        {
            EXPBar = new RealmEXPBar(200, 4f);
            EXPBar.Left.Set(0,0.01f);
            EXPBar.Top.Set(TextEXP.Top.Pixels + TextEXP.Height.Pixels + 60f, 0f);
            panel.Append(EXPBar);
            panel.Recalculate();
        }
        public void ClearPlayerModel()
        {
            if (PlayerModel != null)
            {
                panel.RemoveChild(PlayerModel);
                PlayerModel = null;
            }
            modelInitialized = false;
        }
        public void ClearText()
        {
            if(TextEXP != null)
            {
                panel.RemoveChild(TextEXP);
                TextEXP = null;
            }
            if(TextRealm != null)
            {
                panel.RemoveChild(TextRealm);
                TextRealm = null;
            }
        }
        public void ClearEXPBar()
        {
            if(EXPBar != null)
            {
                panel.RemoveChild(EXPBar);
                EXPBar = null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            var mainLevelSys = Main.LocalPlayer.GetModPlayer<MainLevelSys>();
            EXPBar.SetProgress(mainLevelSys.EXP,mainLevelSys.EXPNeed);
        }
        public void Refresh(MainLevelSys levelSys)
        {
            if(levelSys.Level == 0)
            {
                TextEXP.SetText(GetRealmUIText("Realm").Value + GetRealmUIText("Level.One").Value);
            }
            else if(levelSys.Level == 1)
            {
                TextEXP.SetText(GetRealmUIText("Realm").Value + GetRealmUIText("Level.Two").Value);
            }
            else if (levelSys.Level == 2)
            {
                TextEXP.SetText(GetRealmUIText("Realm").Value + GetRealmUIText("Level.Three").Value);
            }
            else if (levelSys.Level >= 2)
            {
                TextEXP.SetText(GetRealmUIText("Realm").Value + GetRealmUIText("Level.Four").Value);
            }
            TextRealm.SetText(GetRealmUIText("EXP").Value + $"{levelSys.EXP}/{levelSys.EXPNeed}");
        }
    }
}
