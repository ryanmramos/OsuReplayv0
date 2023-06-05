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
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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
        private SolidColorBrush cursorFill = Brushes.Black;

        [ObservableProperty]
        private SolidColorBrush hitCircleFill = Brushes.Transparent;

        [ObservableProperty]
        private string srcImage;

        [ObservableProperty]
        private double hitCircleDiameter;

        [ObservableProperty]
        private double hitCircleLeft;

        [ObservableProperty]
        private double hitCircleTop;

        public ObservableCollection<Circle> Circles { get; } = new ObservableCollection<Circle>();

        private int currObjTapIdx = 0;

        private List<HitObjectTap> objectsTapped = new List<HitObjectTap>();


        public MainWindowViewModel()
        {
            OsrClickCommand = new RelayCommand(OnOsrClick);
            OsuClickCommand = new RelayCommand(OnOsuClick);
            NextHitObjectCommand = new RelayCommand(OnNextHitObjectClick);
            BackHitObjectCommand = new RelayCommand(OnBackHitObjectClick);
        }

        public ICommand OsrClickCommand { get; }

        public ICommand OsuClickCommand { get; }

        public ICommand NextHitObjectCommand { get; }

        public ICommand BackHitObjectCommand { get; }

        private void OnOsuClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            string songsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!", "Songs");
            fileDialog.InitialDirectory = songsFolderPath;
            fileDialog.Filter = ".osu files | *.osu";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                beatmap = BeatmapDecoder.Decode(path);

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                // IMAGE
                SrcImage = path.Substring(0, path.LastIndexOf('\\')) + "\\" + beatmap.EventsSection.BackgroundImage;

                /*
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

        private void OnOsrClick()
        {
            // TODO: make to automatically disabled later (CanExecute)
            if  (beatmap == null)
            {
                Debug.WriteLine("Select corresponding beatmap first");
                return;
            }

            // Check that selected beatmap is in standard mode
            if (beatmap.GeneralSection.ModeId != 0)
            {
                Debug.WriteLine("Beatmap must be in osu!standard");
                return;
            }

            // Prompt user with file dialog to select a replay file
            OpenFileDialog fileDialog = new OpenFileDialog();
            string replaysFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!", "Replays");
            fileDialog.InitialDirectory = replaysFolderPath;
            fileDialog.Filter = ".osr files| *.osr";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                string fileName = fileDialog.SafeFileName;

                Debug.WriteLine($"Path: {path}\nFile name: {fileName}");

                // Parse replay data
                Replay replay = ReplayDecoder.Decode(path);

                replayFrames = replay.ReplayFrames.ToArray();
                osuFrames = new OsuFrame[replayFrames.Length];

                HitObject[] hitObjects = beatmap.HitObjects.ToArray();
                int nextHitObjectIdx = 0;
                LifeFrame[] lifeFrames = replay.LifeFrames.ToArray();

                // TODO: mods effect these values, change them
                // Approach rate of the beatmap
                float AR = beatmap.DifficultySection.ApproachRate;
                // Overall difficulty of the beatmap
                float OD = beatmap.DifficultySection.OverallDifficulty;
                // Circle size of the beatmap
                float CS = beatmap.DifficultySection.CircleSize;

                HitCircleDiameter = (54.4 - 4.48 * CS) * 2;

                // How much time in ms the hit object begins to fade in before its hit time
                int preempt = calculatePreempt(AR);

                int lifeFrameIdx = 0;
                int windowStartIdx = 0;
                StandardKeys currKeys = StandardKeys.None;

                bool beforeFirstHitObject = true;

                objectsTapped = new List<HitObjectTap>();

                int i = 0;
                foreach ( ReplayFrame frame in replayFrames )
                {

                    /**
                     * Check if there are any hit objects preempt ms ahead
                     *  - Check if the cursor is within the next hit object
                     *      - Check if a valid tap is made
                     *          - Add to hitobjecttap list
                     */

                    
                    if (nextHitObjectIdx >= hitObjects.Length)
                    {
                        break;
                    }
                    

                    HitObject nextHitObject = hitObjects[nextHitObjectIdx];
                    StandardKeys prevKeys = currKeys;
                    currKeys = frame.StandardKeys;

                    // Check if next hit object needs to be checked
                    int maxDelay = (200 - (int)(10 * OD)) / 2;
                    // TODO: Check for offset on the replay (actually might accounted for in replay data)
                    if (frame.Time >= nextHitObject.StartTime - preempt && frame.Time <= nextHitObject.StartTime + maxDelay)
                    {
                        // Check if Spinner
                        if (nextHitObject is Spinner && currKeys > StandardKeys.None && currKeys > StandardKeys.M2 && currKeys != StandardKeys.Smoke)
                        {
                            objectsTapped.Add(new HitObjectTap(nextHitObject,
                                              new Vector2(frame.X, frame.Y), currKeys,
                                              frame.Time - nextHitObject.StartTime, true));
                            nextHitObjectIdx++;
                            continue;
                        }
                        // Check if the cursor is within the next hit object
                        if (isCursorWithinHitObject(frame.X, frame.Y, nextHitObject.Position, CS))
                        {
                            // Check if a valid tap is made
                            // TODO: this check for a valid tap needs to be made more rigorous later
                            // (streams not quite being registered correctly because of K1 + K2)
                            if (currKeys != prevKeys && currKeys != StandardKeys.Smoke && currKeys > StandardKeys.M2)
                            {
                                objectsTapped.Add(new HitObjectTap(nextHitObject,
                                                  new Vector2(frame.X, frame.Y), currKeys,
                                                  frame.Time - nextHitObject.StartTime, true));
                                nextHitObjectIdx++;
                            }
                            else if ((replay.Mods & OsuParsers.Enums.Mods.Autoplay) > 0 && nextHitObject.StartTime == frame.Time)
                            {
                                objectsTapped.Add(new HitObjectTap(nextHitObject,
                                                  new Vector2(frame.X, frame.Y), currKeys,
                                                  frame.Time - nextHitObject.StartTime, true));
                                nextHitObjectIdx++;
                            }
                        }
                    }
                    else if (frame.Time > nextHitObject.StartTime + maxDelay)
                    {
                        objectsTapped.Add(new HitObjectTap(nextHitObject, false));
                        nextHitObjectIdx++;
                    }
                }

                DrawHitObjectTap(objectsTapped[0]);

                foreach ( HitObjectTap objTap in objectsTapped )
                {
                    continue;
                }
            }
            else
            {
                // didnt pick anything
                return;
            }
        }

        private bool isCursorWithinHitObject(float cursorX, float cursorY, Vector2 objectPosition, float cs)
        {
            float termX = (cursorX - objectPosition.X) * (cursorX - objectPosition.X);
            float termY = (cursorY - objectPosition.Y) * (cursorY - objectPosition.Y);
            double dist = Math.Sqrt(termX + termY);

            // TODO: maybe abstract some of this out such as determining circle size in osu!pix
            return dist <= (54.4 - 4.48 * cs);
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

        // TODO: generalize Next and Back ObjectClick (remove repeated code)
        private void OnNextHitObjectClick()
        {
            if (currObjTapIdx + 1 >= objectsTapped.Count)
            {
                return;
            }
            DrawHitObjectTap(objectsTapped[++currObjTapIdx]);
        }

        private void OnBackHitObjectClick()
        {
            if (currObjTapIdx == 0)
            {
                return;
            }
            DrawHitObjectTap(objectsTapped[--currObjTapIdx]);
        }

        private void DrawHitObjectTap(HitObjectTap objectTap)
        {
            HitCircleLeft = objectTap.HitObject.Position.X - HitCircleDiameter / 2;
            HitCircleTop = objectTap.HitObject.Position.Y - HitCircleDiameter / 2;

            CursorLeft = objectTap.CursorPosition.X - objectTap.HitObject.Position.X + HitCircleLeft + HitCircleDiameter / 2 - 4;
            CursorTop = objectTap.CursorPosition.Y - objectTap.HitObject.Position.Y + HitCircleTop + HitCircleDiameter / 2 - 4;

            if (objectTap.HitObject is Slider)
            {
                CursorFill = Brushes.Green;
            }
            else if (objectTap.HitObject is Spinner)
            {
                CursorFill = Brushes.Blue;
            }
            else
            {
                CursorFill = Brushes.Red;
            }

            // TODO: Slider accuracy behaves different (300 as long as within 50 window. Account for this)
            DrawHitCircleColor(objectTap.HitError);
        }

        private void DrawHitCircleColor(int hitError)
        {
            float OD = beatmap.DifficultySection.OverallDifficulty;
            int window300 = (int)Math.Round(80 - 6 * OD);
            int window100 = (int)Math.Round(140 - 8 * OD);
            int window50 = (int)Math.Round(200 - 10 * OD);

            if (hitError < 0)
            {
                hitError *= -1;
            }
            
            if (hitError < window300)
            {
                HitCircleFill = Brushes.Aqua;
            }
            else if (hitError < window100)
            {
                HitCircleFill = Brushes.LightGreen;
            }
            else if (hitError < window50)
            {
                HitCircleFill = Brushes.Purple;
            }
            else
            {
                HitCircleFill = Brushes.Red;
            }
        }
    }
}
