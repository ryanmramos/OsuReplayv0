using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OsuParsers.Replays;
using OsuParsers.Decoders;
using OsuParsers.Replays.Objects;
using OsuParsers.Enums.Replays;
using System.Windows.Threading;

namespace OsuReplayv0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Ellipse circle = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = Brushes.Red,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        private DispatcherTimer timer = new DispatcherTimer();
        private ReplayFrame[] replayFrames = new ReplayFrame[] { };
        private int currentIndex = 0;
        private DateTime startTime = new();

        private float time = 0;

        public float Time
        {
            get { return time; }
            set { time = value; }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            time += 1;
            tbTime.Text = time.ToString();
            //Debug.WriteLine($"Time in ms: {time}");

            if (currentIndex >= replayFrames.Length)
            {
                timer.Stop();
                return;
            }

            ReplayFrame currentFrame = replayFrames[currentIndex];

            if (currentIndex == 0)
            {
                Canvas.SetLeft(circle, currentFrame.X);
                Canvas.SetTop(circle, currentFrame.Y);
                canvas.Children.Add(circle);
                currentIndex++;
            }
            else
            {
                double duration = Math.Abs(currentFrame.Time - replayFrames[currentIndex - 1].Time); // Duration between frames
                double elapsed = (DateTime.Now - timer.Interval - startTime).TotalMilliseconds; // Elapsed time since last frame

                if (elapsed >= duration)
                {
                    Canvas.SetLeft(circle, currentFrame.X);
                    Canvas.SetTop(circle, currentFrame.Y);
                    currentIndex++;
                    startTime = DateTime.Now;
                }
                else
                {
                    // do nothing for now
                }
            }

            // check for combination of button presses
            if (currentFrame.StandardKeys == StandardKeys.K1)
            {
                recK1.Fill = Brushes.Gray;
            }
            else
            {
                recK1.Fill = Brushes.White;
            }
            if (currentFrame.StandardKeys == StandardKeys.K2)
            {
                recK2.Fill = Brushes.Gray;
            }
            else
            {
                recK2.Fill = Brushes.White;
            }
        }

        private void btnOsu_Click(object sender, RoutedEventArgs e)
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

        private void btnOsr_Click(object sender, RoutedEventArgs e)
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

                replayFrames = replay.ReplayFrames.ToArray(); ;

                //foreach (ReplayFrame rf in replay.ReplayFrames)
                //{
                //    float second = rf.Time / 1000.0f;
                //    float xPos = rf.X;
                //    float yPos = rf.Y;
                //    StandardKeys keys = rf.StandardKeys;

                //    Debug.WriteLine($"At {second}s: Position = ({xPos},{yPos}) : Keys = {keys}");
                //}
            }
            else
            {
                // didnt pick anything
            }

            time = 0;
            Debug.WriteLine(replayFrames.Length);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
    }
}
