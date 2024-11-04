using System.Windows;
using System.Windows.Input;


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
            if (e.Key == Key.R)
            {
                Point point = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                txbCordinates.Text = txbCordinates.Text + point.X.ToString() + ", " + point.Y.ToString() + "\n";
            }
        }
    }

}