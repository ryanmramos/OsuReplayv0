using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OsuReplayv0.ViewModel.SliderPaths
{
    internal class CompositeBezierCurve
    {
        public List<BezierCurve> BezierCurves { get; set; } = new List<BezierCurve>();

        public double arcLen(int n)
        {
            double totalDist = 0;
            for (int i = 0; i < BezierCurves.Count; i++)
            {
                totalDist += BezierCurves[i].arcLen(n);
            }
            return totalDist;
        }

        // Gives the corresponding point dist away from start to end point (while traveling across curve)
        public Vector2 pointAtDist(double dist)
        {
            double arcLength = arcLen(200);
            if (dist < 0 || dist > arcLength)
            {
                return new Vector2(-1, -1); // dist cannot be above arcLength or below 0
            }

            double distToCover = dist;

            for (int i = 0; i < BezierCurves.Count; i++)
            {
                BezierCurve currCurve = BezierCurves[i];
                if (distToCover < currCurve.arcLen(50))
                {
                    return currCurve.lerp(currCurve.tValueAtDist(distToCover));
                }
                else
                {
                    distToCover -= currCurve.arcLen(50);
                }
            }

            return Vector2.Zero;
        }
    }
}
