using FootballAnalyzerWindows.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FootballAnalyzer;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Input;
using Windows.Devices.Input;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace FootballAnalyzerWindows
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GameReviewPage : Page
    {
        private GameFilm m_gameFilm;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public GameReviewPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            this.GameFilmPlayer.DefaultPlaybackRate = 0.75;
            this.m_dialCenter = new Point(Dial.ActualWidth / 2, Dial.ActualHeight /2);

            InkCanvas.PointerReleased += (o, e) =>
            {
                m_inking = false;
                e.Handled = true;
            };
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object o, LoadStateEventArgs e)
        {
            m_gameFilm = e.NavigationParameter as GameFilm;            
            this.GameFilmPlayer.SetSource(await m_gameFilm.GetVideoStream(), m_gameFilm.VideoFile.ContentType);

            var bgColor = new SolidColorBrush(Colors.WhiteSmoke);

            foreach(var play in m_gameFilm.Plays)
            {
                var button = new Button
                {
                    Background = bgColor,
                    Content = new Image
                    {
                        Source = play.Thumbnail,
                        Height = 100,
                        Width = 150,
                    },
                    Tag = play
                };

                button.Click += (sender, arg) =>
                {
                    var associatedPlay = (sender as Button).Tag as Play;
                    this.GameFilmPlayer.Position = associatedPlay.TimeInGame;
                };

                this.PlayThumbnails.Children.Add(button);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void pageTitle_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private double m_previousAngle = 0;
        private bool m_havePreviousAngle = false;
        private Point m_dialCenter;
        private void Dial_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            if (e.Position.Distance(m_dialCenter) < PlayPause.ActualHeight / 2)
            {
                // If we run get a point to close to the center, ignore it and the previous remembered point
                m_havePreviousAngle = false;
            }
            else
            {
                var angle = Math.Atan2(m_dialCenter.Y - e.Position.Y, e.Position.X - m_dialCenter.X);     
                if (angle < 0)
                {
                    angle += (2 * Math.PI);
                }

                if (m_havePreviousAngle)
                {
                    var delta = m_previousAngle - angle;
                    
                    // Account for going around the circle completely
                    if (delta > Math.PI)
                    {
                        delta -= (Math.PI * 2);
                    }
                    else if (delta < Math.PI * -1)
                    {
                        delta += (Math.PI * 2);
                    }

                    //
                    // control video
                    // normal playback = 5s / 360 degrees
                    //
                    double videoDiffInMs = 5000 * delta / (2 * Math.PI);
                    GameFilmPlayer.Position = GameFilmPlayer.Position.Add(TimeSpan.FromMilliseconds(videoDiffInMs));
                }

                m_previousAngle = angle;
                m_havePreviousAngle = true;
            }
            e.Handled = true;
        }

        private bool m_playVideoAfterDialManipulation = false;
        private void Dial_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (this.GameFilmPlayer.CurrentState == MediaElementState.Playing)
            {
                m_playVideoAfterDialManipulation = true;
                this.GameFilmPlayer.Pause();
            }
            else
            {
                m_playVideoAfterDialManipulation = false;
            }
        }

        private void Dial_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if(m_playVideoAfterDialManipulation)
            {
                this.GameFilmPlayer.Play();
            }
            m_havePreviousAngle = false;
        }

        private void PlayPause_Pressed(object sender, PointerRoutedEventArgs e)
        {
            if (this.GameFilmPlayer.CurrentState == MediaElementState.Playing)
            {
                this.GameFilmPlayer.Pause();
            }
            else if (this.GameFilmPlayer.CurrentState == MediaElementState.Paused)
            {
                this.GameFilmPlayer.Play();
            }
        }

        private void PreviousPlay_Click(object sender, RoutedEventArgs e)
        {
            int currentPlayIndex = m_gameFilm.GetPlayNumber(GameFilmPlayer.Position);
            if (currentPlayIndex >= 0) 
            {
                Play currentPlay = m_gameFilm.Plays[currentPlayIndex];
                
                // If we are within 3 seconds of the start of a play go to the previous play, otherwise skip back to the
                // beginning of this play

                int nextPlayIndex = currentPlayIndex;
                if (GameFilmPlayer.Position.TotalSeconds - currentPlay.TimeInGame.TotalSeconds < 3)
                {
                    nextPlayIndex = Math.Max(0, currentPlayIndex - 1);
                }
                this.GameFilmPlayer.Position = m_gameFilm.Plays[nextPlayIndex].TimeInGame;
            }
        }

        private void NextPlay_Click(object sender, RoutedEventArgs e)
        {
            int currentPlayIndex = m_gameFilm.GetPlayNumber(GameFilmPlayer.Position);
            if (currentPlayIndex < m_gameFilm.Plays.Count - 1)
            {
                this.GameFilmPlayer.Position = m_gameFilm.Plays[currentPlayIndex + 1].TimeInGame;
            }
        }

        Point m_previousContactPoint;
        bool m_inking;
        private void InkCanvasPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Get information about the pointer location.
            PointerPoint pt = e.GetCurrentPoint(InkCanvas);
            m_previousContactPoint = pt.Position;

            // Accept input only from a pen or mouse with the left button pressed.
            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;
            if (pointerDevType == PointerDeviceType.Pen ||
                pointerDevType == PointerDeviceType.Mouse && pt.Properties.IsLeftButtonPressed)
            {
                m_inking = true;
                e.Handled = true;
            }
            else if (pointerDevType == PointerDeviceType.Touch)
            {
                // Process touch input
            }
        }

        private void InkCanvasPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (m_inking)
            {
                PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                if (pt.Position.Distance(m_previousContactPoint) > 1)
                {
                    //
                    // If the delta of the mouse is significant enough,
                    // we add a line geometry to the Canvas
                    Line line = new Line()
                    {
                        X1 = m_previousContactPoint.X,
                        Y1 = m_previousContactPoint.Y,
                        X2 = pt.Position.X,
                        Y2 = pt.Position.Y,
                        StrokeThickness = 10,
                        Stroke = new SolidColorBrush(Colors.Yellow)
                    };

                    m_previousContactPoint = pt.Position;

                    // Draw the line on the canvas by adding the Line object as
                    // a child of the Canvas object.
                    InkCanvas.Children.Add(line);
                }
            }            
        }

        private async void SaveDrawing_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(InkCanvas, 150, 100);

            var playNumber = m_gameFilm.GetPlayNumber(GameFilmPlayer.Position);
            if (playNumber >= 0)
            {
                m_gameFilm.Plays[playNumber].Thumbnail = renderTargetBitmap;
                var thumbnail = PlayThumbnails.Children.ToList()[playNumber] as Button;

                if (thumbnail != null)
                {
                    var image = thumbnail.Content as Image;
                    image.Source = renderTargetBitmap;
                }
            }
        }

        private void ClearDrawing_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.Children.Clear();
        }

        private void ToggleThumbnails(object sender, RoutedEventArgs e)
        {
            this.ThumbnailsScrollViewer.Visibility = ThumbnailsScrollViewer.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
