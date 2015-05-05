using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using FootballAnalyzerWindows.Models;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace FootballAnalyzer
{
    public class GameFilm
    {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        private IRandomAccessStream m_videoStream;

        public List<Play> Plays { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public StorageFile VideoFile { get; set; }
        
        public GameFilm(StorageFile videoFile)
        {
            VideoFile = videoFile;
            Plays = new List<Play>();
        }

        public async Task<IRandomAccessStream> GetVideoStream()
        {
            if (m_videoStream == null)
            {
                m_videoStream = await VideoFile.OpenAsync(FileAccessMode.Read);
            }
            return m_videoStream;
        }

        public Play AddPlay(TimeSpan time, PlayType type)
        {
            var index = Plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index < 0) 
            {
                index = ~index;
                Play play = new Play(this, time, type);
                Plays.Insert(index, play);
                return play;
            }
            else
            {
                //play already exists at this point
                return null;
            }
        }

        public void RemovePlay(Play play)
        {
            RemovePlay(play.TimeInGame);
        }

        public void RemovePlay(TimeSpan time)
        {
            var index = Plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index >= 0)
            {
                Plays.RemoveAt(index);
            }
            else
            {
                // No play exists at the specified time
            }
        }

        public int GetPlayNumber(Play play)
        {
            return GetPlayNumber(play.TimeInGame);
        }

        public int GetPlayNumber(TimeSpan time)
        {
            int index = Plays.Select(i => i.TimeInGame).ToList().BinarySearch(time);
            if (index < 0)
            {
                index = ~index - 1;
            }
            return index;
        }

        public Play GetPlay(TimeSpan time)
        {
            int playNumber = this.GetPlayNumber(time);
            return playNumber < 0 ? null : Plays[playNumber];
        }

        private GameFilm()
        {

        }
    }
}
