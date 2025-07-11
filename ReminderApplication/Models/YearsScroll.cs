using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReminderApplication.Models
{
    public class YearsScroll
    {
        private int _centerYear = DateTime.Now.Year;
        public ObservableCollection<int> Years { get; } = new ObservableCollection<int>();
        //public event PropertyChangedEventHandler PropertyChanged;
        private int CenterYear
        {
            get => _centerYear;
            set
            {
                _centerYear = value;
                //OnPropertyChanged();
                UpdateYears();
            }
        }
        public YearsScroll()
        {
            UpdateYears();
        }
        private void UpdateYears()
        {
            Years.Clear();
            for (int i = -2; i <= 2; i++)
                Years.Add(CenterYear + i);
        }
        public void MoveLeft() => CenterYear--;
        public void MoveRight() => CenterYear++;
        public void SelectedYear(int year) => CenterYear = year;
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
