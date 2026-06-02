using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MortalDao.Content.ModSetting.UI.The_Sencond
{
    public static class TheSencondDialogueSystem
    {
        public static UserInterface Interface = new();
        public static TheSencondUI State;

        public static void Load()
        {
            if (Main.dedServ) return;   // ✅ 专用服务器直接滚
            State = new TheSencondUI();
            Interface.SetState(null);
        }

        public static void Open()
        {
            if (Main.dedServ) return;   // ✅ 再次防一手
            if (State == null) Load();

            Main.npcChatText = "";
            Main.npcChatFocus1 = false;
            Main.npcChatFocus2 = false;

            State.PushNode(DialogueTree.TheSencondFirstMeeting());
            Interface.SetState(State);
        }

        public static void Close()
        {
            Interface.SetState(null);
            Main.player[Main.myPlayer].mouseInterface = false;
            Main.blockInput = false;
        }
    }

    public class SolynDialogueUISystem : ModSystem
    {
        public override void Load()
        {
            TheSencondDialogueSystem.Load();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ) return;   // ✅ 服务器不 Update
            TheSencondDialogueSystem.Interface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ) return;   // ✅ 服务器不插 UI 层

            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx == -1) idx = layers.Count;

            layers.Insert(idx, new LegacyGameInterfaceLayer(
                "MortalDao: ThesencondDialogue",
                () =>
                {
                    // ✅ 只在有 UI 时 Draw
                    if (TheSencondDialogueSystem.Interface?.CurrentState != null)
                        TheSencondDialogueSystem.Interface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);

                    return true;
                },
                InterfaceScaleType.UI
            ));
        }
    }
}