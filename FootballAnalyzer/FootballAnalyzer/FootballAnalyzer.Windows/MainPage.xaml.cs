using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FootballAnalyzer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            StartVideo();
            PrepareCanvasTriggers();
        }

        private void PrepareCanvasTriggers()
        {
            InkCanvas.PointerPressed += OnCanvasPointerPressed;
            InkCanvas.PointerMoved += OnCanvasPointerMoved;
            InkCanvas.PointerReleased += (s, e) =>
            {
                m_isDrawing = false;
                e.Handled = true;
            };
        }

        private Point m_PreviousContactPoint;
        private bool m_isDrawing = false;

        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            double d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        private void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            m_isDrawing = true;
            // Get information about the pointer location.
            PointerPoint pt = e.GetCurrentPoint(InkCanvas);
            m_PreviousContactPoint = pt.Position;

            // Accept input only from a pen or mouse with the left button pressed.
            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;
            if (pointerDevType == PointerDeviceType.Pen ||
                pointerDevType == PointerDeviceType.Mouse && pt.Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
            }
            else if (pointerDevType == PointerDeviceType.Touch)
            {
                // Process touch input
            }

        }

        private void OnCanvasPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (m_isDrawing)//e.Pointer.PointerId == m_PenId)
            {
                PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                var currentContactPt = pt.Position;
                var x1 = m_PreviousContactPoint.X;
                var y1 = m_PreviousContactPoint.Y;
                var x2 = currentContactPt.X;
                var y2 = currentContactPt.Y;

                if (CalculateDistance(x1, y1, x2, y2) > 0.5)
                {
                    //
                    // If the delta of the mouse is significant enough,
                    // we add a line geometry to the Canvas
                    Line line = new Line()
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        StrokeThickness = 4,
                        Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0))
                    };

                    m_PreviousContactPoint = currentContactPt;

                    // Draw the line on the canvas by adding the Line object as
                    // a child of the Canvas object.
                    InkCanvas.Children.Add(line);
                }
            }
            
        }

        private async void StartVideo()
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Videos");
            var file = await folder.GetFileAsync("Coaches Film_ Terrelle Pryor 93 Yard TD Run.mp4");

            var stream = await file.OpenAsync(FileAccessMode.Read);
            VideoPlayer.AreTransportControlsEnabled = true;
            VideoPlayer.SetSource(stream, file.ContentType);
            VideoPlayer.DefaultPlaybackRate = 0.75;
            VideoPlayer.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var oldChildren = InkCanvas.Children.ToList();
            foreach (var child in oldChildren)
            {
                if (child is Line)
                {
                    InkCanvas.Children.Remove(child);
                }
            }
        }

        class ValueAndTime {
            public double value;
            public DateTime time;
        }

        ValueAndTime lastValue = null;
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (lastValue == null)
            {
                lastValue = new ValueAndTime
                {
                    value = e.NewValue,
                    time = DateTime.Now
                };
            }
            else
            {
                int timeDiff = DateTime.Now.Subtract(lastValue.time).Milliseconds;
                double speed = (e.NewValue - e.OldValue) / timeDiff;
                double playbackRate = speed / 0.02;
                if (VideoPlayer != null)
                {
                    double currentPos = VideoPlayer.Position.TotalMilliseconds;
                    double deltaPos = timeDiff * speed / 0.02;
                    double newPos = currentPos + deltaPos;
                    VideoPlayer.Position = TimeSpan.FromMilliseconds(newPos);
                    System.Diagnostics.Debug.WriteLine("New pos: " + VideoPlayer.Position.ToString());
                }
            }

        }

        private double totalAngularChange = 0;
        private Point previousManipulation;
        DateTime previousManipulationTime = DateTime.MinValue;
        private void Ellipse_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var visual = Dial.TransformToVisual(this);
            Point newPoint = visual.TransformPoint(e.Position);
            if (previousManipulationTime != DateTime.MinValue)
            {
                double centerX = Canvas.GetLeft(Dial) + Dial.ActualWidth / 2;
                double centerY = Canvas.GetTop(Dial) + Dial.ActualHeight / 2;
                double c = CalculateDistance(centerX, centerY, previousManipulation.X, previousManipulation.Y);
                double b = CalculateDistance(centerX, centerY, newPoint.X, newPoint.Y);
                double a = CalculateDistance(previousManipulation.X, previousManipulation.Y, newPoint.X, newPoint.Y);
                double cos = (b * b + c * c - a * a) / (2 * b * c);
                
                //cos = Math.Min(1.0, cos);
                //cos = Math.Max(0.0, cos);
                double deltaAlpha = Math.Acos(cos);

                if (Double.IsNaN(deltaAlpha))
                {
                    int jhga = 42;
                }
                double deltaAlphaInDeg = deltaAlpha * 360 / (2 * Math.PI);
                if (deltaAlphaInDeg < 1.0)
                {
                    return;
                }
                totalAngularChange += deltaAlphaInDeg;
                VelocitiesLabel.Text = "Total = " + totalAngularChange;

                // clockwise or counter clockwise?
                double deltaXNew = newPoint.X - centerX;
                double deltaYNew = newPoint.Y - centerY;
                double deltaXOld = previousManipulation.X - centerX;
                double deltaYOld = previousManipulation.Y - centerY;

                double degNew = Math.Atan2(deltaYNew, deltaXNew);
                double degOld = Math.Atan2(deltaYOld, deltaXOld);
                bool goingCounterclockwise = false;
                if(degNew < degOld)
                {
                    VelocitiesLabel.Text += " (CC)";
                    goingCounterclockwise = true;
                    
                }
                else
                {
                    VelocitiesLabel.Text += " (C)";
                }


                //
                // control video
                // normal playback = 13s / 360 degrees
                //
                double normalRatePeriod = 6000;
                double manipulationTime = DateTime.Now.Subtract(previousManipulationTime).TotalMilliseconds;
                double videoDiffInMs = normalRatePeriod * deltaAlpha / (2 * Math.PI);
                if (goingCounterclockwise)
                {
                    videoDiffInMs *= -1;
                }
                VideoPlayer.Position = VideoPlayer.Position.Add(TimeSpan.FromMilliseconds(videoDiffInMs));
            }

            previousManipulation = newPoint;
            previousManipulationTime = DateTime.Now;
        }

        private void Dial_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            previousManipulationTime = DateTime.MinValue;
        }
    }
}
