using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OsuReplayv0.ViewModel.SliderPaths
{
    internal class BezierCurve
    {
        // points defining the Bezier curve
        public List<Vector2> CurvePoints { get; set; } = new List<Vector2>();  // first and last points are the control points

        public BezierCurve(List<Vector2> points)
        {
            CurvePoints = points;
        }

        // Gives the corresponding t-value dist away from start to end point (while traveling across curve)
        public float tValueAtDist(double dist)
        {
            double arcLength = arcLen(50);

            if (dist < 0 || dist > arcLength)
            {
                return -1f; // dist cannot be above arcLength or below 0
            }

            return (float)(dist / arcLength);
        }

        // Get the arc length of Bezier curve
        // Note: method uses an approximation method. Higher values of n will provide more accurate result
        public double arcLen(int n) // TODO: adjust arcLen methods later to be accurate to .001
        {
            float delta = 1.000f / n;
            double totalDist = 0;

            Vector2 prevPoint = lerp(0);

            for (float t = delta; t < 1.0001; t += delta)
            {
                Vector2 p = lerp(t);

                totalDist += Vector2.Distance(p, prevPoint);
                prevPoint = p;
            }

            return totalDist;
        }

        public Vector2 lerp(float t)
        {
            return recursiveLerp(CurvePoints, t);
        }

        // Linear interpolation given two points (gives straight line)
        private static Vector2 linearLerp(Vector2 p1, Vector2 p2, float t)
        {
            return new Vector2(
                ((1 - t) * p1.X + t * p2.X),
                ((1 - t) * p1.Y + t * p2.Y)
            );
        }

        // Recursive linear interpolation for Quadratic, Cubic, and above Bezier curves
        private static Vector2 recursiveLerp(List<Vector2> points, float t)
        {
            if (points.Count < 2)
            {
                throw new ArgumentException("Bezier Curve must have at least 2 points defined");
            }

            if (points.Count == 2) // Base case
            {
                return linearLerp(points[0], points[1], t);
            }
            else // Recursive case
            {
                return linearLerp(
                    recursiveLerp(points.GetRange(0, points.Count - 1), t),
                    recursiveLerp(points.GetRange(1, points.Count - 1), t),
                    t
                );
            }
        }
    }
}
