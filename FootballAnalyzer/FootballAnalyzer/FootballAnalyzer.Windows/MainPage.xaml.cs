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
    }
}
