using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.NPCs.Attacks.RobberAttack
{
    public class RobberWolf : ModNPC
    {
        private enum Attack
        {
            CoolDown,
            Dash,
        }
        public override void OnKill()
        {
            RobberAttackEvent.RemainingRobbers = RobberAttackEvent.RemainingRobbers + 1;
            if (Main.netMode != NetmodeID.Server)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-1f, 1.5f), Main.rand.NextFloat(-4f, -1.0f)),181, NPC.scale * Main.rand.NextFloat(0.8f, 1.0f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-1.25f, 1.75f), Main.rand.NextFloat(-6f, -2f)),182, NPC.scale * Main.rand.NextFloat(0.9f, 1.1f));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), new Vector2(Main.rand.NextFloat(-0.85f, 1f), Main.rand.NextFloat(-5f, -1.5f)),183, NPC.scale * Main.rand.NextFloat(0.85f, 1.05f));
                
            }
        }
        private Attack CurrentAttack = Attack.CoolDown;
        private int CoolTime = 120;
        private int DashDurationTime = 120;

        private bool LockDir = false;
        private Vector2 LockDirection;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 11;
            NPC.frame.Y = 1;
        }
        public override void SetDefaults()
        {
            NPC.width = 76;
            NPC.height = 50;
            NPC.scale = 1.2f;
            NPC.damage = 15;
            NPC.lifeMax = 100;
            NPC.defense = 5;
            NPC.knockBackResist = 1f;
            NPC.friendly = false;
            NPC.noTileCollide = false;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit6;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
        private bool HasStepInFront()
        {
            int direction = NPC.direction;
            float checkX = NPC.Center.X + direction * (NPC.width / 2f + 4f);

            // 从脚底往上数 3 格（含脚那一格），看连续 solid 有几格
            int wallHeight = 0;
            for (int yOffset = 0; yOffset < 3; yOffset++)
            {
                Vector2 checkPos = new Vector2(checkX, NPC.Bottom.Y - 2 - yOffset * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    wallHeight++;
                else
                    break; // 遇到空气就停，只统计"连续"的
            }

            // ≥ 2 格高才算墙，需要跳；1 格高当台阶，直接走过去
            return wallHeight >= 1;
        }
        private bool HasWallInFront()
        {
            int direction = NPC.direction;
            float checkX = NPC.Center.X + direction * (NPC.width / 2f + 4f);

            // 从脚底往上数 3 格（含脚那一格），看连续 solid 有几格
            int wallHeight = 0;
            for (int yOffset = 0; yOffset < 3; yOffset++)
            {
                Vector2 checkPos = new Vector2(checkX, NPC.Bottom.Y - 2 - yOffset * 16f);
                Point tilePos = checkPos.ToTileCoordinates();
                Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    wallHeight++;
                else
                    break; // 遇到空气就停，只统计"连续"的
            }

            // ≥ 2 格高才算墙，需要跳；1 格高当台阶，直接走过去
            return wallHeight >= 2;
        }

        public override void AI()
        {
            int targetID = NPC.FindClosestPlayer();
            if (targetID == -1)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }
            Player target = Main.player[targetID];
            float distance = Vector2.Distance(NPC.Center, target.Center);
            switch (CurrentAttack)
            {
                case (Attack.CoolDown):
                    DoCoolDown();
                    break;
                case (Attack.Dash):
                    DoDash(target);
                    break;
            }
        }

        private void DoCoolDown()
        {
            if(CoolTime > 0)
            {
                CoolTime--;
                NPC.velocity.X *= 0;
            }
            else
            {
                CurrentAttack = Attack.Dash;
                CoolTime = 120;
                return;
            }
        }
        private void DoDash(Player target)
        {
            if (DashDurationTime > 0)
            {
                DashDurationTime--;
            }
            else
            {
                CurrentAttack = Attack.CoolDown;
                DashDurationTime = 120;
                LockDir = false;
                return;
            }
            // 检测脚下 Tile
            bool playerBelow = target.Center.Y > NPC.Center.Y + NPC.height;
            Point tileBelow = NPC.Bottom.ToTileCoordinates();
            Tile belowTile = Framing.GetTileSafely(tileBelow.X, tileBelow.Y);
            bool standingOnPlatform = belowTile.HasTile && TileID.Sets.Platforms[belowTile.TileType];
            if (standingOnPlatform && playerBelow)
            {
                NPC.noTileCollide = true;
                NPC.velocity.Y = 4f; // 向下掉落速度（可调）
            }
            else
            {
                NPC.noTileCollide = false;
            }
            //-------------------------------------------------------------------------------------------------------------------------------------------------
            if (!LockDir)
            {
                int DirInt = NPC.Center.X > target.Center.X ? -1 : 1;
                LockDirection = new Vector2(DirInt, NPC.velocity.Y);
                LockDir = true;
            }
            LockDirection.Normalize();

            float moveSpeed = 6f;
            
            NPC.velocity.X = LockDirection.X * moveSpeed;

            if (HasWallInFront() && NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = -9f;
            }
            if (HasStepInFront() && NPC.velocity.Y == 0)
            {
                NPC.velocity.Y = -4f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            switch (CurrentAttack)
            {
                case Attack.Dash:
                    if(NPC.velocity.Y > 0)
                    {
                        
                    }
                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 6) // 每10帧切换一次
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 8) // 超过第3帧时回到第1帧
                        {
                            NPC.frame.Y = 1;
                        }
                    }
                    break;
                case Attack.CoolDown:
                    int targetID = NPC.FindClosestPlayer();
                    if (targetID == -1)
                    {
                        NPC.velocity.X *= 0.9f;
                        return;
                    }
                    Player target = Main.player[targetID];
                    NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
                    NPC.spriteDirection = NPC.direction;
                    NPC.frame.Y = 0;
                    break;

            }
        }
    }
}
