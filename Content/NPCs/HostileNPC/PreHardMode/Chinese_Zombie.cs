
using Microsoft.Xna.Framework;
using MortalDao.Content.Items.Marterials;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.HostileNPC.PreHardMode
{
    public class Chinese_Zombie : ModNPC
    {
        private const float BounceVelY = -15.5f;
        private int targetDirection = 0;       // 目标方向（玩家所在方向）
        private float currentSpeed = 0f;      // 当前实际水平速度
        private const float Acceleration = 0.05f; // 加速度（越小惯性越大）
        private const float MaxSpeed = 3f;    // 最大水平速度
        private const float InertiaThreshold = 0.5f; // 惯性结束阈值（速度低于此值时可转向）
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5; // 设置NPC的帧数
        }
        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 50;
            NPC.scale = 1.4f;
            NPC.damage = 20;
            NPC.lifeMax = 200;
            NPC.defense = 5;
            NPC.knockBackResist = 0.5f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;      // "噗"的肉感受击声
            NPC.DeathSound = SoundID.NPCDeath2;   // 僵尸倒地死亡声
        }
        public override void AI()
        {
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1) return;
            Player target = Main.player[targetID];
            targetDirection = target.Center.X > NPC.Center.X ? 1 : -1;
            bool playerBelow = target.Center.Y > NPC.Center.Y + NPC.height;
            // 检测脚下 Tile
            Point tileBelow = NPC.Bottom.ToTileCoordinates();
            Tile belowTile = Framing.GetTileSafely(tileBelow.X, tileBelow.Y);
            bool standingOnPlatform =
                belowTile.HasTile &&
                TileID.Sets.Platforms[belowTile.TileType];
            if (standingOnPlatform && playerBelow)
            {
                NPC.noTileCollide = true;
                NPC.velocity.Y = 4f; // 向下掉落速度（可调）
            }
            else
            {
                NPC.noTileCollide = false;
            }
            float targetSpeed = targetDirection * MaxSpeed;
            if (Math.Abs(currentSpeed - targetSpeed) > InertiaThreshold)
            {
                currentSpeed = MathHelper.Lerp(currentSpeed, targetSpeed, Acceleration);
            }
            else
            {
                currentSpeed = targetSpeed;
            }
            NPC.direction = currentSpeed > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            NPC.velocity.X = currentSpeed;
            NPC.frameCounter++;
            if (NPC.frameCounter >= 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += 54;
                if (NPC.frame.Y >= 54 * Main.npcFrameCount[NPC.type])
                    NPC.frame.Y = 0;
            }
            if (NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = BounceVelY;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            base.HitEffect(hit);
        }
        public override void OnKill()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                int GoreType = Main.rand.Next(4, 5);
                Gore.NewGore(
                    NPC.GetSource_Death(),
                    NPC.position,
                    NPC.velocity,
                    GoreType
                );
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if(spawnInfo.Player.ZoneForest && spawnInfo.Player.ZoneOverworldHeight && !Main.dayTime)
            {
                return 0.1f;
            }
            if (spawnInfo.Player.ZoneGraveyard)
            {
                return 0.3f;
            }
            return 0;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(
                ModContent.ItemType<Blank_Talisman_Paper>(), // 物品类型
                3, // 掉落几率分母（1=100%）
                1, // 最小掉落数量
                2  // 最大掉落数量
            ));
        }
    }
}
