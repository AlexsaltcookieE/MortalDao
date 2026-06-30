using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Items.SummonItems;
using MortalDao.Content.ModSetting.Utilities;
using MortalDao.Content.NPCs.Attacks.RobberAttack;
using MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MortalDao.Content.ModSetting.ModLink
{
    public class LinkBossCheckList : ModSystem
    {
        private static void AddBoss(Mod BossChecklist_Mod, Mod HostMod, string name, float difficulty, Func<bool> downed, object npcTypes, Dictionary<string, object> extraInfo) => BossChecklist_Mod.Call("LogBoss",HostMod, name, difficulty, downed, npcTypes, extraInfo);
        private static void AddEvent(Mod bossChecklist, Mod hostMod, string name, float difficulty, Func<bool> downed, List<int> npcTypes, Dictionary<string, object> extraInfo) => bossChecklist.Call("LogEvent", hostMod, name, difficulty, downed, npcTypes, extraInfo);
        private static LocalizedText GetSpawnInfo(string entryName) => MortalDaoUtils.GetText($"BCLInfo.{entryName}.SpawnInfo");
        private static LocalizedText GetEventName(string entryName) => MortalDaoUtils.GetText($"OnspawnMessage.{entryName}");
        public override void PostSetupContent()
        {
            BossCheckListSupport();
        }

        private static void BossCheckListSupport()
        {
            Mod mortalDao = ModContent.GetInstance<MortalDao>();
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossCheckList))
            {
                return;
            }
            //---------------------------------------------------------------------------------------------------------------------------
            //强盗入侵
            //---------------------------------------------------------------------------------------------------------------------------
            Action<SpriteBatch, Rectangle, Color> RobberAttackPortrait = (SpriteBatch sb, Rectangle rect, Color color) =>
            {
                Texture2D texture = ModContent.Request<Texture2D>("MortalDao/Content/ExtraTextures/BCLTex/BCLRobberAttack").Value;
                Vector2 centered = new Vector2(rect.Center.X - (texture.Width / 2), rect.Center.Y - (texture.Height / 2));
                sb.Draw(texture, centered, color);
            };
            Dictionary<string, object> RobberAttackInfo = new Dictionary<string, object>()
            {
                ["spawnItems"] = ModContent.ItemType<RobberFlag>(),
                ["spawnInfo"] = () => GetSpawnInfo("RobberAttack"),
                ["overrideHeadTextures"] = "MortalDao/Content/NPCs/Attacks/RobberAttack/RobberAttackIcon",
                ["customPortrait"] = RobberAttackPortrait
            };
            List<int> RobberAttackNPCs = new List<int>()
            {
                ModContent.NPCType<RobberWolf>(),
                ModContent.NPCType<AxeRobber>(),
                ModContent.NPCType<DartRobber>(),
                ModContent.NPCType<thiefRobber>()
            };
            AddEvent(bossCheckList, mortalDao,GetEventName("RobberAttackName").Value, 1.1f, () => BossesDowned.DownedRobberAttack, RobberAttackNPCs, RobberAttackInfo);
            //---------------------------------------------------------------------------------------------------------------------------

            //---------------------------------------------------------------------------------------------------------------------------
            //九筒
            //---------------------------------------------------------------------------------------------------------------------------
            Action<SpriteBatch, Rectangle, Color> JiuTongPortrait = (SpriteBatch sb, Rectangle rect, Color color) =>
            {
                Texture2D texture = ModContent.Request<Texture2D>("MortalDao/Content/ExtraTextures/BCLTex/BCLJiuTong").Value;
                float scale = 2f;
                Vector2 centered = new Vector2(rect.Center.X - (texture.Width * scale / 2), rect.Center.Y - (texture.Height * scale / 2));
                sb.Draw(texture, centered, null, color, 0f,Vector2.Zero,scale, SpriteEffects.None, 0f);
            };
            Dictionary<string, object> JiuTongInfo = new Dictionary<string, object>()
            {
                ["spawnInfo"] = () => GetSpawnInfo("JiuTong"),
                ["customPortrait"] = JiuTongPortrait
            };
            AddBoss(bossCheckList, mortalDao, "JiuTong", 1.5f, () => BossesDowned.DownedJiuTong, ModContent.NPCType<JiuTong>(), JiuTongInfo);
            //---------------------------------------------------------------------------------------------------------------------------

            //---------------------------------------------------------------------------------------------------------------------------
            //金元素
            //---------------------------------------------------------------------------------------------------------------------------
            Action<SpriteBatch, Rectangle, Color> GoldElementPortrait = (SpriteBatch sb, Rectangle rect, Color color) =>
            {
                Texture2D texture = ModContent.Request<Texture2D>("MortalDao/Content/ExtraTextures/BCLTex/BCLGoldElement").Value;
                Vector2 centered = new Vector2(rect.Center.X - (texture.Width / 2), rect.Center.Y - (texture.Height / 2));
                sb.Draw(texture, centered, color);
            };
            Dictionary<string, object> GoldElementInfo = new Dictionary<string, object>()
            {
                ["spawnItems"] = ModContent.ItemType<Weird_Boulder>(),
                ["spawnInfo"] = () => GetSpawnInfo("GoldElement"),
                ["customPortrait"] = GoldElementPortrait
            };
            AddBoss(bossCheckList, mortalDao, "GoldElement", 3.5f, () => BossesDowned.DownedGoldElementBoss, ModContent.NPCType<GoldBody>(), GoldElementInfo);
            //---------------------------------------------------------------------------------------------------------------------------
        }
    }
}
