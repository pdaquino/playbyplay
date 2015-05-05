using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballAnalyzerWindows.Models
{
    public enum PlayType
    {
        Offense,
        Defense,
        SpecialTeams
    }

    public class PlayTypeName
    {
        public static string FromPlayType(PlayType Type)
        {
            switch(Type)
            {
                case PlayType.Offense:
                    return "Offense";
                case PlayType.Defense:
                    return "Defense";
                case PlayType.SpecialTeams:
                    return "Special teams";
                default:
                    return "Unknown play type";
            }
        }
    }
}
