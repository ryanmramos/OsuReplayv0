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
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OsuReplayv0.ViewModel;

namespace OsuReplayv0
{
    public partial class MainWindowViewModel : ObservableObject
    {

        private ReplayFrame[] replayFrames = new ReplayFrame[] { };
        private OsuFrame[] osuFrames = new OsuFrame[] { };

        private Beatmap beatmap;

        [ObservableProperty]
        private int time = 0;

        [ObservableProperty]
        private double cursorLeft;

        [ObservableProperty]
        private double cursorTop;

        [ObservableProperty]
        private int cursorDiameter = 80;

        [ObservableProperty]
        private ImageBrush cursorFill = new ImageBrush(new BitmapImage(new Uri(
                                                                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                                                        "osu!",
                                                                        "Skins\\-+ Seoul v9 Personal White Cursor\\cursor.png"))));

        [ObservableProperty]
        private SolidColorBrush hitCircleFill = Brushes.Transparent;

        [ObservableProperty]
        private SolidColorBrush rec1Fill = Brushes.White;

        [ObservableProperty]
        private SolidColorBrush rec2Fill = Brushes.White;

        [ObservableProperty]
        private string srcImage;

        [ObservableProperty]
        private double hitCircleDiameter;

        [ObservableProperty]
        private double hitCircleLeft;

        [ObservableProperty]
        private double hitCircleTop;

        [ObservableProperty]
        private double canvasWidth;

        [ObservableProperty]
        private double canvasHeight;

        public ObservableCollection<Circle> Circles { get; } = new ObservableCollection<Circle>();

        private int currFrameIdx = 0;

        private List<HitObjectTap> objectsTapped = new List<HitObjectTap>();

        private Canvas canvas;

        private Grid playGrid;


        public MainWindowViewModel()
        {
            OsrClickCommand = new RelayCommand(OnOsrClick);
            OsuClickCommand = new RelayCommand(OnOsuClick);
            NextHitObjectCommand = new RelayCommand(OnNextHitObjectClick);
            BackHitObjectCommand = new RelayCommand(OnBackHitObjectClick);
        }

