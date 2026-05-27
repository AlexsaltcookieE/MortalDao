using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.RangeWeaponsProj
{
    public class OverLord_Arrow : ModProjectile
    {
        private bool PlaySound = false;
        public override void SetStaticDefaults()
        {
            // 关键：告诉游戏这个弹幕贴图有8帧
            Main.projFrames[Projectile.type] = 7;
        }

        public override void SetDefaults()
        {
            if(Projectile.ai[1] == 1f)
            {
                Projectile.width = 10; // 替换为单帧的实际宽度
                Projectile.height = 10; // 替换为单帧的实际高度
                Projectile.scale = 2.5f;
            }
            else
            {
                Projectile.width = 35; // 替换为单帧的实际宽度
                Projectile.height = 49; // 替换为单帧的实际高度
                Projectile.scale = 2.5f;
            }
            Projectile.friendly = true; // 对敌友好（玩家发射）
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.penetrate = 5; // 穿透次数（1为击中一个敌人后消失）
            Projectile.timeLeft = 240; // 存在时间（帧）
            Projectile.aiStyle = -1; // 不使用预设AI，使用下方自定义AI
            Projectile.DamageType = DamageClass.Ranged; // 伤害类型（远程）
            Projectile.scale = 2.5f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 1f)
            {
                Projectile.friendly = false;
                Projectile.hostile = true;
            }
            if(!PlaySound)
            {
                PlaySound = true;
                SoundEngine.PlaySound(SoundID.Item116, Projectile.position); // 替换为你想要的声音
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            //Frame
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 7) // 8代表总帧数
                {
                    Projectile.frame = 0;
                }
            }
            //Dust
            for (int i = 0; i < 2; i++) // 每帧生成2个粒子
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.SolarFlare, // 粒子类型（可换）
                    -Projectile.velocity.X * 2,
                    -Projectile.velocity.Y * 2,
                    150,
                    default,
                    3.5f
                );
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner == null || !owner.active)
            {
                return; // 如果玩家无效，则不执行后续逻辑
            }
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectile(owner.GetSource_FromThis(),new Vector2(Projectile.Center.X,Projectile.Center.Y),Vector2.Zero,ProjectileID.SolarWhipSwordExplosion,40,0f);
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[0] == 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    NPC npc = Main.npc[Projectile.owner];
                    Projectile.NewProjectile(npc.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 40, 0f);
                }
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