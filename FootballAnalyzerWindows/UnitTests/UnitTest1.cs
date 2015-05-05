using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using FootballAnalyzer;

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

        [TestMethod]
        public void TestFindPlayByTime2()
        {
            GameFilm film = new GameFilm(null);
            film.AddPlay(TimeSpan.FromSeconds(1), FootballAnalyzerWindows.Models.PlayType.Offense);
            film.AddPlay(TimeSpan.FromSeconds(3), FootballAnalyzerWindows.Models.PlayType.Offense);

            Assert.AreEqual(1, film.TryFindPlay(TimeSpan.FromSeconds(2)).TimeInGame.Seconds);
        }


    }
}
