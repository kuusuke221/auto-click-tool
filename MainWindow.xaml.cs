using System.Diagnostics;
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
        /// <summary>
        /// Main window for AutoClickTool. Handles user interactions and delegates
        /// business logic to services where appropriate.
        /// </summary>
        // Import Windows API functions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private DispatcherTimer timerCount;
        private DateTime startTime;
        private PseudoCursorWindow _pseudoCursor;

        // Recording mode state
        private bool isRecording = false;
        private DateTime recordStartTime;
        private DateTime lastRecordTime;

        /// <summary>
        /// Initializes a new instance of <see cref="MainWindow"/>.
        /// Sets up timers and hooks required for the UI.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetInitialValues();
            InitializeTimerCount();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            _pseudoCursor = null;
        }

        private void SetInitialValues()
        {
            txbClickInterval.Text = "1000";
        }

        private void InitializeTimerCount()
        {
            // Initialize the timer
            timerCount = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Tick every 100 milliseconds
            };

            timerCount.Tick += Timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the window
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

        // Capture keyboard input and dispatch events
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN = 0x0100;
            const int VK_CONTROL = 0x11;
            const int VK_SHIFT = 0x10;
            const int VK_C = 0x43;
            const int VK_R = 0x52;
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN)) // WM_KEYDOWN
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Handle Ctrl+Shift+C and Ctrl+Shift+R as global shortcuts for Check / Reset Check
                try
                {
                    if (vkCode == VK_C && (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0 && (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                    {
                        // Invoke Check action on UI thread
                        this.Dispatcher.Invoke(() => btnCheck_Click(null, null));
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }

                    if (vkCode == VK_R && (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0 && (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
                    {
                        this.Dispatcher.Invoke(() => btnResetCheck_Click(null, null));
                        return CallNextHookEx(_hookID, nCode, wParam, lParam);
                    }
                }
                catch
                {
                    // Ignore errors checking key state
                }

                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.F8)
                {
                    // If in recording mode, record elapsed time since last record (or since record start)
                    if (isRecording)
                    {
                        var pos = System.Windows.Forms.Cursor.Position;
                        DateTime now = DateTime.Now;
                        int elapsedMs = (int)(now - lastRecordTime).TotalMilliseconds;
                        lastRecordTime = now;

                        // Update UI on dispatcher (hook runs on a different thread)
                        this.Dispatcher.Invoke(() =>
                        {
                            txbCordinates.Text = txbCordinates.Text + pos.X.ToString() + ", " + pos.Y.ToString() + ", " + elapsedMs.ToString() + "\n";
                        });
                    }
                    else
                    {
                        // Existing behavior when not recording: use default click interval
                        var pos = System.Windows.Forms.Cursor.Position;
                        this.Dispatcher.Invoke(() =>
                        {
                            txbCordinates.Text = txbCordinates.Text + pos.X.ToString() + ", " + pos.Y.ToString() + ", " + txbClickInterval.Text.ToString() + "\n";
                        });
                    }
                }
                else if (key == Key.Escape)
                {
                    if (isRecording)
                    {
                        // Stop recording and switch to Edit Mode
                        isRecording = false;
                        this.Dispatcher.Invoke(() =>
                        {
                            lblMode.Content = "Edit Mode";
                        });
                        timerCount.Stop();
                    }
                    else
                    {
                        lblMode.Content = "Edit Mode";
                        timerCount.Stop();
                        AutoClicker.Stop();
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);

            // Ensure pseudo cursor window is closed when the main window closes
            try
            {
                if (_pseudoCursor != null)
                {
                    _pseudoCursor.Close();
                    _pseudoCursor = null;
                }
            }
            catch
            {
                // Ignore
            }

            base.OnClosed(e);
        }

        // Save button: write txbCordinates.Text to a text file
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Show a save file dialog
            Microsoft.Win32.SaveFileDialog sfd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };

            bool? result = sfd.ShowDialog();

            // If the user selected a file
            if (result == true)
            {
                string path = sfd.FileName;
                File.WriteAllText(path, txbCordinates.Text);
            }
        }

        // Open button: load a text file into txbCordinates.Text
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Show an open file dialog
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };

            bool? result = ofd.ShowDialog();

            // If the user selected a file
            if (result == true)
            {
                string path = ofd.FileName;
                txbCordinates.Text = File.ReadAllText(path);
            }
        }

        // Start button: parse coordinates and intervals, start the timer and automatic clicking
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Parse the coordinates and intervals from the text box
            string[] lines = txbCordinates.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int[] intervals = lines.Select(line => int.Parse(line.Split(',').Last())).ToArray();
            string[] cordinates = lines.Select(line => string.Join(",", line.Split(',').Take(2))).ToArray();
            lblMode.Content = "Running Mode";

            // Start the timer
            startTime = DateTime.Now;
            timerCount.Start();

            // Start automatic clicking
            AutoClicker.Start(intervals, cordinates);

        }

        // Check button: get coordinates from the caret line and show a pseudo cursor
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int caret = txbCordinates.CaretIndex;
                int lineIndex = txbCordinates.GetLineIndexFromCharacterIndex(caret);
                if (lineIndex < 0 || lineIndex >= txbCordinates.LineCount) return;

                string line = txbCordinates.GetLineText(lineIndex).Trim();
                if (string.IsNullOrWhiteSpace(line)) return;

                var rawParts = line.Split(',');
                if (rawParts.Length < 2) return;

                string sx = rawParts[0].Trim();
                string sy = rawParts[1].Trim();

                if (int.TryParse(sx, out int x) && int.TryParse(sy, out int y))
                {
                    // If a pseudo cursor already exists, move it; otherwise create and show it
                    if (_pseudoCursor == null)
                    {
                        _pseudoCursor = new PseudoCursorWindow();
                        _pseudoCursor.Show();
                    }

                    _pseudoCursor.MoveTo(x, y);
                }
            }
            catch
            {
                // Ignore
            }
        }

        // Reset Check button: remove the pseudo cursor
        private void btnResetCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pseudoCursor != null)
                {
                    _pseudoCursor.Close();
                    _pseudoCursor = null;
                }
            }
            catch
            {
                // Ignore
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isRecording)
            {
                // In record mode, show time since last record (or since recording started)
                TimeSpan elapsedSinceLast = DateTime.Now - lastRecordTime;
                // Do not display sub-second precision; show only whole seconds
                lblTimerCount.Content = elapsedSinceLast.ToString(@"hh\:mm\:ss");
                return;
            }

            if (AutoClicker.timerReset)
            {
                startTime = DateTime.Now;
                AutoClicker.timerReset = false;
            }

            TimeSpan elapsedTime = DateTime.Now - startTime;
            lblTimerCount.Content = elapsedTime.ToString(@"hh\:mm\:ss");
            //lblTimerCount.Content = "111";
        }

        // Record button: start/stop recording mode
        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                // Start
                isRecording = true;
                recordStartTime = DateTime.Now;
                lastRecordTime = recordStartTime;
                lblMode.Content = "Record Mode";
                timerCount.Start();
            }
            else
            {
                // Stop (same as ESC): switch to Edit Mode
                isRecording = false;
                lblMode.Content = "Edit Mode";
                timerCount.Stop();
            }
        }

        // Add Point button handler (same behavior as F8)
        private void btnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pos = System.Windows.Forms.Cursor.Position;
                if (isRecording)
                {
                    DateTime now = DateTime.Now;
                    int elapsedMs = (int)(now - lastRecordTime).TotalMilliseconds;
                    lastRecordTime = now;
                    txbCordinates.Text = txbCordinates.Text + pos.X.ToString() + ", " + pos.Y.ToString() + ", " + elapsedMs.ToString() + "\n";
                }
                else
                {
                    txbCordinates.Text = txbCordinates.Text + pos.X.ToString() + ", " + pos.Y.ToString() + ", " + txbClickInterval.Text.ToString() + "\n";
                }
            }
            catch
            {
                // Ignore
            }
        }

        // Stop button (or Esc) handler
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (isRecording)
            {
                // Stop recording and switch to Edit Mode
                isRecording = false;
                lblMode.Content = "Edit Mode";
                timerCount.Stop();
            }
            else
            {
                lblMode.Content = "Edit Mode";
                timerCount.Stop();
                AutoClicker.Stop();
            }
        }
    }

    public static class AutoClicker
    {
        /// <summary>
        /// Performs automatic mouse clicks for a list of coordinates with per-item intervals.
        /// This static helper manages a background <see cref="System.Threading.Timer"/> that
        /// moves the cursor and sends mouse down/up events.
        /// </summary>
        public static Timer timer;
        /// <summary>
        /// Indicates whether the timer was reset by the click loop.
        /// </summary>
        public static bool timerReset = false;

        /// <summary>
        /// Coordinates in the form of "X,Y" for each click step.
        /// </summary>
        private static string[] _cordinates;

        /// <summary>
        /// Current index in the coordinates array.
        /// </summary>
        private static int _currentIndex;

        /// <summary>
        /// Intervals (in milliseconds) for each coordinate.
        /// </summary>
        private static int[] _intervals;

        [DllImport("user32.dll")]
        private static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        /// <summary>
        /// Start automatic clicking sequence.
        /// </summary>
        /// <param name="intervals">Array of intervals in milliseconds for each click.</param>
        /// <param name="cordinates">Array of coordinates where each item is "X,Y".</param>
        public static void Start(int[] intervals, string[] cordinates)
        {
            _cordinates = cordinates.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            _currentIndex = 0;
            _intervals = intervals;

            timer = new Timer(Click, null, 0, _intervals[_currentIndex]);
        }

        /// <summary>
        /// Stop the automatic clicking sequence and dispose the timer.
        /// </summary>
        public static void Stop()
        {
            timer?.Dispose();
        }

        /// <summary>
        /// Timer callback that performs a single click at the current coordinate and advances the index.
        /// </summary>
        /// <param name="state">State object passed by the timer (unused).</param>
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

            // Reset the timer for the next click based on the interval of the next coordinate
            timer.Change(_intervals[_currentIndex], Timeout.Infinite);
            timerReset = true;
        }
    }
}