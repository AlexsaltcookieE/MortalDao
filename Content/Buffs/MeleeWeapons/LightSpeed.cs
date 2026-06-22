using MortalDao.Content.Items.MeleeWeapons;
using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.MeleeWeapons
{
    public class LightSpeed : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<LightSpeedPlayer>().HasLightSpeed = true;
        }
    }
    public class LightSpeedPlayer : ModPlayer
    {
        public bool HasLightSpeed;
        private bool _prevHasLightSpeed;


        private readonly float BoostSpeed = 0.5f;    // 玩家速度提升50%
        private readonly float BoostJump = 0.15f;    // 跳跃高度提升15%
        private readonly float AddBoostSpeed = 0.20f; // 加速度提升20%
        public override void ResetEffects()
        {
            HasLightSpeed = false;
        }
        public override void PostUpdate()
        {
            if(Player.HeldItem.type != ModContent.ItemType<YueMaidenSword>())
            {
                Player.ClearBuff(ModContent.BuffType<LightSpeed>());
            }
        }
        public override void PostUpdateRunSpeeds()
        {
            if (!HasLightSpeed && _prevHasLightSpeed)
            {
                int cdType = ModContent.BuffType<LightSpeedCoolDown>();
                Player.AddBuff(cdType, 60 * 30); // 举例 8 秒，按需要改
            }
            if (HasLightSpeed)
            {
                ApplySpeedAndJumpBoost();
            }
            _prevHasLightSpeed = HasLightSpeed;
        }
        private void ApplySpeedAndJumpBoost()
        {
            Player.moveSpeed += BoostSpeed;
            Player.maxRunSpeed += BoostSpeed;
            Player.accRunSpeed += BoostSpeed;
            Player.wingAccRunSpeed += BoostSpeed;
            Player.runAcceleration += AddBoostSpeed;
            Player.jumpHeight = 20;
        }
    }
}
