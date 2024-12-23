﻿using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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

        private DispatcherTimer timerCount;
        private DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
            SetInitialValues();
            InitializeTimerCount();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private void SetInitialValues()
        {
            txbClickInterval.Text = "1000";
        }

        private void InitializeTimerCount()
        {
            // タイマーの初期設定
            timerCount = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 100ミリ秒ごとにイベント発生
            };

            timerCount.Tick += Timer_Tick;
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

        // キーボードの入力を取得して、イベントを実行
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)0x0100)) // 0x0100: WM_KEYDOWN
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.F8)
                {
                    Point point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                    txbCordinates.Text = txbCordinates.Text + point.X.ToString() + ", " + point.Y.ToString() + ", " + txbClickInterval.Text.ToString() + "\n";
                }
                else if (key == Key.Escape)
                {
                    lblMode.Content = "Edit Mode";
                    timerCount.Stop();
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

        // Openボタンをクリックするとテキストファイルを開いてtxbCordinates.Textに読み込む
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // ファイルパスを選択するダイアログを表示
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };

            bool? result = ofd.ShowDialog();

            // ユーザーがファイルを選択した場合
            if (result == true)
            {
                string path = ofd.FileName;
                txbCordinates.Text = File.ReadAllText(path);
            }
        }

        // Startボタンをクリックすると、自動クリックを開始する
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // テキストボックスの内容を取得
            string[] lines = txbCordinates.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int[] intervals = lines.Select(line => int.Parse(line.Split(',').Last())).ToArray();
            string[] cordinates = lines.Select(line => string.Join(",", line.Split(',').Take(2))).ToArray();
            lblMode.Content = "Running Mode";

            // タイマー開始
            startTime = DateTime.Now;
            timerCount.Start();

            // 自動クリックを開始
            AutoClicker.Start(intervals, cordinates);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AutoClicker.timerReset)
            {
                startTime = DateTime.Now;
                AutoClicker.timerReset = false;
            }

            TimeSpan elapsedTime = DateTime.Now - startTime;
            lblTimerCount.Content = elapsedTime.ToString(@"hh\:mm\:ss");
            //lblTimerCount.Content = "111";
        }
    }

    public static class AutoClicker
    {
        public static Timer timer;
        public static bool timerReset = false;
        private static string[] _cordinates;
        private static int _currentIndex;
        private static int[] _intervals;

        [DllImport("user32.dll")]
        private static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public static void Start(int[] intervals, string[] cordinates)
        {
            _cordinates = cordinates.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            _currentIndex = 0;
            _intervals = intervals;

            timer = new Timer(Click, null, 0, _intervals[_currentIndex]);
        }

        public static void Stop()
        {
            timer?.Dispose();
        }

        private static void Click(object state)
        {
            if (_cordinates.Length == 0) return;

            var cordinate = _cordinates[_currentIndex].Split(',');
            if (cordinate.Length != 2) return;

            if (int.TryParse(cordinate[0], out int x) && int.TryParse(cordinate[1], out int y))
            {
                SetCursorPos(x, y);

                mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            }

            _currentIndex = (_currentIndex + 1) % _cordinates.Length;

            // 次のクリックのインターバルを設定
            timer.Change(_intervals[_currentIndex], Timeout.Infinite);
            timerReset = true;
        }
    }
}