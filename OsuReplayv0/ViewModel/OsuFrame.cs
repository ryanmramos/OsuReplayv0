using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums.Replays;
using OsuReplayv0.ViewModel.SliderPaths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsuReplayv0.ViewModel
{
    public class OsuFrame
    {
        public static Canvas Canvas { get; internal set; }
        public static int CursorDiameter { get; set; }
        public static ImageBrush CursorFill { get; set; }
        public static double HitCircleDiameter { get; set; }
        public static float CS { get; internal set; }
        public static float OD { get; internal set; }
        public static float AR { get; internal set; }
        public Vector2 CursorPosition { get; set; }
        public float Accuracy { get; set; }
        public int Score { get; set; }
        public StandardKeys KeysPressed { get; set; }
        public List<HitObject> HitObjects { get; set; }
        public int Time { get; set; }
        public float LifePercent { get; set; }
        public bool HitObjectTapRegistered { get; set; }

        private static ImageBrush hitCircleOverlay = new ImageBrush(new BitmapImage(new Uri(
                                                                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                                        "osu!",
                                                                        "Skins\\-+ Seoul v9 Personal White Cursor\\hitcircleoverlay.png"))));

        // TODO: initialize some properties above instead of setting them in MainWindowViewModel

        public void Draw()
        {
            // Clear canavs
            Canvas.Children.Clear();

            // Draw HitObjects
            for (int i = 0; i < HitObjects.Count; i++)
            {
                HitObject hitObject = HitObjects[i];
                double opacity = GetHitObjectOpacity(hitObject.StartTime);
                // TODO: alter this method to use this list of Rectangles
                //List<Rectangle> rects = new List<Rectangle>();

                if (hitObject is HitCircle)
                {
                    Rectangle hitCircle = new Rectangle
                    {
                        Width = HitCircleDiameter,
                        Height = HitCircleDiameter,
                        Fill = GetSkinElement("hitcircle"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(hitCircle, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(hitCircle, hitObject.Position.Y - HitCircleDiameter / 2);

                    Rectangle hitCircleOverlay = new Rectangle
                    {
                        Width = HitCircleDiameter, Height = HitCircleDiameter,
                        Fill = GetSkinElement("hitcircleoverlay"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(hitCircleOverlay, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(hitCircleOverlay, hitObject.Position.Y - HitCircleDiameter / 2);

                    Rectangle number = new Rectangle
                    {
                        Width = HitCircleDiameter * .6,
                        Height = HitCircleDiameter * .6,
                        Fill = GetSkinElement("default-1"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(number, hitObject.Position.X - .6 * HitCircleDiameter / 2);
                    Canvas.SetTop(number, hitObject.Position.Y - .6 * HitCircleDiameter / 2);

                    Canvas.Children.Add(hitCircle);
                    Canvas.Children.Add(hitCircleOverlay);
                    Canvas.Children.Add(number);
                }
                else if (hitObject is OsuParsers.Beatmaps.Objects.Slider)
                {
                    Rectangle sliderStartCircle = new Rectangle
                    {
                        Width = HitCircleDiameter,
                        Height = HitCircleDiameter,
                        Fill = GetSkinElement("sliderstartcircle"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(sliderStartCircle, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(sliderStartCircle, hitObject.Position.Y - HitCircleDiameter / 2);

                    Rectangle sliderStartCircleOverlay = new Rectangle
                    {
                        Width = HitCircleDiameter,
                        Height = HitCircleDiameter,
                        Fill = GetSkinElement("sliderstartcircleoverlay"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(sliderStartCircleOverlay, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(sliderStartCircleOverlay, hitObject.Position.Y - HitCircleDiameter / 2);

                    Rectangle number = new Rectangle
                    {
                        Width = HitCircleDiameter * .6,
                        Height = HitCircleDiameter * .6,
                        Fill = GetSkinElement("default-1"),
                        Opacity = opacity
                    };
                    Canvas.SetLeft(number, hitObject.Position.X - .6 * HitCircleDiameter / 2);
                    Canvas.SetTop(number, hitObject.Position.Y - .6 * HitCircleDiameter / 2);

                    Rectangle sliderb = new Rectangle
                    {
                        Width = HitCircleDiameter,
                        Height = HitCircleDiameter,
                        Fill = GetSkinElement("sliderb"),
                        Opacity = opacity
                    };

                    Canvas.Children.Add(sliderStartCircle);
                    Canvas.Children.Add(sliderStartCircleOverlay);
                    Canvas.Children.Add(number);

                    OsuParsers.Beatmaps.Objects.Slider slider = (OsuParsers.Beatmaps.Objects.Slider)hitObject;
                    // Lines that follow points of slider
                    Polyline polyline = new Polyline();
                    polyline.Stroke = Brushes.LightGreen;
                    polyline.StrokeThickness = 2;
                    polyline.Opacity = opacity;

                    // LINE
                    if (slider.CurveType is OsuParsers.Enums.Beatmaps.CurveType.Linear)
                    {
                        // Bezier curve describing slider's path
                        BezierCurve linePath = new BezierCurve(new List<Vector2> { {new Vector2(slider.Position.X, slider.Position.Y)}, 
                                                                                    { new Vector2(slider.SliderPoints.Last().X, slider.SliderPoints.Last().Y)} });

                        // Follow path
                        Vector2 initial = linePath.lerp(0);
                        polyline.Points.Add(new System.Windows.Point(initial.X, initial.Y));
                        Vector2 final = linePath.lerp(1);
                        polyline.Points.Add(new System.Windows.Point(final.X, final.Y));

                        // line 1
                        Polyline line1 = new Polyline();
                        line1.Stroke = Brushes.LightBlue;
                        line1.StrokeThickness = 1;
                        line1.Opacity = opacity;

                        // line 2
                        Polyline line2 = new Polyline();
                        line2.Stroke = Brushes.LightPink; 
                        line2.StrokeThickness = 1;
                        line2.Opacity = opacity;

                        Vector2 normalizedNormal = new Vector2(slider.SliderPoints.Last().Y - slider.Position.Y, -1 * (slider.SliderPoints.Last().X - slider.Position.X));
                        normalizedNormal = Vector2.Normalize(normalizedNormal);

                        // radius of hit circle
                        double r = HitCircleDiameter / 2;

                        // TODO: might be convenient to store these initial and final borderline points for when drawing semicirle
                        line1.Points.Add(new System.Windows.Point(initial.X + r * normalizedNormal.X, initial.Y + r * normalizedNormal.Y));
                        line1.Points.Add(new System.Windows.Point(final.X + r * normalizedNormal.X, final.Y + r * normalizedNormal.Y));

                        line2.Points.Add(new System.Windows.Point(initial.X - r * normalizedNormal.X, initial.Y - r * normalizedNormal.Y));
                        line2.Points.Add(new System.Windows.Point(final.X - r * normalizedNormal.X, final.Y - r * normalizedNormal.Y));

                        Canvas.Children.Add(line1);
                        Canvas.Children.Add(line2);
                        
                    }

                    // TODO: Bezier/CompoundBezier

                    // TODO: Circle

                    Canvas.Children.Add(polyline);
                }
                else
                {
                    Ellipse spinner = new Ellipse
                    {
                        Width = Canvas.ActualWidth / 3,
                        Height = Canvas.ActualHeight / 3,
                        Fill = Brushes.Transparent, Stroke = Brushes.Black, StrokeThickness = 2
                    };
                    Canvas.SetLeft(spinner, Canvas.ActualWidth / 2 - spinner.ActualWidth / 2);
                    Canvas.SetTop(spinner, Canvas.ActualHeight / 2 - spinner.ActualHeight / 2);

                    Canvas.Children.Add(spinner);
                }

            }

            // Draw cursor
            Rectangle cursor = new Rectangle
            {
                Width = CursorDiameter, Height = CursorDiameter,
                Fill = CursorFill,
                Stretch = Stretch.Fill
            };

            Canvas.SetLeft(cursor, CursorPosition.X - CursorDiameter / 2);
            Canvas.SetTop(cursor, CursorPosition.Y - CursorDiameter / 2);

            Canvas.Children.Add(cursor);
        }

        private double GetHitObjectOpacity(int startTime)
        {
            int preempt = calculatePreempt(AR);
            int fadeIn = calculateFadeIn(AR);
            int deltaDistance = startTime - Time;

            // full opacity condition
            if (deltaDistance <= preempt - fadeIn)
            {
                return 1.00;
            }
            else
            {
                return (preempt - deltaDistance) / (1.0 * fadeIn);
            }
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

        private int calculateFadeIn(float ar)
        {
            if (ar < 5)
            {
                return 800 + (int)(400 * (5 - ar) / 5);
            }
            else
            {
                return 800 - (int)(500 * (ar - 5) / 5);
            }
        }

        private ImageBrush GetSkinElement(string fileName)
        {
            return new ImageBrush(new BitmapImage(new Uri(
                                                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                        "osu!",
                                                        "Skins\\-+ Seoul v9 Personal White Cursor\\" + fileName + ".png"))));
        }

    }
}
