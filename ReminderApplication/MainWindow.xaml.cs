using ReminderApplication.Models;
using ReminderApplication.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Wpf.Toolkit;

namespace ReminderApplication
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DailyNotes> _allNotes = new ObservableCollection<DailyNotes>();

        private NotesFileService _notesFileService = new NotesFileService();
        private readonly YandexCalendarService _yandexCalendarService = new();
        private int currentYear = DateTime.Now.Year;
        private int currentMonth = DateTime.Now.Month, currentDay = DateTime.Now.Day;
        private readonly string[] Months = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", 
            "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" }; 

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNotesForCurrentMonth();
            UpdateYears();
            UpdateMonths();
            foreach (var note in _allNotes.SelectMany(day => day.Notes))
            {
                note.ScheduleNotifications();
            }
        }

        private void LoadNotesForCurrentMonth()
        {
            var monthNotes = _notesFileService.LoadNotesForMonth(DateTime.Now.Year, DateTime.Now.Month);
            foreach(var note in monthNotes)
            {
                AddOrUpdateNote(note);
                UpdateNotesContainer(note.Date);
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void lblNote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtNote.Focus();
        }

        private void txtNote_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNote.Text) && txtNote.Text.Length > 0)
            {
                lblNote.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblNote.Visibility = Visibility.Visible;
            }
        }
        private void UpdateMonthAndYear()
        {
            UpdateYears();
            UpdateMonths();
        }
        private void YearsScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            currentYear--;
            UpdateMonthAndYear();
        }

        private void YearsScrollRight_Click(object sender, RoutedEventArgs e)
        {
            currentYear++;
            UpdateMonthAndYear();
        }

        private void YearScroll_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            currentYear = int.Parse(btn.Content.ToString());
            UpdateMonthAndYear();
        }
        private void MonthButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            currentMonth = int.Parse(btn.Content.ToString());
            currentDay = MainCalendar.SelectedDate!.Value.Day;
            UpdateMonths();
        }
        private void UpdateYears()
        {
            var yearButtons = new[] { Year1, Year2, Year3, Year4, Year5 };
            for (int i = 0; i < yearButtons.Length; i++)
            {
                int year = currentYear - 2 + i;
                yearButtons[i].Content = year;
                if (i == 2)
                {
                    yearButtons[i].FontSize = 24;
                    yearButtons[i].Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c76f69"));
                    yearButtons[i].BeginAnimation(FontSizeProperty, new DoubleAnimation(16, 24, TimeSpan.FromSeconds(0.3)));
                }
                else
                {
                    yearButtons[i].Style = (Style)FindResource("button");
                }
            }
        }
        private void UpdateMonths()
        {
            MonthName.Text = Months[currentMonth - 1];
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            if (daysInMonth < currentDay)
                currentDay = daysInMonth;
            MainCalendar.DisplayDate = new DateTime(currentYear, currentMonth, currentDay);
            MainCalendar.SelectedDate = new DateTime(currentYear, currentMonth, currentDay);

            foreach (var child in MonthButtonsPanel.Children)
            {
                if (child is Button btn)
                {
                    var brush = new SolidColorBrush(Colors.White);
                    btn.Foreground = brush;
                    var colorAnimation = new ColorAnimation
                    {
                        From = Colors.White,
                        To = (Color)ColorConverter.ConvertFromString("#bababa"),
                        Duration = TimeSpan.FromSeconds(0.8)
                    };
                    var opacityAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.8),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                    MainCalendar.BeginAnimation(OpacityProperty, opacityAnimation);
                    btn.FontWeight = FontWeights.Normal;
                }

            }
            if (MonthButtonsPanel.Children[currentMonth - 1] is Button currentButton)
            {
                currentButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c73f69"));
                currentButton.FontWeight = FontWeights.SemiBold;
            }
        }

        private void TimeValue_Changed(object sender, RoutedEventArgs e)
        {
            var maskedBox = (MaskedTextBox)sender;
            string timeText = maskedBox.Text;
            if (!timeText.Contains('_'))
            {
                if (!isValidTimeFormat(timeText))
                {
                    maskedBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c7003d"));
                    maskedBox.ToolTip = "Некорректное время! Пример: 12:30 - 14:45";
                }
                else
                {
                    maskedBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eba5bb"));
                    maskedBox.ToolTip = null;
                }
            }
        }
        private bool isValidTimeFormat(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr))
                return false;
            if (!Regex.IsMatch(timeStr, @"^([01]?[0-9]|2[0-3]):[0-5][0-9] - ([01]?[0-9]|2[0-3]):[0-5][0-9]$"))
                return false;
            var parts = timeStr.Split(new[] { " - " }, StringSplitOptions.None);
            TimeSpan startTime, endTime;

            if (!TimeSpan.TryParse(parts[0], out startTime) ||
                !TimeSpan.TryParse(parts[1], out endTime))
                return false;

            return startTime < endTime;
        }
        private void PressLeftAndRightAngles(int increment)
        {
            int year = MainCalendar.SelectedDate!.Value.Year;
            DateTime dateTime = MainCalendar.SelectedDate.Value.AddDays(increment);
            currentDay = dateTime.Day;
            currentMonth = dateTime.Month;
            if (year != dateTime.Year)
            {
                currentYear += increment;
                UpdateMonthAndYear();
            }
            else
                UpdateMonths();
        }
        private void DayAngleButton_ClickLeft(object sender, RoutedEventArgs e) =>
            PressLeftAndRightAngles(-1);

        private void DayAngleButton_ClickRight(object sender, RoutedEventArgs e) =>
            PressLeftAndRightAngles(1);

        private void BellToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BellToggle.Icon = BellToggle.Icon == FontAwesome.WPF.FontAwesomeIcon.Bell ? 
                FontAwesome.WPF.FontAwesomeIcon.BellSlash : FontAwesome.WPF.FontAwesomeIcon.Bell;
            BellToggle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString
                (BellToggle.Icon == FontAwesome.WPF.FontAwesomeIcon.Bell ? "#f1f1f1" : "#eba5bb"));
        }

        private void AddNewNote_Click(object sender, RoutedEventArgs e)
        {
            if (txtNote.Text.Length > 0 && isValidTimeFormat(TimeValue.Text))
            {
                var timeParts = TimeValue.Text.Split('-');
                var startTime = TimeSpan.Parse(timeParts[0].Trim());
                var endTime = TimeSpan.Parse(timeParts[1].Trim());
                var newNote = new Note
                {
                    Id = DateTime.Now.Ticks.ToString("x"),
                    Date = MainCalendar.SelectedDate ?? DateTime.Today,
                    Title = txtNote.Text,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsCompleted = false,
                    HasReminder = BellToggle.Icon == FontAwesome.WPF.FontAwesomeIcon.Bell ? true : false,
                    TimeDisplay = TimeValue.Text
                };
                AddOrUpdateNote(newNote);
                UpdateNotesContainer(newNote.Date);
            }
        }
        private void AddOrUpdateNote(Note note)
        {
            var dailtyNotes = _allNotes.FirstOrDefault(d => d.Date == note.Date.Date);
            if (dailtyNotes == null)
            {
                dailtyNotes = new DailyNotes {  Date = note.Date };
                _allNotes.Add(dailtyNotes);
            }
            dailtyNotes.Notes.Add(note);
            note.ScheduleNotifications();
        }
        private void UpdateNotesContainer(DateTime date)
        {
            NotesContainer.Children.Clear();
            var dailyNotes = _allNotes.FirstOrDefault(d => d.Date == date);
            if (dailyNotes == null) return;
            foreach(var note in dailyNotes.Notes.OrderBy(p => p.StartTime))
            {
                var newItem = new Item
                {
                    Note = note,
                    Title = note.Title,
                    Time = note.TimeDisplay,
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(note.ColorHex)),
                    Icon = note.IsCompleted ? FontAwesome.WPF.FontAwesomeIcon.CheckCircle : FontAwesome.WPF.FontAwesomeIcon.CircleThin,
                    IconBell = note.HasReminder ? FontAwesome.WPF.FontAwesomeIcon.Bell : FontAwesome.WPF.FontAwesomeIcon.BellSlash
                };
                newItem.OnNoteDeleted += OnNoteDeletedHandler;
                newItem.NoteChanged += NoteChangedHandler;
                _notesFileService.SaveNote(note);
                NotesContainer.Children.Add(newItem);
            }
        }
        private void NoteChangedHandler(object? sender, Note e) => _notesFileService.SaveNote(e);

        private void OnNoteDeletedHandler(object? sender, Note deletedNote)
        {
            var dailyNotes = _allNotes.FirstOrDefault(d => d.Date.Date == deletedNote.Date.Date);
            if (dailyNotes != null)
            {
                dailyNotes.Notes.Remove(deletedNote);
                if (!dailyNotes.Notes.Any())
                {
                    _allNotes.Remove(dailyNotes);
                }
            }
        }

        private void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainCalendar.SelectedDate.HasValue)
                UpdateNotesContainer(MainCalendar.SelectedDate.Value.Date);
        }

        private async void YandexSync_Click(object sender, RoutedEventArgs e)
        {
            //string response = await _yandexCalendarService.Authenticate();
            //if (!string.IsNullOrEmpty(response))
            //{
            //    }
            foreach (var note in _allNotes.SelectMany(d => d.Notes))
                await _yandexCalendarService.SyncWithYandexCalendar(note);
            System.Windows.MessageBox.Show("Синхронизация с Яндекс Календарем завершена!", "Синхронизация", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        protected override void OnClosed(EventArgs e)
        {
            foreach(var note in _allNotes.SelectMany(day => day.Notes))
            {
                note.Dispose();
            }
            base.OnClosed(e);
        }
    }
}
