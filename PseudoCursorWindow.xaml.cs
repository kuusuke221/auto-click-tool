using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace auto_click_tool
{
    public partial class PseudoCursorWindow : Window
    {
        private DispatcherTimer _blinkTimer;
        private bool _visible = true;

        public PseudoCursorWindow()
        {
            InitializeComponent();
            // ウィンドウのクリックやフォーカスを無視して、常に背後の操作を可能にする
            this.IsHitTestVisible = false;
            this.Focusable = false;
            this.ShowActivated = false;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;

            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(600);
            _blinkTimer.Tick += BlinkTimer_Tick;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // 表示されたら点滅開始
            _blinkTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _blinkTimer.Stop();
            base.OnClosed(e);
        }

        private void BlinkTimer_Tick(object? sender, EventArgs e)
        {
            _visible = !_visible;
            ellipsePseudoCursor.Opacity = _visible ? 0.95 : 0.15;
        }

        public void MoveTo(int x, int y)
        {
            try
            {
                // Convert from physical pixels (screen coords) to WPF device-independent units (DIPs)
                // Prefer CompositionTarget.TransformFromDevice when available (handles per-monitor DPI)
                Point dipPoint;
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    // TransformFromDevice converts pixels -> DIPs
                    var transform = source.CompositionTarget.TransformFromDevice;
                    dipPoint = transform.Transform(new Point(x, y));
                }
                else
                {
                    // Fallback: use VisualTreeHelper.GetDpi
                    var dpi = VisualTreeHelper.GetDpi(this);
                    dipPoint = new Point(x / dpi.DpiScaleX, y / dpi.DpiScaleY);
                }

                // 中央に合わせる
                this.Left = dipPoint.X - (this.Width / 2);
                this.Top = dipPoint.Y - (this.Height / 2);
            }
            catch
            {
                // ignore
            }
        }
    }
}