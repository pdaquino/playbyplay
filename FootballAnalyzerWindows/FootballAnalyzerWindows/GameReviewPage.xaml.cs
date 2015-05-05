using FootballAnalyzer;
using FootballAnalyzerWindows.Common;
using System;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

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

            foreach(var play in m_gameFilm.Plays)
            {
                Button button = PlayThumbnailButtonHelper.FromPlay(play);

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

        private void DialTimeDelta(object sender, double timeDelta)
        {
            GameFilmPlayer.Position = GameFilmPlayer.Position.Add(TimeSpan.FromMilliseconds(timeDelta));
        }

        private void DialManipulationStarted(object sender, EventArgs e)
        {
            this.GameFilmPlayer.Pause();   
        }

        private void PlayPausePressed(object sender, EventArgs e)
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
                pointerDevType == PointerDeviceType.Mouse && pt.Properties.IsLeftButtonPressed ||
                pointerDevType == PointerDeviceType.Touch)
            {
                m_inking = true;
                e.Handled = true;
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
