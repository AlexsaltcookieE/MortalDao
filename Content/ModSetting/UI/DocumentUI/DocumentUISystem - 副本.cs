//using Microsoft.Xna.Framework;
//using System.Collections.Generic;
//using System.Linq;
//using Terraria;
//using Terraria.ModLoader;
//using Terraria.UI;

//namespace MortalDao.Content.ModSetting.UI.DocumentUI
//{
//    public class DocumentUISystem : ModSystem
//    {
//        internal DocumentUI bookUI;
//        private UserInterface uiInterface;

//        public override void Load()
//        {
//            bookUI = new DocumentUI();
//            bookUI.Activate();
//            uiInterface = new UserInterface();
//            uiInterface.SetState(null);
//        }

//        public void ToggleUI()
//        {
//            if (uiInterface.CurrentState == null)
//                uiInterface.SetState(bookUI);
//            else
//                uiInterface.SetState(null);
//        }

//        public override void UpdateUI(GameTime gameTime)
//        {
//            uiInterface?.Update(gameTime);
//        }

//        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
//        {
//            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
//            layers.Insert(idx, new LegacyGameInterfaceLayer(
//                "MortalDao: DocumentUI",
//                () =>
//                {
//                    uiInterface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
//                    return true;
//                },
//                InterfaceScaleType.UI));
//        }
//    }
//}