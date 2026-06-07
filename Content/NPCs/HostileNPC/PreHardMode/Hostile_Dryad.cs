using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace MortalDao.Content.NPCs.HostileNPC.PreHardMode
{
    public class Hostile_Dryad : ModNPC
    {
        private int _frameIndex = 0;
        private int NPCtime;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 15;
        }
        public override void SetDefaults()
        {
            NPC.width = 22;
            NPC.height = 46;
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
            NPC.direction = target.Center.X - NPC.Center.X > 0 ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            //弹幕
            float DISTANCE = Vector2.Distance(NPC.Center, target.Center);
            if(DISTANCE < 1200f)
            {
                NPCtime++;
                if (NPCtime > 80)
                {
                    NPCtime = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 shootDirection = (target.Center - NPC.Center).RotatedByRandom(MathHelper.ToRadians(15));
                        shootDirection.Normalize();
                        shootDirection *= 6f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection, ModContent.ProjectileType<Projectiles.HostileProj.HostileLeafShot>(), 15, 0f);
                    }
                }
            }
            //帧图逻辑
            NPC.frameCounter++;
            if (NPC.frameCounter > 5)
            {
                NPC.frameCounter = 0;
                if (NPC.velocity == Vector2.Zero)
                {
                    _frameIndex = 0;
                }
                else if (Math.Abs(NPC.velocity.Y) > Math.Abs(NPC.velocity.X))
                {
                    _frameIndex = 1;
                }
                else
                {
                    _frameIndex++;
                    if (_frameIndex > 14)
                        _frameIndex = 2;
                }
                NPC.frame.Y = _frameIndex * 46;
            }
        }
    }

}
