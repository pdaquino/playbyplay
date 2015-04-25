using System;
using Windows.UI.Xaml.Media;

namespace FootballAnalyzer
{
    public class Play
    {
        public ImageSource Thumbnail { get; set; }

        public string Name { get; set; }

		public TimeSpan TimeInGame { get; set; }

		public string Notes { get; set; }
			
		private GameFilm m_parent;
		public GameFilm Parent
        {
            get { return m_parent; }
        }

		public Play(GameFilm parent, TimeSpan time)
        {
            m_parent = parent;
            TimeInGame = time;
        }
    }
}
