using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;
using OsuParsers.Enums.Replays;
using OsuParsers.Replays;
using OsuParsers.Replays.Objects;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace OsuReplayv0
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private ReplayFrame[] replayFrames = new ReplayFrame[] { };

        public event PropertyChangedEventHandler? PropertyChanged;

        [ObservableProperty]
        private int time = 0;

        [ObservableProperty]
        private double cursorLeft;

        [ObservableProperty]
        private double cursorTop;

        [ObservableProperty]
        private SolidColorBrush rec1Fill = Brushes.White;

        [ObservableProperty]
        private SolidColorBrush rec2Fill = Brushes.White;

        [ObservableProperty]
        private string srcImage;

        public MainWindowViewModel()
        {
            OsrClickCommand = new RelayCommand(OnOsrClick);
            OsuClickCommand = new RelayCommand(OnOsuClick);
        }

        public ICommand OsrClickCommand { get; }

        public ICommand OsuClickCommand { get; }

        private async void OnOsuClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".osu files | *.osu";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                Beatmap beatmap = BeatmapDecoder.Decode(path);

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                // IMAGE
                SrcImage = path.Substring(0, path.LastIndexOf('\\')) + "\\" + beatmap.EventsSection.BackgroundImage;


                // AUDIO
                // TODO: need to increase scope of mediaplayer later (problem of one or more songs at same time)
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(path.Substring(0, path.LastIndexOf('\\')) + "\\" + beatmap.GeneralSection.AudioFilename));
                player.Play();
                Application.Current.Dispatcher.Invoke(() => player.Play());

                HitObject[] hitObjects = beatmap.HitObjects.ToArray();
                foreach (HitObject hitObject in hitObjects)
                {
                    Debug.WriteLine(hitObject.StartTime);
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

            for (int i = 1; i < replayFrames.Length; i++)
            {
                ReplayFrame prevFrame = replayFrames[i - 1];
                Time = prevFrame.Time;
                Draw(prevFrame.X, prevFrame.Y, prevFrame.StandardKeys);

                currentFrame = replayFrames[i];

                if (currentFrame.TimeDiff < 0)
                {
                    await Task.Delay(-1 * currentFrame.TimeDiff);
                }
                else
                {
                    //await Task.Delay(1 * currentFrame.TimeDiff);
                    await Task.Delay((int)(1 * currentFrame.TimeDiff));
                }
            }
        }

        private void Draw(double x, double y, StandardKeys sKeys)
        {
            CursorLeft = x; CursorTop = y;

            // TODO: check for combination of button presses
            if (sKeys == StandardKeys.K1)
            {
                Rec1Fill = Brushes.Gray;
            }
            else
            {
                Rec1Fill = Brushes.White;
            }
            if (sKeys == StandardKeys.K2)
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
