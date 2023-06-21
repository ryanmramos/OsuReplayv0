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
                        Fill = hitCircleOverlay
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
                        Fill = Brushes.SeaShell
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

        private ImageBrush GetSkinElement(string fileName)
        {
            return new ImageBrush(new BitmapImage(new Uri(
                                                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                        "osu!",
                                                        "Skins\\-+ Seoul v9 Personal White Cursor\\" + fileName))));
        }

    }
}
