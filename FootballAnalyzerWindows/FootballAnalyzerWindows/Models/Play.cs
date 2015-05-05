using System;
using Windows.UI.Xaml.Media;
﻿using FootballAnalyzerWindows.Models;
using System.Xml.Serialization;
using System.Xml;

namespace FootballAnalyzer
{
    public class Play
    {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public ImageSource Thumbnail { get; set; }

        public string Name { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute]
		public TimeSpan TimeInGame { get; set; }

        // XmlSerializer does not support TimeSpan, so use this property for 
        // serialization instead.
        [XmlElement(DataType = "duration", ElementName = "TimeInGame")]
        public string TimeInGameString
        {
            get
            {
                return XmlConvert.ToString(TimeInGame);
            }
            set
            {
                TimeInGame = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }
		public string Notes { get; set; }

        public PlayType Type { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public GameFilm Parent { get; set; }

		public Play(GameFilm parent, TimeSpan time, PlayType type)
        {
            Parent = parent;
            TimeInGame = time;
            Type = type;
        }

        public Play()
        {

        }
    }
}
