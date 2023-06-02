using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums.Replays;
using OsuParsers.Replays.Objects;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;

namespace OsuReplayv0
{
    public class HitObjectTap
    {

        public HitObject HitObject { get; set; }
        public Vector2 CursorPosition { get; set; }
        public StandardKeys KeysPressed { get; set; }
        public int HitError { get; set; }
        public bool IsPressed { get; set; }

        // TODO: add more properties (listed in notes)

        public HitObjectTap(HitObject hitObj, Vector2 cursorPos, StandardKeys keys, int error, bool isPressed)
        {
            HitObject = hitObj;
            CursorPosition = cursorPos;
            KeysPressed = keys;
            HitError = error;
            IsPressed = isPressed;
        }

        public HitObjectTap(HitObject hitObj, bool isPressed)
        {
            HitObject = hitObj;
            CursorPosition = Vector2.Zero;
            KeysPressed = StandardKeys.None;
            HitError = int.MaxValue;
            IsPressed = isPressed;
        }
    }
}
