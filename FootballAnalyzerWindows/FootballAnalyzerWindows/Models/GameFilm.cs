using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FootballAnalyzer
{
    public class GameFilm
    {
        private IRandomAccessStream m_videoStream;
        public IRandomAccessStream VideoStream
        {
            get { return m_videoStream; }
        }

        private List<Play> m_plays;
        public IList<Play> Plays 
        {
            get { return m_plays; }
        }

        public GameFilm(StorageFile videoFile)
        {
            m_videoStream = videoFile.OpenAsync(FileAccessMode.Read).GetResults();
            m_plays = new List<Play>();
        }

        public void AddPlay(TimeSpan time)
        {
            var index = m_plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index < 0) 
            {
                index = ~index;
                m_plays.Insert(index, new Play(this, time));
            }
            else
            {
                //play already exists at this point
            }
        }

        public void RemovePlay(TimeSpan time)
        {
            var index = m_plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index > 0)
            {
                m_plays.RemoveAt(index);
            }
            else
            {
                // No play exists at the specified time
            }
        }
    }
}
