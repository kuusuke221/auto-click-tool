using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Runtime.InteropServices;

namespace auto_click_tool
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetInitialValues();
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

        // Rキーを押すと、マウスの座標を取得して、テキストボックスに表示する
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                Point point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                txbCordinates.Text = txbCordinates.Text + point.X.ToString() + ", " + point.Y.ToString() + "\n";
            }
            else if (e.Key == Key.Escape)
            {
                AutoClicker.Stop();
            }
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