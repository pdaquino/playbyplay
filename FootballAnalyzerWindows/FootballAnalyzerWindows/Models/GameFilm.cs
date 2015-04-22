using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using FootballAnalyzerWindows.Models;

namespace FootballAnalyzer
{
    public class GameFilm
    {
        private IRandomAccessStream m_videoStream;

        private List<Play> m_plays;
        public IList<Play> Plays 
        {
            get { return m_plays; }
        }

        private StorageFile m_videoFile;
        public StorageFile VideoFile
        {
            get { return m_videoFile; }
        }
        
        public GameFilm(StorageFile videoFile)
        {
            m_videoFile = videoFile;
            m_plays = new List<Play>();
        }

        public async Task<IRandomAccessStream> GetVideoStream()
        {
            if (m_videoStream == null)
            {
                m_videoStream = await m_videoFile.OpenAsync(FileAccessMode.Read);
            }
            return m_videoStream;
        }

        public void AddPlay(TimeSpan time, PlayType type)
        {
            var index = m_plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index < 0) 
            {
                index = ~index;
                m_plays.Insert(index, new Play(this, time, type));
            }
            else
            {
                //play already exists at this point
            }
        }

        public void RemovePlay(TimeSpan time)
        {
            var index = m_plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index >= 0)
            {
                m_plays.RemoveAt(index);
            }
            else
            {
                // No play exists at the specified time
            }
        }

        protected Play TryFindPlayAtExactTime(TimeSpan time)
        {
            var index = m_plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index >= 0)
            {
                return m_plays[index];
            }
            else
            {
                return null;
            }
        }

        public Play TryFindPlay(TimeSpan time)
        {
            List<TimeSpan> possiblePlays = m_plays.Select(i => i.TimeInGame).ToList().FindAll(x => x.CompareTo(time) <= 0);
            if (possiblePlays.Count() > 0)
            {
                possiblePlays.Sort();
                TimeSpan playTimeSpan = possiblePlays.LastOrDefault();
                return TryFindPlayAtExactTime(playTimeSpan);
            }
            else
            {
                return null;
            }
        }
    }
}
