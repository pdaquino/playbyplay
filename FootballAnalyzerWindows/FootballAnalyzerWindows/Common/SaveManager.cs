using FootballAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace FootballAnalyzerWindows.Common
{
    public class SaveManager
    {
        public string MakeSaveFile(GameFilm gameFilm)
        {
            var xmlSerializer = new XmlSerializer(gameFilm.GetType());
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlSerializer.Serialize(writer, gameFilm);
                return stringWriter.ToString();
            }
        }

        public async Task<GameFilm> LoadSaveFileThroughPicker()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pbp");
            StorageFile gameFilmFile = await picker.PickSingleFileAsync();
            string contents = await FileIO.ReadTextAsync(gameFilmFile);
            return LoadSaveFile(contents);
        }

        public GameFilm LoadSaveFile(string saveFile)
        {
            return LoadSaveFile(new StringReader(saveFile));
        }

        private GameFilm LoadSaveFile(TextReader textReader)
        {
            var xmlSerializer = new XmlSerializer(typeof(GameFilm));
            GameFilm film = (GameFilm) xmlSerializer.Deserialize(textReader);

            // The "parent" field of Plays doesn't get serialized because that would be
            // a circular reference.
            foreach(Play play in film.Plays)
            {
                play.Parent = film;
            }

            return film;
        }

        public async void SaveFileThroughPicker(GameFilm film)
        {
            var fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add("Play by Play files", new List<string>() { ".pbp" });
            if (film.VideoFile != null)
            {

                fileSavePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(film.VideoFile.Name);
            }
            var fileToSave = await fileSavePicker.PickSaveFileAsync();
            if (fileToSave != null)
            {
                string fileContents = MakeSaveFile(film);
                await FileIO.WriteTextAsync(fileToSave, fileContents);
            }
        }
    }
}
