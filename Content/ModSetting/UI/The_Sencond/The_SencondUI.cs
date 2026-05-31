using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace MortalDao.Content.ModSetting.UI.The_Sencond
{
    public class TheSencondUI : UIState
    {
        public override void OnActivate()
        {
            Main.blockInput = true; // 锁住世界点击（可选但强烈建议）
        }

        public override void OnDeactivate()
        {
            Main.blockInput = false;
        }
        private System.Collections.Generic.List<DialogueNode> _stack = new();
        private UIPanel _panel;
        public void PushNode(DialogueNode node)
        {
            _stack.Add(node);
            Rebuild();
        }
        public void PopNode()
        {
            if (_stack.Count > 1) _stack.RemoveAt(_stack.Count - 1);
            Rebuild();
        }
        public void Rebuild()
        {
            RemoveAllChildren();
            _panel = null;

            if (_stack.Count == 0) return;

            var node = _stack[_stack.Count - 1]; // 修正：获取栈顶节点

            // ── 半屏底框 ──────────────────────────────────────
            _panel = new UIPanel();
            // 位置：底部居中，宽度 ~800，高度 ~220
            _panel.Width.Set(800, 0f);
            _panel.Height.Set(240, 0f);
            _panel.Left.Set(-400, 0.5f);   // 水平居中
            _panel.Top.Set(-260, 1f);       // 底部往上
            Append(_panel);

            // ── 台词正文 ──────────────────────────────────────
            var text = new UIText(node.Text, 0.55f, true)
            {
                HAlign = 0.5f,
                Top = { Pixels = 18 },
                Left = { Pixels = 0 },      // 留左边给头像
                Width = { Pixels = 680 },
                Height = { Pixels = 80 },
            };
            text.Width.Set(680, 0f);
            text.Height.Set(120, 0f); // ✅ 原来是 80，容易挤爆
            text.IsWrapped = true;    // ✅ 强制换行
            _panel.Append(text);

            // ── 选项按钮 ──────────────────────────────────────
            int btnY = 110;
            foreach (var opt in node.Options)
            {
                if (opt.Condition != null && !opt.Condition()) continue;

                var btn = new UIPanel();
                btn.Width.Set(600, 0f);
                btn.Height.Set(36, 0f);
                btn.Top.Set(btnY, 0f);
                btn.Left.Set(80, 0f);
                var lbl = new UIText(opt.Label, 0.85f);
                lbl.HAlign = 0.5f;
                lbl.VAlign = 0.5f;
                btn.Append(lbl);

                btn.OnLeftClick += (_, __) =>
                {
                    opt.OnSelect?.Invoke(this);
                };

                _panel.Append(btn);
                btnY += 44;
            }
        }
        // 阻止玩家鼠标穿透到世界
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Main.player[Main.myPlayer].mouseInterface = true;
            // 如果你想完全禁用玩家移动/攻击输入：
            // Main.blockInput = true; （用完记得 Close() 时恢复）
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
        }

    }
}