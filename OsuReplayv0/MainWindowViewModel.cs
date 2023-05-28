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
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Generic;

namespace OsuReplayv0
{
    public partial class MainWindowViewModel : ObservableObject
    {

        private ReplayFrame[] replayFrames = new ReplayFrame[] { };
        private OsuFrame[] osuFrames = new OsuFrame[] { };

        private Beatmap beatmap;

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

        public ObservableCollection<Circle> Circles { get; } = new ObservableCollection<Circle>();

        public MainWindowViewModel()
        {
            OsrClickCommand = new RelayCommand(OnOsrClick);
            OsuClickCommand = new RelayCommand(OnOsuClick);
        }

        public ICommand OsrClickCommand { get; }

        public ICommand OsuClickCommand { get; }

        private void OnOsuClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C:\\Users\\ryanr\\AppData\\Local\\osu!\\Songs";
            fileDialog.Filter = ".osu files | *.osu";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                beatmap = BeatmapDecoder.Decode(path);

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                /*
                // IMAGE
                SrcImage = path.Substring(0, path.LastIndexOf('\\')) + "\\" + beatmap.EventsSection.BackgroundImage;


                // AUDIO
                // TODO: need to increase scope of mediaplayer later (problem of one or more songs at same time)
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(path.Substring(0, path.LastIndexOf('\\')) + "\\" + beatmap.GeneralSection.AudioFilename));
                player.Play();
                Application.Current.Dispatcher.Invoke(() => player.Play());

                HitObject[] hitObjects = beatmap.HitObjects.ToArray();
                for (int i = 1; i < hitObjects.Length; i++)
                {
                    HitObject prevHitObject = hitObjects[i - 1];
                    HitObject currHitObject = hitObjects[i];

                    DrawHitObject(prevHitObject);

                    await Task.Delay(currHitObject.StartTime -  prevHitObject.StartTime);

                }
                */
            }
            else
            {
                // didnt pick anything
            }
        }

        private async void DrawHitObject(HitObject hitObject)
        {
            if (hitObject.GetType() != typeof(HitCircle))
            {
                return;
            }
            Circle circle = new Circle { X = hitObject.Position.X, Y = hitObject.Position.Y, Radius = 30, Fill = Brushes.Aqua };
            var uiContext = SynchronizationContext.Current;
            Circles.Add(circle);
            await Task.Delay(1000);
            Circles.Remove(circle);
        }

        private void OnOsrClick()
        {
            /*
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
            */

            // TODO: make to automatically disabled later (CanExecute)
            if  (beatmap == null)
            {
                Debug.WriteLine("Select corresponding beatmap first");
                return;
            }

            if (beatmap.GeneralSection.ModeId != 0)
            {
                Debug.WriteLine("Beatmap must be in osu!standard");
                return;
            }

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "C:\\Users\\ryanr\\AppData\\Local\\osu!\\Replays";
            fileDialog.Filter = ".osr files| *.osr";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                Replay replay = ReplayDecoder.Decode(path);

                replayFrames = replay.ReplayFrames.ToArray();
                osuFrames = new OsuFrame[replayFrames.Length];

                HitObject[] hitObjects = beatmap.HitObjects.ToArray();
                LifeFrame[] lifeFrames = replay.LifeFrames.ToArray();
                float AR = beatmap.DifficultySection.ApproachRate;

                int preempt = calculatePreempt(AR);

                int lifeFrameIdx = 0;
                int windowStartIdx = 0;

                bool beforeFirstHitObject = true;

                int i = 0;
                foreach ( ReplayFrame frame in replayFrames )
                {
                    if (beforeFirstHitObject)
                    {
                        if (frame.Time >= hitObjects[0].StartTime)
                            beforeFirstHitObject = false;
                    }

                    List<HitObject> objects = new List<HitObject>();
                    int windowStart = frame.Time - preempt;
                    int windowEnd = frame.Time + (200 - (int) (10 * beatmap.DifficultySection.OverallDifficulty)); // TODO: technically window end is when this object is clicked (which may be slightly after), if it ever is. Come back and fix
                    while ( windowStartIdx < hitObjects.Length && windowStart > hitObjects[windowStartIdx].StartTime) 
                    {
                        windowStartIdx++;
                    }
                    int localIdx = windowStartIdx;
                    while ( localIdx < hitObjects.Length && hitObjects[localIdx].StartTime < windowEnd)
                    {
                        objects.Add(hitObjects[localIdx]);
                        localIdx++;
                    }

                    LifeFrame lifeFrame = new LifeFrame();
                    while (lifeFrameIdx + 1 < lifeFrames.Length && frame.Time >= lifeFrames[lifeFrameIdx + 1].Time)
                    {
                        lifeFrameIdx++;
                    }
                    lifeFrame = lifeFrames[lifeFrameIdx];

                    HitObject nextHitObject = objects.Count > 0 ? objects[0] : new HitObject();

                    osuFrames[i] = new OsuFrame(frame, objects, lifeFrame, nextHitObject, beforeFirstHitObject);
                    i++;
                }

                List<OsuFrame> debugFrames = new List<OsuFrame>();
                foreach ( OsuFrame frame in osuFrames )
                {
                    if (!frame.IsBeforeFirstHitObject && frame.ReplayFrame.StandardKeys != 0 && frame.HitObjects.Count > 0)
                    {
                        /*
                        Debug.WriteLine($"Time: {frame.ReplayFrame.Time}\n" +
                                        $"Cursor pos.: {frame.ReplayFrame.X}, {frame.ReplayFrame.Y}\n" +
                                        $"Keys: {frame.ReplayFrame.StandardKeys}\n" +
                                        $"List of hit objects: {frame.HitObjects.Count}\n" +
                                        $"Next hit object: {frame.NextHitObject.Position}\n" +
                                        $"LifeFrame: {frame.LifeFrame.Time}, {frame.LifeFrame.Percentage}\n" +
                                        $"isBeforeFirstHitObject: {frame.IsBeforeFirstHitObject}\n");
                        */
                        debugFrames.Add(frame);
                    }
                }
                return;
            }
            else
            {
                // didnt pick anything
                return;
            }
        }

        private int calculatePreempt(float ar)
        {
            if (ar < 5)
            {
                return 1200 + (int)(600 * (5 - ar) / 5);
            }
            else
            {
                return 1200 + (int)(750 * (ar - 5) / 5);
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
