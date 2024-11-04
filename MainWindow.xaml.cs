using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace auto_click_tool
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Windows APIの関数をインポート
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            SetInitialValues();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private void SetInitialValues()
        {
            txbClickInterval.Text = "1000";
            rbtnLeftClick.IsChecked = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ウィンドウにフォーカスを設定
            this.Focus();
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(13, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)0x0100)) // 0x0100: WM_KEYDOWN
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.F8)
                {
                    Point point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                    txbCordinates.Text = txbCordinates.Text + point.X.ToString() + ", " + point.Y.ToString() + "\n";
                }
                else if (key == Key.Escape)
                {
                    AutoClicker.Stop();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        // SaveボタンをクリックするとtxbCordinates.Textの内容をテキストファイルで保存する。
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ファイルパスを選択するダイアログを表示
            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };

            bool? result = sfd.ShowDialog();

            // ユーザーがファイルを選択した場合
            if (result == true)
            {
                string path = sfd.FileName;
                File.WriteAllText(path, txbCordinates.Text);
            }
        }

        // Startボタンをクリックすると、自動クリックを開始する
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // テキストボックスの内容を取得
            int interval = int.Parse(txbClickInterval.Text);
            string[] cordinates = txbCordinates.Text.Split('\n');

            // マウスのクリックボタンを取得
            MouseButton button = rbtnLeftClick.IsChecked == true ? MouseButton.Left : MouseButton.Right;

            // 自動クリックを開始
            AutoClicker.Start(interval, cordinates, button);
        }

        // interval、cordinates、buttonを引数ととして渡すと、自動クリックを開始する
        public void Start(int interval, string[] cordinates, MouseButton button)
        {
            AutoClicker.Start(interval, cordinates, button);
        }

        // Stopボタンをクリックすると、自動クリックを停止する
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            AutoClicker.Stop();
        }
    }

    public static class AutoClicker
    {
        private static Timer _timer;
        private static string[] _cordinates;
        private static MouseButton _button;
        private static int _currentIndex;

        [DllImport("user32.dll")]
        private static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void Start(int interval, string[] cordinates, MouseButton button)
        {
            _cordinates = cordinates.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            _button = button;
            _currentIndex = 0;

            _timer = new Timer(Click, null, 0, interval);
        }

        public static void Stop()
        {
            _timer?.Dispose();
        }

        private static void Click(object state)
        {
            if (_cordinates.Length == 0) return;

            var cordinate = _cordinates[_currentIndex].Split(',');
            if (cordinate.Length != 2) return;

            if (int.TryParse(cordinate[0], out int x) && int.TryParse(cordinate[1], out int y))
            {
                SetCursorPos(x, y);

                if (_button == MouseButton.Left)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                }
                else if (_button == MouseButton.Right)
                {
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
                }
            }

            _currentIndex = (_currentIndex + 1) % _cordinates.Length;
        }
    }
}