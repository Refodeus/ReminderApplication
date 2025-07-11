using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderApplication.Models
{
    public class NotesFileService
    {
        private const string BaseFolder = "Data";
        public void DeleteNote(Note note)
        {
            var path = GetNoteFilePath(note.Date, note.Id);
            if (File.Exists(path))
            {
                File.Delete(path);
                CleanEmptyFolders(Path.GetDirectoryName(path));
            }
        }
        private void CleanEmptyFolders(string folderPath)
        {
            while (folderPath != BaseFolder && Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                Directory.Delete(folderPath);
                folderPath = Path.GetDirectoryName(folderPath);
            }
        }
            public void SaveNote(Note note)
        {
            var path = GetNoteFilePath(note.Date, note.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(note, settings));
        }
        public List<Note> LoadNotesForMonth(int year, int month)
        {
            var monthFolder = GetMonthFolderPath(year, month);
            if (!Directory.Exists(monthFolder)) return new List<Note>();
            return Directory.GetFiles(monthFolder, "*.json").Select(file => JsonConvert.DeserializeObject<Note>(File.ReadAllText(file))).ToList();
        }
        private string GetNoteFilePath(DateTime date, string id) =>
             Path.Combine(BaseFolder, date.Year.ToString(), $"{date.Month:00}_{date:MMM}", $"notes_{date:dd}_{id}.json");
        private string GetMonthFolderPath(int year, int month) =>
            Path.Combine(BaseFolder, year.ToString(), $"{month:00}_{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}");
    }
}
