using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.UI;

namespace MortalDao.Content.ModSetting.UI
{
    public class RealmUICharacter : UIElement
    {
        internal Realm RealmUI;
        private Player _player;//玩家
        private Asset<Texture2D> _texture;//绘制
        private bool _animated;
        private bool _drawsBackPanel;
        private float _characterScale = 1f;
        private int _animationCounter;
        public bool IsAnimated => _animated;
        public RealmUICharacter(Player player, bool animated = false, bool hasBackPanel = true, float characterScale = 1f, bool useAClone = false)
        {
            _player = player;
            if (useAClone)
            {
                _player = player.SerializedClone();
                _player.dead = false;
                _player.active = false;
                _player.whoAmI = -1;
                _player.immune = false;
                _player.immuneTime = 0;
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    _player.buffType[i] = 0;
                    _player.buffTime[i] = 0;
                }
                using var _currentPlr = new Main.CurrentPlayerOverride(_player);
                _player.PlayerFrame();
            }
            Width.Set(59f, 0f);
            Height.Set(58f, 0f);
            _texture = Main.Assets.Request<Texture2D>("Images/UI/PlayerBackground");
            UseImmediateMode = true;
            _animated = animated;
            _drawsBackPanel = hasBackPanel;
            _characterScale = characterScale;
            OverrideSamplerState = SamplerState.PointClamp;
        }
        public override void Update(GameTime gameTime)
        {
            // Override the current player reference until the end of this method.
            using var _currentPlr = new Main.CurrentPlayerOverride(_player);
            _player.UpdateMiscCounter();
            _player.UpdateDyes();
            _player.PlayerFrame();
            if (_animated)
                _animationCounter++;

            base.Update(gameTime);
        }
        private void UpdateAnim()
        {
            if (!_animated)
            {
                _player.bodyFrame.Y = _player.legFrame.Y = _player.headFrame.Y = 0;
                return;
            }
            // 原逻辑是0.07秒一帧，60帧/秒下等价于每4帧切换一次，改用私有计数器
            int frameIndex = (_animationCounter / 4) % 14 + 6;
            _player.bodyFrame.Y = _player.legFrame.Y = _player.headFrame.Y = frameIndex * 56;
            _player.WingFrame(wingFlap: false);
            RealmUI.RenderPlayerLayer();
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            using var _currentPlr = new Main.CurrentPlayerOverride(_player);
            CalculatedStyle dimensions = GetDimensions();
            UpdateAnim();
            Vector2 playerPosition = GetPlayerPosition(ref dimensions);
            int originalHeldProj = _player.heldProj;
            byte originalHideMisc = _player.hideMisc;
            _player.heldProj = -1;
            _player.hideMisc |= 1 << 2;  // 位运算避免覆盖其他隐藏设置
            Main.PlayerRenderer.DrawPlayer(Main.Camera,_player,playerPosition + Main.screenPosition,0f,Vector2.Zero,0f,_characterScale);
            _player.heldProj = originalHeldProj;
            _player.hideMisc = originalHideMisc;
        }
        private Vector2 GetPlayerPosition(ref CalculatedStyle dimensions)
        {
            Vector2 result = dimensions.Position() + new Vector2(dimensions.Width * 0.5f - (float)(_player.width >> 1), dimensions.Height * 0.5f - (float)(_player.height >> 1));
            return result;
        }
        public void SetAnimated(bool animated)
        {
            _animated = animated;
        }
    }
}
