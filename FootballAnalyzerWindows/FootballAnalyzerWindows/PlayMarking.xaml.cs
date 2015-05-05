using FootballAnalyzer;
using FootballAnalyzerWindows.Common;
using FootballAnalyzerWindows.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace FootballAnalyzerWindows
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PlayMarking : Page
    {
        private GameFilm m_gameFilm;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private PlayType m_currentPlayType;
        DispatcherTimer m_playRefreshTimer = new DispatcherTimer();
        private SolidColorBrush playThumbnailColor = new SolidColorBrush(Colors.WhiteSmoke);
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


        public PlayMarking()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
            if (m_gameFilm == null)
            {
                throw new ArgumentException("PlayMarking expects a GameFilm object");
            }
            GameFilmPlayer.SetSource(await m_gameFilm.GetVideoStream(), "");
            GameFilmPlayer.Play();
            m_currentPlayType = PlayType.Offense;
            RefreshPlayTypeButton();

            m_playRefreshTimer.Interval = TimeSpan.FromMilliseconds(250);
            m_playRefreshTimer.Tick += m_playRefreshTimer_Tick;
            m_playRefreshTimer.Start();
            //StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Videos\Coaches Film_ Terrelle Pryor 93 Yard TD Run.mp4");
            //m_gameFilm = new GameFilm(file);
        }

        void m_playRefreshTimer_Tick(object sender, object e)
        {
            Play currentPlay = GetCurrentPlay();
            RefreshCurrentPlayInfo(currentPlay);
            RefreshRemoveButtonState(currentPlay);
        }

        private Play GetCurrentPlay()
        {
            Play currentPlay = m_gameFilm.GetPlay(GameFilmPlayer.Position);
            return currentPlay;
        }

        private void RefreshRemoveButtonState(Play currentPlay)
        {
            RemoveCurrentPlayButton.IsEnabled = (currentPlay != null);
        }

        private void RefreshPlayTypeButton()
        {
            PlayTypeButton.Content = PlayTypeName.FromPlayType(m_currentPlayType);
        }

        private void RefreshCurrentPlayInfo(Play currentPlay)
        {
            
            if(currentPlay != null)
            {
                CurrentPlayInfoText.Text = String.Format(
                    "{0} #{1}",
                    PlayTypeName.FromPlayType(currentPlay.Type),
                    m_gameFilm.GetPlayNumber(currentPlay));
                RemoveCurrentPlayButton.IsEnabled = true;
            }
            else
            {
                CurrentPlayInfoText.Text = "No current play";
            }
        }

        private void AddThumbnailButton(Play play)
        {
            Button button = PlayThumbnailButtonHelper.FromPlay(play);

            button.Click += (sender, arg) =>
            {
                var associatedPlay = (sender as Button).Tag as Play;
                this.GameFilmPlayer.Position = associatedPlay.TimeInGame;
            };

            this.PlayThumbnails.Children.Add(button);

            RefreshThumbnailButtons();
        }
        private void RefreshThumbnailButtons()
        {
            System.Diagnostics.Debug.Assert(PlayThumbnails.Children.Count == m_gameFilm.Plays.Count);
            for(int i = 0; i < PlayThumbnails.Children.Count; i++)
            {
                PlayThumbnailButtonHelper.AssociateButtonToPlay(
                    (Button)PlayThumbnails.Children[i],
                    m_gameFilm.Plays[i]);
            }
        }
        private void RemoveThumbnailButton(Play play)
        {
            // We are going to refresh every single button after removing this one,
            // so it doesn't matter which one we remove.
            this.PlayThumbnails.Children.RemoveAt(0);

            // The play numbers may have changed -- refresh them.
            RefreshThumbnailButtons();
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
        private void AddPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Play newPlay = m_gameFilm.AddPlay(GameFilmPlayer.Position, m_currentPlayType);
            System.Diagnostics.Debug.WriteLine("Marked a new play @ " + newPlay.TimeInGame);
            AddThumbnailButton(newPlay);
            RefreshCurrentPlayInfo(GetCurrentPlay());

        }

        private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            GameFilmPlayer.PlaybackRate = 2 * GameFilmPlayer.PlaybackRate;
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            GameFilmPlayer.PlaybackRate = 0.5 * GameFilmPlayer.PlaybackRate;
        }

        private void Rewind10SecsButton_Click(object sender, RoutedEventArgs e)
        {
            GameFilmPlayer.Position = GameFilmPlayer.Position.Subtract(TimeSpan.FromSeconds(7));
        }

        private void PlayTypeButton_Click(object sender, RoutedEventArgs e)
        {
            switch (m_currentPlayType)
            {
                case PlayType.Offense:
                    m_currentPlayType = PlayType.Defense;
                    break;
                case PlayType.Defense:
                    m_currentPlayType = PlayType.Offense;
                    break;
                default:
                    throw new Exception("Invalid play type");
            }
            RefreshPlayTypeButton();
        }

        private void GameFilmPlayer_SeekCompleted(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Seek completed");
        }

        private void RemoveCurrentPlayButton_Click(object sender, RoutedEventArgs e)
        {
            Play currentPlay = m_gameFilm.GetPlay(GameFilmPlayer.Position);
            if(currentPlay != null)
            {
                m_gameFilm.RemovePlay(currentPlay);
            }
            else
            {
                System.Diagnostics.Debug.Assert(currentPlay != null);
            }
            RemoveThumbnailButton(currentPlay);
        }

        private void GoToFilmReview_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(GameReviewPage), m_gameFilm);
        }
    }
}
