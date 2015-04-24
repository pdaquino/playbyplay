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
            this.GameFilmPlayer.DefaultPlaybackRate = 0.75;
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
            this.GameFilmPlayer.SetSource(await m_gameFilm.GetVideoStream(), m_gameFilm.VideoFile.ContentType);
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

        private void Dial_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            if (Math.Sqrt(Math.Pow(e.Position.X - (Dial.ActualWidth / 2), 2) + Math.Pow(e.Position.Y - (Dial.ActualHeight / 2), 2)) < PlayPause.ActualHeight / 2)
            {
                // If we run get a point to close to the center, ignore it and the previous remembered point
                m_havePreviousAngle = false;
            }
            else
            {
                var angle = Math.Atan2((Dial.ActualHeight / 2) - e.Position.Y, e.Position.X - (Dial.ActualWidth / 2));     
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

    }
}
