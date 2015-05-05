using FootballAnalyzer;
using FootballAnalyzerWindows.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FootballAnalyzerWindows.Common
{
    public class PlayThumbnailButtonHelper
    {
        private static SolidColorBrush m_bgColor = new SolidColorBrush(Colors.WhiteSmoke);
        private static double m_height = 100;
        private static double m_width = 150;
        public static Button FromPlay(Play play)
        {
            Button button = new Button
            {
                //Background = m_bgColor,
                Tag = play
            };
            Grid buttonGrid = MakeButtonGrid(play);


            button.Content = buttonGrid;
            return button;
        }

        private static Grid MakeButtonGrid(Play play)
        {
            Grid buttonGrid = new Grid();
            buttonGrid.RowDefinitions.Add(new RowDefinition());
            buttonGrid.RowDefinitions.Add(new RowDefinition());


            TextBlock playName = new TextBlock
            {
                Text = GetPlayName(play)
            };
            playName.FontSize = 12;
            playName.SetValue(Grid.RowProperty, 1);
            playName.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            playName.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Center);
            playName.Name = "playName";

            // We need the border to apply the background color
            Border textBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0xBF, 0, 0, 0))
            };
            textBorder.SetValue(Grid.RowProperty, 1);
            buttonGrid.Children.Add(
                textBorder
            );
            buttonGrid.Children.Add(playName);


            Image thumbnail = new Image
            {
                Source = play.Thumbnail,
                Height = m_height,
                Width = m_width,
                Name = "playThumbnail"
            };
            Grid.SetRowSpan(thumbnail, 2);
            buttonGrid.Children.Add(thumbnail);
            return buttonGrid;
        }

        private static string GetPlayName(Play play)
        {
            return String.Format(
                                    "{0} #{1}",
                                    PlayTypeName.FromPlayType(play.Type),
                                    play.Parent.GetPlayNumber(play));
        }

        public static void AssociateButtonToPlay(Button button, Play play)
        {
            button.Content = MakeButtonGrid(play);
            button.Tag = play;
        }
    }
}
