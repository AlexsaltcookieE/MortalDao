using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace MortalDao.Content.ModSetting.UI.DocumentUI
{
    public class OldPlayerDiaryUI : UIState
    {
        private UIPanel _panel;
        public override void OnInitialize()
        {
            _panel = new UIPanel();
            _panel.Width.Set(400, 0.5f);
            _panel.Height.Set(700, 0);
            _panel.Left.Set(250, 0);
            _panel.Top.Set(150, 0);
            
            Append(_panel);
        }
        public override void OnActivate()
        {
            base.OnActivate();
            RefreshContent();
        }
        private void RefreshContent()
        {
            _panel.RemoveAllChildren();
            if (Main.hardMode)
            {
                _panel.BackgroundColor = Color.DarkRed;
                _panel.BorderColor = Color.Red;

                var text = new UIText(
                    "我要报仇！", 1f);
                text.HAlign = 0.5f;
                text.VAlign = 0.5f;
                _panel.Append(text);
            }
            else
            {
                _panel.BackgroundColor = Color.LightBlue;
                _panel.BorderColor = Color.CornflowerBlue;

                var text = new UIText(
                    "!@#&!#()$*$&!$#(#!*#*!#_)#(#*!(*!*&#*#)(!&*#*(&^#(*!$&(*!$*U$*()*!$&*!$$*(!&*)$&*!*$\n#!Y#*(*!()#&**()(!#&#*&#*_#&*#&#!*$^$&*&!*#&#*#!&#*^##&^#*(#!&#(*^!(*#\n#&**#!&^*#!&#)(!*()#!&!#)(##(!*#(#!#*#*&!*(^!)#!#)(#)(*!&#*()!*#_!()*#()", 1f);
                text.HAlign = 0.5f;
                text.VAlign = 0.5f;
                _panel.Append(text);
            }
        } 
    }
}