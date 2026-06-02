using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.QuestSystem;
using System;
using Terraria;
using Terraria.ID;

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
                        new DialogueOption("跟我说说你吧?", ui =>
                        {
                            ui.PushNode(new DialogueNode("唉，造化弄人，我刚刚还是从老家回来的一名城内驿站的店员，但是我刚刚受到飞鸽传信，说老板失踪了，所以我失业了\n我现在无处可去，你有什么办法吗?")
                            {
                                Options =
                                {
                                    new DialogueOption("我看看能不能给你一个住处吧,***接取任务***", ui =>
                                    {
                                        if (!GlobalTaskSystem.GlobalTasks.ContainsKey(1))
                                            GlobalTaskSystem.GlobalTasks[1] = TaskStatus.NotStarted;
                                        GlobalTaskSystem.SyncTask(1, TaskStatus.Accepted);
                                        var thankYouNode = new DialogueNode("太好了,谢谢你了,");
                                        thankYouNode.Options.Add(new DialogueOption("不客气,我先去忙了", ui =>
                                        {
                                            TheSencondDialogueSystem.Close();
                                        }));
                                        ui.PushNode(thankYouNode); // 显示感谢节点
                                    },() =>
                                    {
                                        bool notAccepted = !GlobalTaskSystem.GlobalTasks.ContainsKey(1) || GlobalTaskSystem.GlobalTasks[1] == TaskStatus.NotStarted;
                                        bool notCompleted = !GlobalTaskSystem.GlobalTaskStates.ContainsKey(1) ||!GlobalTaskSystem.GlobalTaskStates[1];
                                        return notAccepted && notCompleted;
                                    }),
                                    new DialogueOption("我看再说吧,我也不清楚这里的情况", ui =>
                                    {
                                        ui.PopNode();
                                        ui.PopNode();
                                        ui.PopNode();
                                    })
                                }
                            });
                        })
                    }
                });
            }));
            root.Options.Add(new DialogueOption("「这房间还满意吗？/ 给你安排好了」", ui =>
            {
                GlobalTaskSystem.RequestCompleteTask(1, Main.LocalPlayer);
                if(Main.netMode == NetmodeID.SinglePlayer)
                {
                    GlobalTaskSystem.SyncTask(1, TaskStatus.Completed);
                }
                var doneNode = new DialogueNode("还行吧，虽然有点破旧，但总比露宿街头强。\n以后我就在这儿落脚了，多谢啊！");
                doneNode.Options.Add(new DialogueNode("不客气，有事再找我")
                {
                    Options =
                {
                    new DialogueOption("（离开）", ui2 => TheSencondDialogueSystem.Close())
                }
                }.Options[0]); // 借用已有结构
                               // 简化写法：
                var thanksNode = new DialogueNode("还行吧，虽然有点破旧，但总比露宿街头强。");
                thanksNode.Options.Add(new DialogueOption("（离开）", ui2 => TheSencondDialogueSystem.Close()));
                ui.PushNode(thanksNode);
            }, () => 
            {
                bool can = GlobalTaskSystem.CanCompleteTask(1);
                return can;
            }
            ));
            // 选项B → 直接关对话
            root.Options.Add(new DialogueOption("「我还有事，晚点再来。」", ui =>
            {
                TheSencondDialogueSystem.Close();
            }));
            return root;
        }
    }
}
