using OsuParsers.Beatmaps.Objects;
using OsuParsers.Enums.Replays;
using System;
using System.Collections.Generic;
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

                if (hitObject is HitCircle)
                {
                    Ellipse hitCircle = new Ellipse
                    {
                        Width = HitCircleDiameter, Height = HitCircleDiameter,
                        Fill = GetSkinElement("hitcircleoverlay.png"),
                        Opacity = GetHitObjectOpacity(hitObject.StartTime)
                    };
                    Canvas.SetLeft(hitCircle, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(hitCircle, hitObject.Position.Y - HitCircleDiameter / 2);

                    Canvas.Children.Add(hitCircle);
                }
                else if (hitObject is OsuParsers.Beatmaps.Objects.Slider)
                {
                    Ellipse slider = new Ellipse
                    {
                        Width = HitCircleDiameter,
                        Height = HitCircleDiameter,
                        Fill = hitCircleOverlay,
                        Stroke = Brushes.Aqua, StrokeThickness = 3,
                        Opacity = GetHitObjectOpacity(hitObject.StartTime)
                    };
                    Canvas.SetLeft(slider, hitObject.Position.X - HitCircleDiameter / 2);
                    Canvas.SetTop(slider, hitObject.Position.Y - HitCircleDiameter / 2);

                    Canvas.Children.Add(slider);
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
            Ellipse cursor = new Ellipse
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
                                                        "Skins\\-+ Seoul v9 Personal White Cursor\\" + fileName))));
        }

    }
}
