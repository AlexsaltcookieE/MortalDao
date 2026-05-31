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
        public static GameTime CurrentGameTime = new();
        public static void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            State = new TheSencondUI();
            Interface.SetState(null);
        }
        public static void Open()
        {
            if (State == null)
                Load();
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
            Main.blockInput = false; // ✅ 防止卡输入
        }
    }
    public class SolynDialogueUISystem : ModSystem
    {
        public override void Load()
        {
            // ✅ 确保系统在 Mod 加载时初始化
            TheSencondDialogueSystem.Load();
        }
        public override void UpdateUI(GameTime gameTime)
        {
            TheSencondDialogueSystem.CurrentGameTime = gameTime;
            TheSencondDialogueSystem.Interface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx == -1) idx = layers.Count;

            layers.Insert(idx, new LegacyGameInterfaceLayer(
                "MortalDao: ThesencondDialogue",
                () =>
                {
                    TheSencondDialogueSystem.Interface?
                        .Draw(Main.spriteBatch, TheSencondDialogueSystem.CurrentGameTime);
                    return true;
                },
                InterfaceScaleType.UI
            ));
        }
    }
}
