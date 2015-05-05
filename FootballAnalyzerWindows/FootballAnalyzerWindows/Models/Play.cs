using System;
using Windows.UI.Xaml.Media;
﻿using FootballAnalyzerWindows.Models;

namespace FootballAnalyzer
{
    public class Play
    {
        public ImageSource Thumbnail { get; set; }

        public string Name { get; set; }

		public TimeSpan TimeInGame { get; set; }

		public string Notes { get; set; }

        public PlayType Type { get; set; }
			
		private GameFilm m_parent;
        private GameFilm gameFilm;
        private TimeSpan time;
		public GameFilm Parent
        {
            get { return m_parent; }
        }

		public Play(GameFilm parent, TimeSpan time, PlayType type)
        {
            m_parent = parent;
            TimeInGame = time;
            Type = type;
        }

    }
}
