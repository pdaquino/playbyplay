using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using FootballAnalyzer;
using FootballAnalyzerWindows.Common;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFindPlayByTime()
        {
            GameFilm film = new GameFilm(null);
            film.AddPlay(TimeSpan.FromSeconds(4), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(0), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(3), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(1), FootballAnalyzerWindows.Models.PlayType.Offense);
            

            Assert.AreEqual(1, film.GetPlay(TimeSpan.FromSeconds(2)).TimeInGame.Seconds);
        }

        private void CompareSerializedFilms(GameFilm expected, GameFilm actual)
        {
            Assert.AreEqual(expected.Plays.Count, actual.Plays.Count);
            for(int i = 0; i < expected.Plays.Count; i++)
            {
                Play expectedPlay = expected.Plays[i];
                Play actualPlay = actual.Plays[i];

                Assert.AreEqual(actual, actualPlay.Parent);
                Assert.AreEqual(expectedPlay.TimeInGame, actualPlay.TimeInGame);
                Assert.AreEqual(expectedPlay.Name, actualPlay.Name);
                Assert.AreEqual(expectedPlay.Notes, actualPlay.Notes);
                Assert.AreEqual(expectedPlay.Type, actualPlay.Type);
            }
        }

        [TestMethod]
        public void TestSerialization1()
        {
            GameFilm film = new GameFilm(null);
            film.AddPlay(TimeSpan.FromSeconds(4), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(0), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(3), FootballAnalyzerWindows.Models.PlayType.Defense);
            film.AddPlay(TimeSpan.FromSeconds(1), FootballAnalyzerWindows.Models.PlayType.Offense);
            
            SaveManager saveMgr = new SaveManager();
            string saveFile = saveMgr.MakeSaveFile(film);

            GameFilm serializedFilm = saveMgr.LoadSaveFile(saveFile);
            CompareSerializedFilms(film, serializedFilm);
        }
    }
}
