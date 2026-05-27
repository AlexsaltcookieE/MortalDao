using Microsoft.Xna.Framework;
using MortalDao.Content.NPCs.BOSS.GeneralSoul;
using MortalDao.Content.Projectiles.BossProj.GeneralSoulProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs
{
    public class Ghost_Hooked : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<Ghost_HookedPlayer>().HasGhost_Hooked = true;
        }
    }
    public class Ghost_HookedPlayer : ModPlayer
    {
        public bool HasGhost_Hooked;
        private Vector2 _lockedPosition;
        private bool Stuggled;
        public int KeySpaceCount = 0;
        public int KeyACount = 0;
        public int KeyDCount = 0;
        private int NeedSpace;
        private int NeedA;
        private int NeedD;
        private bool Randed;
        private int MaxKeyNum = 21;
        private int HookTimer = 5;
        private bool EXPLOSION = false;
        private bool LockWarning = false;
        private bool WarningKey = false;

        public override void ResetEffects()
        {
            HasGhost_Hooked = false;
        }
        public override void PreUpdate()
        {
            if (HasGhost_Hooked)
            {
                // 记录当前帧开始时的位置，准备锁定
                // 或者如果在PostUpdate里已经锁定了，这里就沿用那个位置
                if (_lockedPosition == Vector2.Zero)
                {
                    _lockedPosition = Player.position;
                }
            }
            else
            {
                _lockedPosition = Vector2.Zero;
            }
        }
        public override void PostUpdate()
        {
            if (HasGhost_Hooked)
            {
                if (LockWarning)
                {
                    if (!WarningKey)
                    {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), new Vector2(Player.Center.X - 190, Player.Center.Y), Vector2.Zero, ModContent.ProjectileType<AKey>(), 0, 0f, Main.myPlayer, NeedA,Player.whoAmI);
                        Projectile.NewProjectile(Player.GetSource_FromThis(), new Vector2(Player.Center.X + 200, Player.Center.Y), Vector2.Zero, ModContent.ProjectileType<DKey>(), 0, 0f, Main.myPlayer, NeedD, Player.whoAmI);
                        Projectile.NewProjectile(Player.GetSource_FromThis(), new Vector2(Player.Center.X, Player.Center.Y + 400), Vector2.Zero, ModContent.ProjectileType<SpaceKey>(), 0, 0f, Main.myPlayer, NeedSpace, Player.whoAmI);
                        WarningKey = true;
                    }
                }
                if (!EXPLOSION)
                {
                    HookTimer++;
                }
                if (_lockedPosition != Vector2.Zero)
                {
                    Player.position = _lockedPosition;
                }
                Player.velocity = Vector2.Zero;
                Player.controlHook = false;
                if (Player.mount.Active)
                {
                    Player.mount.Dismount(Player);
                }
                if (!Randed)
                {
                    NextRandNeed();
                }
                HandleQTEInput();
                CheckQTE();
            }

        }
        public override bool CanUseItem(Item item)
        {
            if (HasGhost_Hooked)
            {
                // 你可以选择允许某些特定物品，比如生命药水
                // if (item.healLife > 0) return true; 
                return false; // 禁止一切物品使用
            }
            return base.CanUseItem(item);
        }
        private void NextRandNeed()
        {
            if(!LockWarning)
            {
                LockWarning = true;
                NeedA = Main.rand.Next(0, MaxKeyNum + 1);
                NeedD = Main.rand.Next(0, MaxKeyNum + 1 - NeedA);
                NeedSpace = MaxKeyNum - (NeedA + NeedD);
                Randed = true;
                Main.NewText($"需要: Space[{NeedSpace}] A[{NeedA}] D[{NeedD}]", Color.Yellow);
            }
        }
        private void HandleQTEInput()
        {
            // 检测按下（JustPressed），防止按住连续计数
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) && !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                KeySpaceCount++;
            }
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) && !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                KeyACount++;
            }
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) && !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                KeyDCount++;
            }
        }
        private void ResetQTEState()
        {
            Stuggled = false;
            KeySpaceCount = 0;
            KeyACount = 0;
            KeyDCount = 0;
            NeedSpace = 0;
            NeedA = 0;
            NeedD = 0;
            Randed = false;
            HookTimer = 0;
            EXPLOSION = false;
            LockWarning = false;
            WarningKey = false;
        }
        private void CheckQTE()
        {
            bool bossAlive = false;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == ModContent.NPCType<GeneralSoul>())
                {
                    bossAlive = true;
                    if (HookTimer > 300 && !EXPLOSION)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromAI(),Player.position, Vector2.Zero, ModContent.ProjectileType<Ghost_Explosion>(), 10000, 0f, Main.myPlayer, Main.myPlayer);
                        EXPLOSION = true;
                    }
                    break; // 找到了就不需要继续遍历了
                }
            }
            if (!bossAlive)
            {
                Player.ClearBuff(ModContent.BuffType<Ghost_Hooked>());
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.GreenBlood, 0, 0, 100, default, 1.5f);
                }
                ResetQTEState();
                return; // 直接结束，不再执行下面的 QTE 检测
            }
            if (KeySpaceCount >= NeedSpace && KeyACount >= NeedA && KeyDCount >= NeedD)
            {
                // 成功挣脱
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<Ghost_Explosion>())
                    {
                        proj.Kill();
                    }
                }
                    Player.ClearBuff(ModContent.BuffType<Ghost_Hooked>());
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height, DustID.GreenBlood, 0, 0, 100, default, 1.5f);
                }
                ResetQTEState(); // 重置状态
            }
            else
            {
                Player.AddBuff(ModContent.BuffType<Ghost_Hooked>(), 60 * 2);
            }

        }
    }
}
