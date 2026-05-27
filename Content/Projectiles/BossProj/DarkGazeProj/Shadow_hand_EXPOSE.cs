using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.DarkGazeProj
{
    public class Shadow_hand_EXPOSE : ModProjectile
    {
        private const int FrameSpeed = 6;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 31;
        }
        public override void SetDefaults()
        {
            Projectile.damage = 20;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.friendly = false;
            Projectile.penetrate = 3; //穿透
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;//穿墙？
            Projectile.timeLeft = 31 * FrameSpeed;
            Projectile.light = 1f;
            Projectile.hostile = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            // 让该弹幕绘制在 NPC 后面，避免盖住 Shadow_hand
            behindNPCs.Add(index);
        }

        public override void OnSpawn(IEntitySource source)
        {
            // If spawned with 0 damage, force a sane default.
            if (Projectile.damage <= 0)
            {
                Projectile.damage = 20;
                Projectile.originalDamage = Projectile.damage;
            }
        }

        public override void AI()
        {
            if(Projectile.ai[1] == 1f)
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                NPC owner = Main.npc[npcIndex];
                Projectile.Center = owner.Center;
                Projectile.velocity = owner.velocity;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.Kill(); // 播放完后消失
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 弹幕消失后，让发射它的 Shadow_hand 自杀
            if (Projectile.ai[1] != 1f)
                return;

            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
                return;

            NPC owner = Main.npc[npcIndex];
            if (!owner.active)
                return;

            // 只在服务器/单机处理，避免多人客户端重复判定
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            owner.life = 0;
            owner.checkDead();
            owner.netUpdate = true;
        }
        public override bool? CanDamage()
        {
            // 前面帧是预警：到大约第27帧后才开始有伤害（0-based 的 26）
            int impactFrame = Math.Min(26, Main.projFrames[Projectile.type] - 1);
            return Projectile.frame >= impactFrame ? null : false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            // Make sprite size match hitbox size (width/height) exactly.
            Vector2 drawScale = new Vector2(
                Projectile.width / (float)frame.Width,
                Projectile.height / (float)frame.Height
            );

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, drawScale, effects);
            return false;
        }
        
    }
}