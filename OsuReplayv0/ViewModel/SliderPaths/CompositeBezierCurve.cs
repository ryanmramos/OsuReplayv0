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

        // Constructor that formats the list of BezierCurves as expected given list of points
        public CompositeBezierCurve(List<Vector2> points)
        {
            List<Vector2> knot = new List<Vector2> { points[0] };
            Vector2 prevPoint = new Vector2(-200, -200);    // throwaway point
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 currPoint = points[i];
                if (currPoint.X == prevPoint.X && currPoint.Y == prevPoint.Y)
                {
                    BezierCurves.Add(new BezierCurve(new List<Vector2>(knot)));
                    knot.Clear();
                    knot.Add(currPoint);
                }
                else
                {
                    knot.Add(currPoint);
                }
                prevPoint = currPoint;
            }
            BezierCurves.Add(new BezierCurve(new List<Vector2>(knot)));
        }

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

        // "Artificially" perform a recursive linear interpolation for values of t: [0, 1]
        public Vector2 pointAtT(float t)
        {
            double arclen = arcLen(200);
            double distTraveled = t * arclen;

            return pointAtDist(distTraveled);
        }
    }
}
