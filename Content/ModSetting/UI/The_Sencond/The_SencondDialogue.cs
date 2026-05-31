using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MortalDao.Content.ModSetting.UI.The_Sencond
{
    public class DialogueOption
    {
        public string Label;
        public System.Func<bool> Condition;          // 是否显示这个选项
        public System.Action<TheSencondUI> OnSelect; // 点了干嘛

        public DialogueOption(string label, Action<TheSencondUI> onSelect, Func<bool> cond = null)
        {
            Label = label; OnSelect = onSelect; Condition = cond;
        }
    }

    public class DialogueNode
    {
        public string Text;
        public Texture2D Portrait;                     // 可选头像
        public System.Collections.Generic.List<DialogueOption> Options = new();

        public DialogueNode(string text) { Text = text; }
    }

    // ============================================================
    // 把你的剧情节点组织在这里
    // ============================================================
    public static class DialogueTree
    {
        public static DialogueNode TheSencondFirstMeeting()
        {
            var root = new DialogueNode("");

            // 选项A → 推子节点
            root.Options.Add(new DialogueOption("「你好啊，我能问你点事情吗」", ui =>
            {
                ui.PushNode(new DialogueNode("当然可以,你想知道点什么?")
                {
                    Options =
                    {
                        new DialogueOption("我醒来之后发现什么都不记得了,这是哪里?", ui =>
                        {
                            ui.PushNode(new DialogueNode("这里是逸风岭，这里便是隙离之地最中心的位置，\n传闻这里灵石矿蕴丰富,但十分危险。")
                            {
                                Options =
                                {
                                    new DialogueOption("为什么我会在这里?", ui =>
                                    {
                                        ui.PushNode(new DialogueNode("这我怎么知道，我只是路过，看你昏迷不醒，就叫醒了你")
                                        {
                                            Options =
                                            {
                                                new DialogueOption("谢啦,我能再问你点事吗", ui =>
                                                {
                                                    ui.PopNode();
                                                    ui.PopNode();
                                                    ui.PopNode();
                                                }),
                                            }
                                        });
                                    }),

                                }
                            });
                        }),
                        new DialogueOption("我这里有卷卷宗，上面有我不认识的字，你看得懂吗?", ui =>
                        {
                            ui.PushNode(new DialogueNode("这不是鬼画符吗，我也不知道上面写的是什么。")
                            {
                                Options =
                                {
                                    new DialogueOption("麻烦了,我能再问你点事吗", ui1 =>
                                                {
                                                    ui.PopNode();
                                                    ui.PopNode();
                                                }),
                                }
                            });
                        }),
                    }
                });
            }));
            // 选项B → 直接关对话
            root.Options.Add(new DialogueOption("「我还有事，晚点再来。」", ui =>
            {
                TheSencondDialogueSystem.Close();
            }));
            return root;
        }
    }
}
