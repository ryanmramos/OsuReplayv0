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

        public Vector2 CursorPosition { get; }
        public float Accuracy { get; }
        public int Score { get; }
        public StandardKeys KeysPressed { get; }
        public List<HitObject> HitObjects { get; }
        public int Time { get; }
        public float LifePercent { get; }
        public bool TapRegistered { get; }
        private static int CursorDiameter { get; set; }
        private static ImageBrush CursorFill { get; set; }

        public void Draw(ref Canvas canvas)
        {
            // TODO: 
        }

    }
}
