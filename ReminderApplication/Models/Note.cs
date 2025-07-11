using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ReminderApplication.Models
{
    public class Note : IDisposable
    {
        //Основные поля класса
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ColorHex { get; set; } = "#f1f1f1";
        public bool IsCompleted { get; set; }
        public bool HasReminder { get; set; }

        //Вспомогательные поля
        [JsonIgnore]
        public string TimeDisplay { get; set; }
        [JsonIgnore]
        public DateTime StartDateTime => Date.Add(StartTime);
        [JsonIgnore]
        public DateTime EndDateTime => Date.Add(EndTime);

        [JsonIgnore]
        private System.Timers.Timer _startTimer;
        [JsonIgnore]
        private System.Timers.Timer _endTimer;
        [JsonIgnore]
        private readonly NotificationService _notificationService = new();
        public void ScheduleNotifications()
        {
            CancelNotifications();
            if (!HasReminder) return;
            ScheduleTimer(StartDateTime, $"Начинается: {Title}");
            ScheduleTimer(EndDateTime, $"Заканчивается: {Title}");
        }
        private void ScheduleTimer(DateTime eventTime, string message)
        {
            var delay = eventTime - DateTime.Now;
            if (delay <= TimeSpan.Zero) return;
            var timer = new System.Timers.Timer(delay.TotalMilliseconds) { AutoReset = false };
            timer.Elapsed += (s, e) => _notificationService.ShowToast(message, ColorHex);
            timer.Start();
            if (eventTime == StartDateTime)
                _startTimer = timer;
            else
                _endTimer = timer;
        }
        private void CancelNotifications()
        {
            _startTimer?.Dispose();
            _endTimer?.Dispose();
        }
        public void Dispose() => CancelNotifications();
    }
}
