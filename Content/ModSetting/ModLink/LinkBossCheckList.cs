using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Items.SummonItems;
using MortalDao.Content.ModSetting.Utilities;
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
        private static LocalizedText GetSpawnInfo(string entryName) => MortalDaoUtils.GetText($"BCLInfo.{entryName}.SpawnInfo");


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
            //---------------------------------------------------------------------------------------------------------------------------
            AddBoss(bossCheckList, mortalDao, "GoldElement",3.5f,()=> BossesDowned.DownedGoldElementBoss, ModContent.NPCType<GoldBody>(),GoldElementInfo);
        }
    }
}
