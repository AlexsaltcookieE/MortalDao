using log4net.Core;
using MortalDao.Content.ModSetting.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace MortalDao.Content.ModSetting
{
    public class MainLevelSys : ModPlayer
    {
        //经验
        public long EXP;
        public long EXPNeed;
        //等级
        public int Level;
        //UI
        public bool UINeedsRefresh { get; set; }
        private static LocalizedText GetRealmUIText(string entryName) => MortalDaoUtils.GetText($"Level.UI.Name.Level.{entryName}");
        public static long CalculateExpForLevel(int targetLevel)
        {
            if(targetLevel == 1)
            {
                return (long)50;
            }
            if(targetLevel == 2)
            {
                return (long) 70;
            }
            if(targetLevel == 3)
            {
                return (long) 200;
            }
                return 9999;
        }
        public void AddExperience(long amount)
        {
            EXP += amount;
            while (EXP >= EXPNeed)
            {
                EXP -= EXPNeed;
                if (Level == 0)
                {
                    Level = 1;
                    EXPNeed = CalculateExpForLevel(Level + 1);
                    if (Main.myPlayer == Player.whoAmI)
                    { // 只在本机飘字
                        CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Gold, GetRealmUIText("OnUpGrade").Value + GetRealmUIText("One").Value);
                        SoundEngine.PlaySound(SoundID.AchievementComplete, Player.position);
                    }
                }
                else if (Level == 1)
                {
                    Level++;
                    EXPNeed = CalculateExpForLevel(Level + 1);
                    if (Main.myPlayer == Player.whoAmI)
                    { // 只在本机飘字
                        CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Gold, GetRealmUIText("OnUpGrade").Value + GetRealmUIText("Two").Value);
                        SoundEngine.PlaySound(SoundID.AchievementComplete, Player.position);
                    }
                }
                else if(Level >= 2)
                {
                    if (NPC.downedSlimeKing)
                    {
                        Level++;
                        EXPNeed = CalculateExpForLevel(Level + 1);
                        if (Main.myPlayer == Player.whoAmI)
                        { // 只在本机飘字
                            CombatText.NewText(Player.getRect(), Microsoft.Xna.Framework.Color.Gold, GetRealmUIText("OnUpGrade").Value + GetRealmUIText("Three").Value);
                            SoundEngine.PlaySound(SoundID.AchievementComplete, Player.position);
                        }
                    }
                    else
                    {
                        EXP = EXPNeed;
                        UINeedsRefresh = true;
                        return;
                    }
                }
            }
            UINeedsRefresh = true;
        }
        public override void SaveData(TagCompound tag)
        {
            tag["Level"] = Level;
            tag["EXP"] = EXP;
        }
        public override void LoadData(TagCompound tag)
        {
            Level = tag.GetInt("Level");
            EXP = tag.GetLong("EXP");

            // 重新计算升级所需经验
            EXPNeed = CalculateExpForLevel(Level + 1);
            UINeedsRefresh = true;
        }
        public override void Initialize()
        {
            Level = 0;
            EXP = 0;
            EXPNeed = CalculateExpForLevel(2); // Lv1 → Lv2
            UINeedsRefresh = true;
        }
        public override bool CanUseItem(Item item)
        {
            if(item.type == ItemID.LifeCrystal)
            {
                return CanUseLifeCrystal(item);
            }
            return base.CanUseItem(item);
        }

        private bool CanUseLifeCrystal(Item item)
        {
            if (Level == 0)
            {
                if (Player.ConsumedLifeCrystals == 0)
                {
                    return true;
                }
                return false;
            }
            //一级
            if (Level == 1)
            {
                if (Player.ConsumedLifeCrystals < 4)
                {
                    return true;
                }
                return false;
            }
            //二级
            if (Level == 2)
            {
                if (Player.ConsumedLifeCrystals < 6)
                {
                    return true;
                }
                return false;
            }
            if (Level > 2)
            {
                return true;
            }
            return false;
        }
    }
}