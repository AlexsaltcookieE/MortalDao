using Terraria.ModLoader;
using Terraria.ModLoader.IO;

public class BossesDowned : ModSystem
{
    public static bool DownedGoldElementBoss = false;
    public override void SaveWorldData(TagCompound tag)
    {
        tag["DownedGoldElementBoss"] = DownedGoldElementBoss;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        DownedGoldElementBoss = tag.ContainsKey("DownedGoldElementBoss") && tag.GetBool("DownedGoldElementBoss");
    }
    public override void ClearWorld()
    {
        DownedGoldElementBoss = false;
    }
}