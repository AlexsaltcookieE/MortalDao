using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MortalDao.Content.Items.Specials;
using MortalDao.Content.ModSetting.Business;
using MortalDao.Content.ModSetting.UI;
using MortalDao.Content.ModSetting.UI.The_Sencond;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace MortalDao.Content.NPCs.TownNPCs
{
    // AutoloadHead：自动去同目录下找 LazyNPC_Head.png 作为头像
    //               （地图小旗、对话窗头像都要靠它）
    [AutoloadHead]
    public class The_Second : ModNPC
    {
        // ---------- 贴图路径 ----------
        // 对应 MyMod/Content/NPCs/The_Second.png
        public override string Texture => "MortalDao/Content/NPCs/TownNPCs/The_Second";
        private const string ShopName = "灵石交易";

        //对话
        public override void SetStaticDefaults()
        {
            // ★ 这行必须跟你 LazyNPC.png 的实际帧数一致！
            // 如果你直接拿原版向导/镇民贴图改：一般是 25 帧（0~24）
            // 如果你自己画了一套：量一下 (总高 ÷ 单帧高) = 填这里
            Main.npcFrameCount[Type] = 17;
            // ---- 下面这组是城镇NPC"坐下/闲聊/社交"所需的额外帧区间 ----
            // 大多数城镇NPC模板：前 16 帧是站立+行走，后 9 帧是坐下等
            // 你如果不想深究：照抄就好，等你换自己的贴图再调
            NPCID.Sets.ExtraFramesCount[Type] = 1;     // 非行走的"额外"帧数
            NPCID.Sets.AttackFrameCount[Type] = 0;     // 我们不攻击 → 攻击帧数 0
            NPCID.Sets.DangerDetectRange[Type] = 0;    // 不索敌
            NPCID.Sets.AttackType[Type] = -1;           // 无攻击类型
            NPCID.Sets.AttackTime[Type] = 0;
            NPCID.Sets.AttackAverageChance[Type] = 0;
            NPCID.Sets.FaceEmote[Type] = 0;         // 如果你NPC戴派对帽时帽子位置不对，调这个偏移：
            NPCID.Sets.HatOffsetY[Type] = 0;
            NPCID.Sets.NPCFramingGroup[Type] = -1; // 不参与NPC社交分组
        }

        // ---------- 运行时属性 ----------
        public override void SetDefaults()
        {
            NPC.homeless = true;
            NPC.townNPC = true;   // ★ 标记为城镇NPC（住宅系统认它）
            NPC.friendly = true;  // 不会伤玩家
            NPC.width = 18;      // 碰撞/交互命中框 宽
            NPC.height = 40;      // 碰撞/交互命中框 高
            NPC.aiStyle = NPCAIStyleID.Passive; // = 7 原版城镇被动AI              // 包含：回家/找椅子坐下/躲避怪物/跟随玩家对话
            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
        }
        // ---------- 什么时候允许入住 ----------
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            // 条件随便你改，现在写的是：
            // "只要有空合格房，就可能出现"（最松）
            // 常见加条件：return Main.hardMode; 之类
            return true;
        }

        // ---------- 对话 ----------
        public override string GetChat()
        {
            return GetCurrentLine();
        }
        private string GetCurrentLine()
        {
            int pick = WorldGen.genRand.Next(5);
            return pick switch
            {
                0 => "吾酒馆闭了，望高人许我此处下榻",
                1 => "吾消息灵通，需要得知何事?",
                2 => "客官可知，灵石可换我口袋宝物",
                3 => "传闻万年前，此间还无人类",
                4 => "吾相信天地神明会好好保护我们",
                5 => "客官丢了重要的东西吗，吾这里捡到了了",
                _ => "客官，需要来一壶好酒吗",
            };
        }
        // ---------- 对话按钮 ----------
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = ShopName;
            button2 = "聊天";
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                // 原版商店流程：给它一个 shopName，后续 ModifyActiveShop 会接管
                shopName = ShopName;
            }
            else
            {
                // 你的聊天UI
                Main.npcChatText = "";
                Main.npcChatFocus1 = false;
                Player player = Main.LocalPlayer;
                if (Main.LocalPlayer.active && Main.myPlayer == player.whoAmI)
                {
                    TheSencondDialogueSystem.Open();
                }
            }
        }
        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            Item MysteryBag = new Item(ModContent.ItemType<MysteryBag>());
            MysteryBag.value = MysteryBag.value;
            MysteryBag.shopSpecialCurrency = DownGradeLingShiBusiness.CurrencyID; // 绑定灵石
            shop.Add(MysteryBag, Condition.NpcIsPresent(ModContent.NPCType<The_Second>()));
            //锦囊
            shop.Register();
        }
        public override void OnKill()
        {
            
        }
    }
}