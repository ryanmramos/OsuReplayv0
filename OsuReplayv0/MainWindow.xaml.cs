using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using OsuParsers.Replays;
using OsuParsers.Decoders;
using OsuParsers.Replays.Objects;
using OsuParsers.Enums.Replays;
using System.Windows.Threading;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OsuReplayv0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private ReplayFrame[] replayFrames = new ReplayFrame[] { };
        private int currentIndex = 0;

        private int time = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Time
        {
            get { return time; }
            set
            {
                time = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            }
        }

        private double cursorLeft;

        public double CursorLeft
        {
            get { return cursorLeft; }
            set 
            { 
                cursorLeft = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CursorLeft)));
            }
        }

        private double cursorTop;

        public double CursorTop
        {
            get { return cursorTop; }
            set 
            { 
                cursorTop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CursorTop)));
            }
        }

        private SolidColorBrush rec1Fill = Brushes.White;

        public SolidColorBrush Rec1Fill
        {
            get { return rec1Fill; }
            set
            {
                rec1Fill = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rec1Fill)));
            }
        }

        private SolidColorBrush rec2Fill = Brushes.White;

        public SolidColorBrush Rec2Fill
        {
            get { return rec2Fill; }
            set
            {
                rec2Fill = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rec2Fill)));
            }
        }


        public MainWindow()
        {
            DataContext = this;
            OsrClickCommand = new RelayCommand(OnOsrClick);
            OsuClickCommand = new RelayCommand(OnOsuClick);
            InitializeComponent();
        }

        public ICommand OsrClickCommand { get; }

        public ICommand OsuClickCommand { get; }

        private void OnOsuClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".osu files | *.osu";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                string[] lines;

                try
                {
                    lines = File.ReadAllLines(path);
                }
                catch (IOException)
                {
                    throw;
                }

                foreach (string line in lines)
                {
                    Debug.WriteLine(line);
                }
            }
            else
            {
                // didnt pick anything
            }
        }

        private async void OnOsrClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".osr files| *.osr";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                Replay replay = ReplayDecoder.Decode(path);

                Debug.WriteLine(replay.PlayerName);

                replayFrames = replay.ReplayFrames.ToArray();
            }
            else
            {
                // didnt pick anything
                return;
            }
            Debug.WriteLine(replayFrames.Length);

            ReplayFrame currentFrame = replayFrames[0];
            CursorLeft = currentFrame.X;
            CursorTop = currentFrame.Y;

            for (int i = 1; i < replayFrames.Length; i++)
            {
                Time = replayFrames[i - 1].Time;

                currentFrame = replayFrames[i];

                if (currentFrame.TimeDiff < 0)
                {
                    await Task.Delay(-1 * currentFrame.TimeDiff);
                }
                else
                {
                    await Task.Delay(1 * currentFrame.TimeDiff);
                }
                CursorLeft = currentFrame.X;
                CursorTop = currentFrame.Y;
                
                // TODO: check for combination of button presses
                if (currentFrame.StandardKeys == StandardKeys.K1)
                {
                    Rec1Fill = Brushes.Gray;
                }
                else
                {   
                    Rec1Fill = Brushes.White;   
                }
                if (currentFrame.StandardKeys == StandardKeys.K2)
                {
                    Rec2Fill = Brushes.Gray;
                }
                else
                {
                    Rec2Fill = Brushes.White;
                }
            }
        }
    }
}
