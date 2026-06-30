using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.ModSetting.Utilities;
using MortalDao.Content.NPCs.BOSS.DarkGaze;
using MortalDao.Content.NPCs.BOSS.FiveElement.GoldElement;
using SteelSeries.GameSense;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Terraria.GameContent.Animations.Actions.Sprites;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class RobberAttackSpawn : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (!RobberAttackEvent.EventActive == true) return;
            pool.Clear();
            pool.Add(ModContent.NPCType<RobberWolf>(), 1f);
            pool.Add(ModContent.NPCType<AxeRobber>(), 1f);
            pool.Add(ModContent.NPCType<DartRobber>(), 1f);
            pool.Add(ModContent.NPCType<thiefRobber>(), 1f);
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            spawnRate = (int)(spawnRate * 0.25f);
            base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
        }
    }
    public class RobberAttackEvent : ModSystem
    {
        private static LocalizedText GetSpawnInfo(string entryName) => MortalDaoUtils.GetText($"OnspawnMessage.{entryName}");
        private static LocalizedText GetDeSpawnInfo(string entryName) => MortalDaoUtils.GetText($"DespawnMessage.{entryName}");
        //
        public static bool EventActive = false;
        public static int TotalRobbers = 50;
        public static int RemainingRobbers = 0;
        public static float Progress = 0;
        public static string progressText;

        //Animation
        private float uiScale = 0f;  // 当前缩放值
        private const float SCALE_SPEED = 0.08f;  // 缩放速度

        public override void PostUpdateNPCs()
        {
            if (!EventActive)
            {
                uiScale = MathHelper.Lerp(uiScale, 0f, SCALE_SPEED);
                return;
            }
            uiScale = MathHelper.Lerp(uiScale, 1f, SCALE_SPEED);
            //
            Progress = (float)RemainingRobbers / TotalRobbers;
            progressText = GetSpawnInfo("RobberAttackProgress").Value + $"{(int)((float)RemainingRobbers / TotalRobbers * 100)}%";
            if (RemainingRobbers >= TotalRobbers)
            {
                EventActive = false;
                if (Main.netMode != NetmodeID.MultiplayerClient) // 确保只在服务器/单机生成
                {
                    // 选择一个玩家作为生成点（例如主玩家）
                    Player targetPlayer = Main.LocalPlayer;
                    int spawnX = (int)targetPlayer.Center.X;
                    int spawnY = (int)targetPlayer.Center.Y - 100; // 在玩家上方生成

                    // 生成九筒
                    NPC.NewNPC(
                        NPC.GetSource_NaturalSpawn(),
                        spawnX,
                        spawnY,
                        ModContent.NPCType<JiuTong>()
                    );
                }
                Main.NewText(GetDeSpawnInfo("RobberAttack.DespawnText").Value, 255, 200, 50);
                BossesDowned.DownedRobberAttack = true;
                return;
            }
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["RobberEventActive"] = EventActive;
            tag["RobberEventRemaining"] = RemainingRobbers;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            EventActive = tag.GetBool("RobberEventActive");
            RemainingRobbers = tag.GetInt("RobberEventRemaining");
        }
        public override void ClearWorld()
        {
            EventActive = false;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int idx = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (idx == -1) return;
            layers.Insert(idx, new LegacyGameInterfaceLayer(
                "MortalDao:RobberAttack",
                () =>
                {
                    if (EventActive)
                    {
                        DrawInvasionInfo(Main.spriteBatch);
                    }
                    return true;
                },
                InterfaceScaleType.UI));
        }
        private Color InvasionBarColor = Color.Yellow;
        private void DrawBlueBar(SpriteBatch sb, Vector2 barDrawPosistion, int barOffsetY)
        {
            int BarWidth = 200;
            int BarHeight = 45;
            barDrawPosistion.Y += barOffsetY;

            Rectangle ScreenCoordinate = new Rectangle((int)barDrawPosistion.X - BarWidth / 2, (int)barDrawPosistion.Y - BarHeight / 2, BarWidth, BarHeight);
            Texture2D barTexture = TextureAssets.ColorBar.Value;

            Utils.DrawInvBG(sb, ScreenCoordinate, new Color(63, 65, 151, 255) * 0.785f);
            sb.Draw(barTexture, barDrawPosistion, null, Color.White, 0f, new Vector2(barTexture.Width / 2, 0f), 1f, SpriteEffects.None, 0f);
        }
        private void DrawProgressText(SpriteBatch sb, float yScale, Vector2 baseBarDrawPosition, int barOffsetY, out Vector2 newBarPosition)
        {
            var font = FontAssets.MouseText.Value;
            if (font == null || progressText == null)
            {
                newBarPosition = baseBarDrawPosition + Vector2.UnitY * (yScale + barOffsetY);
                return;
            }
            Vector2 textSize = font.MeasureString(progressText);
            float progressTextScale = 1f;
            if (textSize.Y > 22f)
            {
                progressTextScale *= 22f / textSize.Y;
            }
            newBarPosition = baseBarDrawPosition + Vector2.UnitY * (yScale + barOffsetY);
            Utils.DrawBorderString(sb, progressText, newBarPosition - Vector2.UnitY * 4f, Color.White, progressTextScale, 0.5f, 1f, -1);
        }
        private void DrawBackground(SpriteBatch sb, float yScale, Vector2 baseBarDrawPosition, int barOffsetY)
        {
            float barDrawOffsetX = 169f;
            //CompleteRatio
            Vector2 barDrawPosition = baseBarDrawPosition + Vector2.UnitX * (Progress - 0.5f) * barDrawOffsetX;
            sb.Draw(TextureAssets.MagicPixel.Value, barDrawPosition, new Rectangle(0, 0, 1, 1), new Color(255, 241, 51), 0f, new Vector2(1f, 0.5f), new Vector2(barDrawOffsetX * 0, yScale), SpriteEffects.None, 0f);
            sb.Draw(TextureAssets.MagicPixel.Value, barDrawPosition, new Rectangle(0, 0, 1, 1), new Color(255, 165, 0, 127), 0f, new Vector2(1f, 0.5f), new Vector2(2f, yScale), SpriteEffects.None, 0f);
            sb.Draw(TextureAssets.MagicPixel.Value, barDrawPosition, new Rectangle(0, 0, 1, 1), Color.Black, 0f, Vector2.UnitY * 0.5f, new Vector2(barDrawOffsetX * (1f - Progress), yScale), SpriteEffects.None, 0f);
        }
        private void DrawProgressTextAndIcons(SpriteBatch sb, int barOffsetY)
        {
            Texture2D IconTexture = ModContent.Request<Texture2D>("MortalDao/Content/NPCs/Attacks/RobberAttack/RobberAttackIcon").Value;
            Vector2 textMeasurement = FontAssets.MouseText.Value.MeasureString(GetSpawnInfo("RobberAttackName").Value);
            float x = 120f;
            if (textMeasurement.X > 200f)
            {
                x += textMeasurement.X - 200f;
            }
            //
            Rectangle iconRectangle = Utils.CenteredRectangle(new Vector2(Main.screenWidth - x, Main.screenHeight - 80 + barOffsetY), textMeasurement + new Vector2(IconTexture.Width + 12, 6f));
            Utils.DrawInvBG(sb, iconRectangle, InvasionBarColor * 0.5f);
            sb.Draw(IconTexture,iconRectangle.Left() + Vector2.UnitX * 8f, null, Color.White, 0f, Vector2.UnitY * IconTexture.Height / 2, 0.8f, SpriteEffects.None, 0f);
            Utils.DrawBorderString(sb,GetSpawnInfo("RobberAttackName").Value, iconRectangle.Right() + Vector2.UnitX * -16f, Color.White, 0.9f, 1f, 0.4f, -1);
        }
        public void DrawInvasionInfo(SpriteBatch sb)
        {
            if (!EventActive)
            {
                return;
            }
            int barOffsetY = 0;
            int totalBars = 0;

            if(Main.invasionProgressNearInvasion || Main.invasionProgressAlpha > 0f)
            {
                totalBars++;
            }
            Vector2 barDrawPosition = new Vector2(Main.screenWidth - 120, Main.screenHeight - 40);
            DrawBlueBar(sb, barDrawPosition, barOffsetY);
            float yScale = 8f;
            DrawProgressText(sb, yScale, barDrawPosition, barOffsetY, out Vector2 newBarPosition);
            DrawBackground(sb, yScale, newBarPosition, barOffsetY);
            DrawProgressTextAndIcons(sb, barOffsetY);
        }
    }
}
