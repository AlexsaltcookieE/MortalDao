using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MortalDao.Content.NPCs.BOSS.GeneralSoul;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj // 确保命名空间正确
{
    public class Ghost_Restriction_Ring : ModProjectile
    {
        // 限制圈的半径（像素）
        public const int RestrictionRadius = 880;
        private bool appear = false;
        public override void SetDefaults()
        {
            Projectile.width = RestrictionRadius * 2; // 碰撞箱设为全屏覆盖以便检测
            Projectile.height = RestrictionRadius * 2;
            Projectile.hostile = false; // 不直接碰撞伤害
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10; // 会被Boss不断刷新，所以时间很短
            Projectile.alpha = 255; // 透明
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            if (!appear)
            {
                Projectile.alpha -= 5;
                if(Projectile.alpha == 125)
                {
                    appear = true;
                }
            }
            NPC boss = Main.npc[(int)Projectile.ai[0]];//寻找Boss
            if (!boss.active || boss.type != ModContent.NPCType<GeneralSoul>())
            {
                Projectile.Kill(); // 如果Boss死了或不存在，销毁自己
                return;
            }
            Projectile.Center = boss.Center;
            int bossIndex = (int)Projectile.ai[0];
            NPC npc = Main.npc[bossIndex];
            // 转换为具体类
            if (npc.active && npc.type == ModContent.NPCType<GeneralSoul>())
            {
                GeneralSoul myBoss = npc.ModNPC as GeneralSoul;
                if (myBoss != null)
                {
                    if (myBoss.CurrentBossState != GeneralSoul.BossState.Idle)
                    {
                        Projectile.timeLeft = 10; // 在钩子状态持续存在
                    }
                    else
                    {
                        if (appear)
                        {
                            Projectile.alpha += 5;
                            if (Projectile.alpha == 255)
                            {
                                Projectile.timeLeft = 0; // 其他状态下销毁
                            }
                        }
                    }
                }
            }
            Player player = Main.player[Player.FindClosest(Projectile.Center, 0, 0)];
            if (player != null && player.active && !player.dead)
            {
                float dist = Vector2.Distance(player.Center, Projectile.Center);
                if (dist > RestrictionRadius)
                {
                    player.immuneTime = 0;
                    player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(Projectile.whoAmI, 20), 50, 0);
                    for (int i = 0; i < 3; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, DustID.GreenBlood, 0, 0, 100, default, 1.5f);
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 1. 加载圆形纹理（请确保你的项目里有这个路径的图片）
            Texture2D texture = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/BossProj/GeneralSoulProj/Ghost_Restriction_Ring").Value;
            // 2. 计算绘制位置和缩放
            // 假设你的图片原始大小是 100x100，你想把它拉伸到 RestrictionRadius*2 的大小
            Vector2 position = Projectile.Center - Main.screenPosition;
            // 计算缩放比例：目标直径 / 图片直径
            // 如果你的图片本身就是刚好覆盖整个碰撞箱的，也可以直接用 Projectile.scale
            float scale = (RestrictionRadius * 2) / (float)texture.Width;
            // 3. 绘制
            // 注意：这里用 Projectile.Center 作为原点，所以 origin 要设为图片中心
            Color color = Color.Green * (1f - Projectile.alpha / 255f);
            Main.spriteBatch.Draw(texture, position, null,color, 0f, texture.Size() / 2, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
