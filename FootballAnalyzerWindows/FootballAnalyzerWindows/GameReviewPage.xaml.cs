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
            this.VideoPlayer.DefaultPlaybackRate = 0.75;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            m_gameFilm = e.NavigationParameter as GameFilm;            
            this.VideoPlayer.SetSource(await m_gameFilm.GetVideoStream(), m_gameFilm.VideoFile.ContentType);
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

        private double previousAngle = -1;
        static Point center = new Point(125, 125);
        private void Dial_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Sqrt(Math.Pow(e.Position.X - center.X, 2) + Math.Pow(e.Position.Y - center.Y, 2)) < 30)
            {
                // If we run get a point to close to the center, ignore it and the previous remembered point
                previousAngle = -1;
            }
            else
            {
                var angle = Math.Atan2(center.Y - e.Position.Y, e.Position.X - center.X);     
                if (angle < 0)
                {
                    angle += (2 * Math.PI);
                }

                if (previousAngle > 0)
                {
                    var delta = previousAngle - angle;
                    
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
                    // normal playback = 10s / 360 degrees
                    //
                    double normalRatePeriod = 6000;
                    double videoDiffInMs = normalRatePeriod * delta / (2 * Math.PI);
                    VideoPlayer.Position = VideoPlayer.Position.Add(TimeSpan.FromMilliseconds(videoDiffInMs));
                }

                previousAngle = angle;
            }
            e.Handled = true;
        }

        private bool m_playVideoAfterDialManipulation = false;
        private void Dial_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (this.VideoPlayer.CurrentState == MediaElementState.Playing)
            {
                m_playVideoAfterDialManipulation = true;
                this.VideoPlayer.Pause();
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
                this.VideoPlayer.Play();
            }
        }

        private void PlayPause_Pressed(object sender, PointerRoutedEventArgs e)
        {
            if (this.VideoPlayer.CurrentState == MediaElementState.Playing)
            {
                this.VideoPlayer.Pause();
            }
            else if (this.VideoPlayer.CurrentState == MediaElementState.Paused)
            {
                this.VideoPlayer.Play();
            }
        }

    }
}
