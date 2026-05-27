using Terraria;
using Terraria.ModLoader;
using MortalDao.Content.Buffs.BossBuffs.GeneralSoulBuffs;

namespace MortalDao.Content.Projectiles.BossProj.GeneralSoulProj // 确保你的命名空间正确
{
    public class AKey : ModProjectile // 类名可以自己改
    {
        private bool initialized = false; // 是否已初始化帧位置
        private int startFrame;           // 起始帧（两帧中的第一帧）
        private bool useFirstFrame = true;// 当前是否使用第一帧
        private int animationSpeed = 5;  // 切换帧的速度（帧数）
        private int CurrentKeyACount = 0;    // 实时 KeyACount
        private bool OnKey = false;
        public override void SetStaticDefaults()
        {
            // 告诉游戏这个弹幕有 44 帧动画
            Main.projFrames[Projectile.type] = 44;
            Projectile.frameCounter = 0;
        }

        public override void SetDefaults()
        {
            // === 基础属性 ===
            Projectile.width = 16;      // 贴图宽度（根据实际贴图调整）
            Projectile.height = 16;     // 贴图高度（根据实际贴图调整）
            // === 伤害与阵营 ===
            Projectile.friendly = true; // 对玩家友好？false = 敌人发射的
            Projectile.hostile = false;  // 对敌人友好？true = 会打玩家
            Projectile.DamageType = DamageClass.Ranged; // 伤害类型
            // === 物理特性 ===
            Projectile.penetrate = 1;    // 能穿透几个敌人 (1 = 击中一个就消失)
            Projectile.timeLeft = 600;  // 存活时间 (600 = 10秒)
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // true = 撞墙会消失
            // === 外观 ===
            Projectile.alpha = 0;       // 透明度 (0 = 完全可见, 255 = 透明)
            Projectile.light = 0.5f;    // 发出的光亮度
            Projectile.scale = 2.25f;      // 贴图缩放
            // === 速度相关 ===
            //Projectile.extraUpdates = 0; // 每帧额外更新次数 (0 = 正常速度)
        }

        public override void AI()
        {
            int PlayerIndex = (int)Projectile.ai[1];
            if (PlayerIndex < 0 || PlayerIndex >= Main.maxPlayers) 
            {
                Projectile.Kill(); // 如果玩家索引无效，销毁弹幕
            }
            Player player = Main.player[PlayerIndex];
            if (player == null || !player.active)
            {
                Projectile.Kill();
            }
            else 
            {
                var ghostPlayer = player.GetModPlayer<Ghost_HookedPlayer>();
                if (!ghostPlayer.HasGhost_Hooked)
                {
                    Projectile.Kill();
                }
                if(CurrentKeyACount < ghostPlayer.KeyACount)
                {
                    CurrentKeyACount = ghostPlayer.KeyACount;
                    OnKey = true;
                }
            }
            if (!initialized)
            {
                int number = (int)Projectile.ai[0];
                if (number < 0) number = 0;
                if (number > 21) number = 21;
                startFrame = (21 - number) * 2;
                Projectile.frame = startFrame;
                useFirstFrame = true;
                initialized = true;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= animationSpeed)
            {
                Projectile.frameCounter = 0;
                if(Projectile.frame == 43 || Projectile.frame == 44)
                {
                    Projectile.Kill();
                }
                else
                {
                    Projectile.timeLeft += 2; // 每次切换帧时增加存活时间，确保弹幕不会过早消失
                }
                if (!OnKey)
                {
                    if (useFirstFrame)
                    {
                        // 切换到第二帧
                        Projectile.frame = startFrame + 1;
                    }
                    else
                    {
                        // 切换回第一帧
                        Projectile.frame = startFrame;
                    }
                    useFirstFrame = !useFirstFrame;
                }
                else if (Projectile.frame % 2 == 1)
                {
                    Projectile.frame = Projectile.frame + 1;
                    startFrame = Projectile.frame;
                    OnKey = false;
                }
                else if (Projectile.frame % 2 == 0)
                {
                    Projectile.frame = Projectile.frame  + 2;
                    startFrame = Projectile.frame;
                    OnKey = false;
                }
            }
            }
    }
}