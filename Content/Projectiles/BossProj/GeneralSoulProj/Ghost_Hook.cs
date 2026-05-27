using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj
{
    public class Ghost_Hook : ModProjectile
    {
        private Vector2 HookOrigin;
        private int numChains;
        private bool HasSetOrigin = false;
        private bool HasHitPlayer = false;
        private int grabbedPlayer = -1;
        private int ProjectileTimer;
        private bool ReachMaxLength;
        private int State = 0;
        private const int maxChains = 40;
        private bool Playsound = false;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 2f;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 800;
            Projectile.aiStyle = -1;
            Projectile.hide = false;
            Projectile.knockBack = -100;
        }
        public override void AI()
        {
            Projectile.knockBack = -100;
            ProjectileTimer++;
            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs || !Main.npc[npcIndex].active)
            {
                Projectile.Kill();
                return;
            }
            NPC OwnerNpc = Main.npc[npcIndex];
            Player targetPlayer = Main.player[OwnerNpc.target];
            if (!HasSetOrigin)
            {
                HookOrigin = Projectile.Center;
                HasSetOrigin = true;
            }
            float currentLength = Vector2.Distance(Projectile.Center, HookOrigin);
            float chainSegmentLength = 10f * Projectile.scale; // 每节长度
            int currentChains = (int)(currentLength / chainSegmentLength);
            switch (State)
            {
                case 0:
                    // 伸展阶段
                    if (!Playsound)
                    {
                        SoundEngine.PlaySound(SoundID.Item18, Projectile.Center);
                        Playsound = true;
                    }
                    Vector2 ToPlayer = Vector2.Normalize(targetPlayer.Center - OwnerNpc.Center);
                    Projectile.velocity = ToPlayer * 40f;
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    //检测是否击中玩家
                    foreach (Player p in Main.player)
                    {
                        if (!p.active || p.dead) continue;
                        if (Projectile.Hitbox.Intersects(p.Hitbox))
                        {
                            if (Playsound)
                            {
                                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                                Playsound = false;
                            }
                            HasHitPlayer = true;
                            grabbedPlayer = p.whoAmI;
                            Projectile.velocity = Vector2.Zero;
                            State = 1; // 进入暂停阶段
                            ProjectileTimer = 0;
                            targetPlayer.AddBuff(ModContent.BuffType<Ghost_Hooked>(), 60 * 2);
                            break;  
                        }
                        //检测是否达到最大长度
                        if (currentChains >= maxChains)
                        {
                            Projectile.velocity = Vector2.Zero; // 停止
                            State = 1; // 进入暂停阶段
                            ProjectileTimer = 0;
                        }
                    }
                    break;

                case 1: // 暂停阶段（无论是击中了还是飞远了，都在这停顿一下）
                        if (HasHitPlayer)
                        {
                            Player p = Main.player[grabbedPlayer];
                            if (!p.active || p.dead || !p.HasBuff(ModContent.BuffType<Ghost_Hooked>()))
                            {
                                State = 2; // 进入缩回阶段
                                ProjectileTimer = 0;
                            }
                            else
                            {
                                Projectile.timeLeft = 800;
                                State = 1;
                                ProjectileTimer = 1;
                            }
                        }
                        else if (ProjectileTimer > 60) // 停顿60帧
                        {
                            State = 2; // 进入缩回阶段
                            ProjectileTimer = 0;
                        }
                        break;

                case 2: // 缩回阶段
                                // 此时 HookOrigin 应该跟随Boss，否则链子会连在空气里
                       HookOrigin = OwnerNpc.Center;

                       Vector2 ToBoss = OwnerNpc.Center - Projectile.Center;
                       float distanceToBoss = ToBoss.Length();

                       if (distanceToBoss < 30f)
                       {
                           Projectile.Kill();
                       }
                       else
                       {
                            ToBoss.Normalize();
                            Projectile.velocity = ToBoss * 15f;
                            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                       }
                       break;
                        
            }
                    
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            // 强制将击退倍数设为0，无论其他加成如何
            modifiers.Knockback *= 0;
            base.ModifyHitPlayer(target, ref modifiers);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 在这里绘制所有属于这个Boss的钩子
                    // 1. 加载贴图
                    Texture2D chainTex = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/BossProj/GeneralSoulProj/Ghost_Hook_Chain").Value;
                    Texture2D headTex = ModContent.Request<Texture2D>("MortalDao/Content/Projectiles/BossProj/GeneralSoulProj/Ghost_Hook").Value; // 确保这是20x20的头部贴图

                    // 2. 基础数据计算
                    Vector2 start = HookOrigin;
                    Vector2 end = Projectile.Center; // 这是弹幕中心，也就是我们要对接的点
                    Vector2 dir = end - start;
                    float totalLength = dir.Length();
                    float rotation = dir.ToRotation() + MathHelper.PiOver2; // 因为贴图朝上，需要+90度

                    // 3. 计算链子参数
                    float chainScale = Projectile.scale;
                    float singleChainHeight = 10f * chainScale; // 链子单节高度 (10px * 缩放)
                    Vector2 chainOrigin = new Vector2(chainTex.Width / 2f, 0f); // 链子上端中心

                    // 4. 核心改动：计算“对齐后的末端”
                    // 为了保证链子末端平整，我们取整节数，防止最后一节只画一半
                    numChains = (int)(totalLength / singleChainHeight);
                    // 如果距离太短，至少画一节
                    if (numChains == 0 && totalLength > 0) numChains = 1;

                    // 计算实际绘制的长度（对齐后的长度）
                    float actualDrawnLength = numChains * singleChainHeight;

                    // 计算链子实际绘制的终点（这才是链子真正的末端）
                    // 如果实际距离小于一节，就画到实际位置
                    Vector2 chainActualEnd = start + dir.SafeNormalize(Vector2.Zero) * actualDrawnLength;

                    // 5. 绘制链子
                    for (int i = 0; i < numChains; i++)
                    {
                        // 每一节的位置：从起点开始，沿着方向走 i * 单节高度
                        Vector2 pos = start + dir.SafeNormalize(Vector2.Zero) * (i * singleChainHeight);
                        Main.EntitySpriteDraw(
                            chainTex,
                            pos - Main.screenPosition,
                            null,
                            lightColor,
                            rotation,
                            chainOrigin,
                            chainScale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                    // 6. 绘制钩子头部（接在链子末端）
                    // 头部贴图原点：假设20x20的贴图，上端中心是连接点
                    Vector2 headOrigin = new Vector2(headTex.Width / 2f, 0f);
                    Vector2 chainLogicalEnd = start + dir.SafeNormalize(Vector2.Zero) * actualDrawnLength;
                    float overlapOffset = -10f * chainScale;
                    // 关键：钩子的位置就是链子的实际末端
                    // 因为链子的origin设在了上端(0f)，所以绘制出来的最后一节下端就在 chainActualEnd
                    // 我们把钩子的上端也放在这里，完美对接
                    //Vector2 headPos = chainActualEnd;
                    Vector2 headPos = chainLogicalEnd - dir.SafeNormalize(Vector2.Zero) * overlapOffset;
                    Main.EntitySpriteDraw(
                        headTex,
                        headPos - Main.screenPosition,
                        null,
                        lightColor,
                        rotation,
                        headOrigin, // 使用上端中心作为原点
                        chainScale,
                        SpriteEffects.None,
                        0f
                    );
                return false;
        }
    }
}
