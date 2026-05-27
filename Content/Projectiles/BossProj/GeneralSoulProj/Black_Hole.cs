using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs;
using Terraria;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Black_Hole : ModProjectile
    {
        // ── 每帧动画速度（tick）：调小=更快，调大=更慢 ──
        private const int FrameSpeed = 4;
        private bool IsSuck = false;
        public override void SetStaticDefaults()
        {
            // 告诉游戏你的精灵图是 6 帧竖直排列
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.scale = 3f;
            Projectile.width = 0;     // ← 改你贴图单帧宽
            Projectile.height = 0;     // ← 改你贴图单帧高
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 6000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ignoreWater = true;
            // ai[0] 当乒乓方向标记：0 = 正向(4→5)，1 = 反向(5→4)
            Projectile.ai[0] = 0f;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 1f)
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                NPC Npc = Main.npc[npcIndex];
                //float offsetX = 0f;
                //float offsetY = -140f;
                //Vector2 ProjPos = new Vector2(Npc.Center.X + offsetX, Npc.Center.Y + offsetY);
                //Projectile.Center = ProjPos;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                // ★ 还没走到 frame 4 之前：一路 +1 往前走
                if (Projectile.frame < 4)
                {
                    Projectile.frame++;
                }
                else 
                {
                    IsSuck = true;
                    if(Projectile.frame == 5)
                    {
                        Projectile.frame = 4;
                    }
                    else if(Projectile.frame == 4)
                    {
                        Projectile.frame = 5;
                    }
                }
            }
            if (IsSuck)
            {
                Suck();
            }
            // ── 新增：引力逻辑 ─
            void Suck()
            {
                Player player = Main.LocalPlayer;
                if (player.active && !player.dead && !player.ghost && Projectile.Center != player.Center && Projectile.Distance(player.Center) < 3000)
                {
                    float dragSpeed = Projectile.Distance(player.Center) / 60;
                    player.position += Projectile.DirectionFrom(player.Center) * dragSpeed;
                    player.wingTime = 60;
                }
            }
            //buff
            Player localPlayer = Main.LocalPlayer;
            if (localPlayer.active && !localPlayer.dead && !localPlayer.ghost && Projectile.Distance(localPlayer.Center) < 3000)
            {
                // 每帧施加 Buff，持续时间设为 2 帧以确保连续
                localPlayer.AddBuff(ModContent.BuffType<Unable_Platform>(), 2);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取弹幕的纹理
            Microsoft.Xna.Framework.Graphics.Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition, // 位置调整到屏幕坐标
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );
            return false; // 返回 false 阻止默认绘制
        }
    }
}