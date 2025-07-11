namespace ReminderApplication.Models
{
    public class YandexDeviceResponse
    {
        public string device_code { get; set; }
        public string user_code { get; set; }
        public string verification_url { get; set; }
        public int interval { get; set; }
    }
}