using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderApplication.Models
{
    public class DailyNotes
    {
        public DateTime Date { get; set; }
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
    }
}
