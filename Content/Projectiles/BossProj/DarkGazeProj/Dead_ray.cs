using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.DarkGazeProj
{
    public class Dead_ray : ModProjectile
    {
        float LaserLength = 500;
        float MaxLaserLength = 5000f;
        float LaserHitboxwidth = 32;
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000; //绘制距离
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;//不穿墙 但设置false
            Projectile.timeLeft = 600;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;//无限穿过
            Projectile.ignoreWater = true;
            base.SetDefaults();
        }
        public override bool ShouldUpdatePosition()//是否速度取决位置
        {
            return false;
        }
        public override void AI()
        {
            LaserLength = MaxLaserLength;
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
                Projectile.velocity = owner.rotation.ToRotationVector2();
            }
            if (Projectile.ai[1] == 2f)//laserWall upward
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }

                NPC owner = Main.npc[npcIndex];
                Projectile.Center = owner.Center;
                Projectile.velocity = -Vector2.UnitY;
            }
            if (Projectile.ai[1] == 3f)//laserWall downward
            {
                int npcIndex = (int)Projectile.ai[0];
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
                {
                    Projectile.Kill();
                    return;
                }
                NPC owner = Main.npc[npcIndex];
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.UnitY;
            }
            if(Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = Vector2.UnitX;
            }
            if (Projectile.localAI[0] < 10 && Projectile.timeLeft > 25)
            {
                Projectile.localAI[0]++;
            }
            else if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
            }
        }
        public override bool? CanDamage()
        {
            return Projectile.localAI[0] >= 8 ? null : false;
        }
        public override bool? Colliding(Rectangle projHitbox,Rectangle targetHitbox)
        {
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 LaserStart = Projectile.Center;
            Vector2 laserEnd = LaserStart + unit * LaserLength;
            float HitboxWidth = LaserHitboxwidth * (Projectile.localAI[0] / 10f);
            float CollisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), LaserStart, laserEnd, HitboxWidth, ref CollisionPoint);
            
        }
        public override bool PreDraw(ref Color LightColor)
        {
            //HEAD
            int Length = (int)LaserLength;
            Color color1 = Color.White;//原色
            color1.A = 0;
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float scaleY = Projectile.localAI[0] / 10f;
            float rotation = unit.ToRotation();
            Vector2 start = Projectile.Center - Main.screenPosition;

            Texture2D head = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/BossProj/DarkGazeProj/Dead_ray_head").Value;
            Main.EntitySpriteDraw(head, start, null, color1, rotation, new Vector2(0, head.Height / 2), new Vector2(1f, scaleY), SpriteEffects.None, 0);

            //MIDDLE
            Texture2D middle = TextureAssets.Projectile[Type].Value;
            for (float i = head.Width; i < Length - middle.Width; i += middle.Width)
            {
                Vector2 bodyPos = start + unit * i;
                Main.EntitySpriteDraw(middle, bodyPos, null, color1, rotation, new Vector2(0, middle.Height / 2f), new Vector2(1f, scaleY), SpriteEffects.None, 0);
            }

            //TAILS
            Texture2D tail= ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/BossProj/DarkGazeProj/Dead_ray_tail").Value;
            Vector2 tailPos = start + unit * (Length - 8f);
            Main.EntitySpriteDraw(tail, tailPos, null, color1, rotation, new Vector2(0, tail.Height / 2f), new Vector2(1f, scaleY), SpriteEffects.None, 0);
            return false;
        }
    }
}
