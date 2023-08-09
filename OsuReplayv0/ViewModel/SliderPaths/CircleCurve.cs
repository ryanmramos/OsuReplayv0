using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OsuReplayv0.ViewModel.SliderPaths
{
    internal class CircleCurve
    {

        // (x - h)^2 + (y - k)^2 = r^2
        double h, k, r;

        public CircleCurve(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // Matrix computation
            // LINK: https://math.stackexchange.com/questions/213658/get-the-equation-of-a-circle-when-given-3-points

            // CONT.: continuing back here after MMAtrix is defined with what I need to use it for

        }
    }
}
