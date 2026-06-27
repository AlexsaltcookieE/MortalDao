using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class BossesDowned : ModSystem
{
    public static bool DownedRobberAttack = false;
    public static bool DownedGoldElementBoss = false;
    public override void SaveWorldData(TagCompound tag)
    {
        tag["DownedRobberAttack"] = DownedRobberAttack;
        tag["DownedGoldElementBoss"] = DownedGoldElementBoss;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        DownedGoldElementBoss = tag.ContainsKey("DownedGoldElementBoss") && tag.GetBool("DownedGoldElementBoss");
        DownedRobberAttack = tag.ContainsKey("DownedRobberAttack") && tag.GetBool("DownedRobberAttack");
    }
    public override void ClearWorld()
    {
        DownedRobberAttack = false;
        DownedGoldElementBoss = false;
    }
}