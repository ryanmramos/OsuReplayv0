using OsuParsers.Beatmaps.Objects;
using OsuParsers.Replays.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuReplayv0
{
    public class OsuFrame
    {
        public ReplayFrame ReplayFrame { get; set; }
        public List<HitObject> HitObjects { get; set; }
        public LifeFrame LifeFrame { get; set; }
        public HitObject NextHitObject { get; set; }
        public bool IsBeforeFirstHitObject { get; set; }

        public OsuFrame(ReplayFrame rf, List<HitObject> ho, LifeFrame lf, HitObject nho, bool bfho)
        {
            ReplayFrame = rf;
            HitObjects = ho;
            LifeFrame = lf;
            NextHitObject = nho;
            IsBeforeFirstHitObject = bfho;
        }

    }
}