        internal void SetCanvas(ref Canvas _canvas, ref Grid _playGrid)
        {
            canvas = _canvas;
            playGrid = _playGrid;

            // Add outside play area elements
            playGrid.Children.Add(new TextBlock() { Text = "", Foreground = new SolidColorBrush(Colors.White) });

            OsuFrame.Canvas = _canvas;
            OsuFrame.PlayGrid = _playGrid;
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
            if (beatmap == null)
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

            // OsuFrame static variables
            OsuFrame.CursorDiameter = 80;
            OsuFrame.CursorFill = CursorFill;
            OsuFrame.CS = beatmap.DifficultySection.CircleSize;
            OsuFrame.OD = beatmap.DifficultySection.OverallDifficulty;
            OsuFrame.AR = beatmap.DifficultySection.ApproachRate;

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

                HitCircleDiameter = ((54.4 - 4.48 * CS) * 2);
                OsuFrame.HitCircleDiameter = HitCircleDiameter;

                // Some algebra here (TODO: double check this, seems wrong)
                double r_prime = HitCircleDiameter / 2.0 * (-1 + Math.Sqrt((CanvasWidth * CanvasHeight) / (512 * 384)));
                HitCircleDiameter += 2 * r_prime;

                // How much time in ms the hit object begins to fade in before its hit time
                int preempt = calculatePreempt(AR);

                int lifeFrameIdx = 0;
                int windowStartIdx = 0;
                StandardKeys currKeys = StandardKeys.None;

                bool beforeFirstHitObject = true;

                objectsTapped = new List<HitObjectTap>();

                for (int i = 0; i < replayFrames.Length; i++)
                {
                    ReplayFrame frame = replayFrames[i];

                    // OsuFrame construction setup
                    StandardKeys prevKeys = currKeys;
                    currKeys = frame.StandardKeys;

                    osuFrames[i] = new OsuFrame
                    {
                        CursorPosition = new Vector2(frame.X, frame.Y),
                        Accuracy = -1f,
                        Score = -1,
                        KeysPressed = currKeys,
                        HitObjects = new List<HitObject>(),
                        Time = frame.Time,
                        LifePercent = -1f,
                        HitObjectTapRegistered = false,
                    };

                    // Get next relevant hit object
                    if (nextHitObjectIdx >= hitObjects.Length)
                    {
                        continue;
                    }
                    HitObject nextHitObject = hitObjects[nextHitObjectIdx];

                    // Check if next hit object needs to be checked
                    int maxDelay = (200 - (int)(10 * OD)) / 2;
                    if (frame.Time >= 0 && (frame.Time >= nextHitObject.StartTime - preempt && frame.Time <= nextHitObject.StartTime + maxDelay) || 
                                            (nextHitObject is OsuParsers.Beatmaps.Objects.Slider && frame.Time <= nextHitObject.EndTime))
                    {
                        osuFrames[i].HitObjects.Add(nextHitObject);
                        // TODO: get following relevant hit objects, add them to list of HitObjects for OsuFrame
                        int j = nextHitObjectIdx + 1;
                        while (j < hitObjects.Length)
                        {
                            HitObject hitObject = hitObjects[j];
                            if (frame.Time >= hitObject.StartTime - preempt && frame.Time <= hitObject.StartTime + maxDelay)
                                {
                                    osuFrames[i].HitObjects.Add(hitObject);
                                }
                            else
                            {
                                break;
                            }
                            j++;
                        }

                        // Check if Spinner
                        if (nextHitObject is Spinner && currKeys > StandardKeys.M2 && currKeys != StandardKeys.Smoke)
                        {
                            osuFrames[i].HitObjectTapRegistered = true;
                        }

                        // Check if cursor is within the next hit object
                        if (isCursorWithinHitObject(osuFrames[i].CursorPosition, nextHitObject.Position, CS))
                        {
                            // TODO: new conditional statement leads to this needing modifying for sliders
                            if (nextHitObject is OsuParsers.Beatmaps.Objects.Slider)
                            {
                                if (frame.Time <= nextHitObject.EndTime)
                                {
                                    continue;
                                }
                                else
                                {
                                    nextHitObjectIdx++;
                                }
                            }

                            // Check if valid tap is made
                            if (isValidTap(prevKeys, currKeys))
                            {
                                osuFrames[i].HitObjectTapRegistered = true;
                                nextHitObjectIdx++;
                            }
                            // Auto mod case
                            else if ((replay.Mods & OsuParsers.Enums.Mods.Autoplay) > 0 && nextHitObject.StartTime == frame.Time)
                            {
                                osuFrames[i].HitObjectTapRegistered = true;
                                nextHitObjectIdx++;
                            }
                        }
                    }
                    else if (frame.Time > nextHitObject.StartTime + maxDelay)
                    {
                        // Temporary fix
                        int j = ++nextHitObjectIdx;
                        while (j < hitObjects.Length)
                        {
                            HitObject hitObject = hitObjects[j];
                            if (frame.Time >= hitObject.StartTime - preempt && frame.Time <= hitObject.StartTime + maxDelay)
                            {
                                osuFrames[i].HitObjects.Add(hitObject);
                            }
                            else
                            {
                                break;
                            }
                            j++;
                        }
                    }

                }

                osuFrames[0].Draw();
                //DrawHitObjectTap(objectsTapped[0]);

                foreach (OsuFrame oFrame in osuFrames)
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

        // Checks to see if a valid tap was made based on the previous and current set of keys pressed
        // TODO: This will, for now, ignore mouse button inputs (M1 and M2). Figure out a way to account for mouse buttons also later
        // Problem arises from the fact that mouse button inputs are still logged when they are disabled (can this toggle be read from replay md5?)
        private bool isValidTap(StandardKeys prevKeys, StandardKeys currKeys)
        {
            // TODO: might have to reimplement this using a queue later for registering double-tapping
            if (currKeys != prevKeys && currKeys != StandardKeys.Smoke && currKeys > StandardKeys.M2)
            {
                // K1 pressed
                if ((currKeys & StandardKeys.K1) > 0)
                {
                    // Check that K1 wasn't also pressed in previous set of pressed keys
                    if ((currKeys & StandardKeys.K1) != (prevKeys & StandardKeys.K1))
                    {
                        // K1 validly pressed here
                        return true;
                    }
                }

                // K2 pressed
                if ((currKeys & StandardKeys.K2) > 0)
                {
                    // Check that K2 wasn't also pressed in previous set of pressed keys
                    if ((currKeys & StandardKeys.K2) != (prevKeys & StandardKeys.K2))
                    {
                        // K2 validly pressed here
                        return true;
                    }
                }
            }

            // Neither K1 nor K2 were validly pressed
            return false;
        }

        // Checks to see whether the cursor is within a given object
        private bool isCursorWithinHitObject(Vector2 cursorPosition, Vector2 objectPosition, float cs)
        {
            float termX = (cursorPosition.X - objectPosition.X) * (cursorPosition.X - objectPosition.X);
            float termY = (cursorPosition.Y - objectPosition.Y) * (cursorPosition.Y - objectPosition.Y);
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
                return 1200 - (int)(750 * (ar - 5) / 5);
            }
        }

