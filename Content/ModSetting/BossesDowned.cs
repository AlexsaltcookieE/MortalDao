using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class BossesDowned : ModSystem
{
    public static bool DownedRobberAttack = false;
    public static bool DownedJiuTong = false;
    public static bool DownedGoldElementBoss = false;
    public override void SaveWorldData(TagCompound tag)
    {
        tag["DownedRobberAttack"] = DownedRobberAttack;
        tag["DownedJiuTong"] = DownedRobberAttack;
        tag["DownedGoldElementBoss"] = DownedGoldElementBoss;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        DownedGoldElementBoss = tag.ContainsKey("DownedGoldElementBoss") && tag.GetBool("DownedGoldElementBoss");
        DownedRobberAttack = tag.ContainsKey("DownedRobberAttack") && tag.GetBool("DownedRobberAttack");
        DownedJiuTong = tag.ContainsKey("DownedJiuTong") && tag.GetBool("DownedJiuTong");
    }
    public override void ClearWorld()
    {
        DownedRobberAttack = false;
        DownedGoldElementBoss = false;
        DownedJiuTong = false;
    }
}