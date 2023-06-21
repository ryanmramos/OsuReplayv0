using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums.Replays;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;

namespace OsuReplayv0.ViewModel
{
    public class OsuFrame
    {

        public Vector2 CursorPosition { get; set; }
        public float Accuracy { get; set; }
        public int Score { get; set; }
        public StandardKeys KeysPressed { get; set; }
        public List<HitObject> HitObjects { get; set; }
        public int Time { get; set; }
        public float LifePercent { get; set; }
        public bool HitObjectTapRegistered { get; set; }
        public static int CursorDiameter { get; set; }
        public static ImageBrush CursorFill { get; set; }

        public void Draw(ref Canvas canvas)
        {
            
        }

    }
}