        // TODO: generalize Next and Back ObjectClick (remove repeated code)
        private void OnNextHitObjectClick()
        {
            if (currFrameIdx + 1 >= osuFrames.Length)
            {
                return;
            }
            OsuFrame frame = osuFrames[++currFrameIdx];
            frame.Draw();

            // Mark key presses
            if ((frame.KeysPressed & StandardKeys.K1) > 0)
            {
                Rec1Fill = Brushes.Gray;
            }
            else
            {
                Rec1Fill = Brushes.White;
            }
            if ((frame.KeysPressed & StandardKeys.K2) > 0)
            {
                Rec2Fill = Brushes.Gray;
            }
            else
            {
                Rec2Fill = Brushes.White;
            }
        }

        private void OnBackHitObjectClick()
        {
            if (currFrameIdx == 0)
            {
                return;
            }
            OsuFrame frame = osuFrames[--currFrameIdx];
            frame.Draw();

            // Mark key presses
            if ((frame.KeysPressed & StandardKeys.K1) > 0)
            {
                Rec1Fill = Brushes.Gray;
            }
            else
            {
                Rec1Fill = Brushes.White;
            }
            if ((frame.KeysPressed & StandardKeys.K2) > 0)
            {
                Rec2Fill = Brushes.Gray;
            }
            else
            {
                Rec2Fill = Brushes.White;
            }
        }

        private void DrawHitObjectTap(HitObjectTap objectTap)
        {
            float CS = beatmap.DifficultySection.CircleSize;
            double osuPixDiameter = ((54.4 - 4.48 * CS) * 2);

            HitCircleLeft = objectTap.HitObject.Position.X - osuPixDiameter / 2;
            HitCircleTop = objectTap.HitObject.Position.Y - osuPixDiameter / 2;

            CursorLeft = objectTap.CursorPosition.X - objectTap.HitObject.Position.X + HitCircleLeft + osuPixDiameter / 2 - CursorDiameter / 2;
            CursorTop = objectTap.CursorPosition.Y - objectTap.HitObject.Position.Y + HitCircleTop + osuPixDiameter / 2 - CursorDiameter / 2;

            /*
             * Window resize helper code (not in use)
            HitCircleLeft *= CanvasWidth / 512;
            HitCircleTop *= CanvasHeight / 384;
            CursorLeft *= CanvasWidth / 512;
            CursorTop *= CanvasHeight / 384;
            */

            if (objectTap.HitObject is OsuParsers.Beatmaps.Objects.Slider)
            {
                //CursorFill = Brushes.Green;
            }
            else if (objectTap.HitObject is Spinner)
            {
                //CursorFill = Brushes.Blue;
            }
            else
            {
                //CursorFill = Brushes.Red;
            }

            if ((objectTap.KeysPressed & StandardKeys.K1) > 0)
            {
                Rec1Fill = Brushes.Gray;
            }
            else
            {
                Rec1Fill = Brushes.White;
            }
            if ((objectTap.KeysPressed & StandardKeys.K2) > 0)
            {
                Rec2Fill = Brushes.Gray;
            }
            else
            {
                Rec2Fill = Brushes.White;
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
                HitCircleFill = Brushes.CadetBlue;
            }
            else if (hitError < window100)
            {
                HitCircleFill = Brushes.Green;
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

        // TODO: won't need this, will get rid soon
        internal void WindowSizeChange(Grid playGrid)
        {
            CanvasWidth = playGrid.ActualWidth * .8;
            CanvasHeight = playGrid.ActualHeight * .8;

            HitCircleLeft *= CanvasWidth / 512;
            HitCircleTop *= CanvasHeight / 384;
            CursorLeft *= CanvasWidth / 512;
            CursorTop *= CanvasHeight / 384;
        }
    }
}