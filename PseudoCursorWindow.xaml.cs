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

        /// <summary>
        /// Initializes a new instance of <see cref="PseudoCursorWindow"/>.
        /// The window is configured to be non-interactive and topmost so it can display
        /// a blinking pseudo-cursor without receiving focus.
        /// </summary>
        public PseudoCursorWindow()
        {
            InitializeComponent();
            // �E�B���h�E�̃N���b�N��t�H�[�J�X�𖳎����āA��ɔw��̑�����\�ɂ���
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
            // �\�����ꂽ��_�ŊJ�n
            _blinkTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _blinkTimer.Stop();
            base.OnClosed(e);
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            _visible = !_visible;
            ellipsePseudoCursor.Opacity = _visible ? 0.95 : 0.15;
        }

        /// <summary>
        /// Move the pseudo cursor window to the specified screen coordinates.
        /// Coordinates are provided in physical screen pixels; this method
        /// converts to WPF device-independent units (DIPs) to position the window
        /// correctly on high-DPI displays.
        /// </summary>
        /// <param name="x">X coordinate in physical pixels.</param>
        /// <param name="y">Y coordinate in physical pixels.</param>
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

                // �����ɍ��킹��
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