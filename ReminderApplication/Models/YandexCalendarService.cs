using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ReminderApplication.Models
{
    public class YandexCalendarService
    {
        //private readonly string ClientId = "d4f60e9810f44d5d922d3443d435f70b";
        //private readonly string SecretId = "7df2e04e4e6d4a2fb506a51ad1aa5210";
        //private string _accessToken;
        //public async Task<string> Authenticate()
        //{
        //    using var client = new HttpClient();

        //    // 1. Запрашиваем код устройства
        //    var deviceResponse = await client.PostAsync(
        //        "https://oauth.yandex.ru/device/code",
        //        new FormUrlEncodedContent(new[]
        //        {
        //    new KeyValuePair<string, string>("client_id", ClientId),
        //    new KeyValuePair<string, string>("client_secret", SecretId)
        //        }));

        //    var deviceData = JsonConvert.DeserializeObject<YandexDeviceResponse>(
        //        await deviceResponse.Content.ReadAsStringAsync());

        //    var confirmationUrl = $"https://oauth.yandex.ru/device?user_code={deviceData.user_code}";
        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = confirmationUrl,
        //        UseShellExecute = true
        //    });
        //    await Task.Delay(10000);
        //    var tokenResponse = await client.PostAsync(
        //    "https://oauth.yandex.ru/token",
        //    new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("grant_type", "device_code"),
        //        new KeyValuePair<string, string>("code", deviceData.device_code),
        //        new KeyValuePair<string, string>("client_id", ClientId),
        //        new KeyValuePair<string, string>("client_secret", SecretId)
        //    }));

        //    return JsonConvert.DeserializeObject<TokenResponse>(
        //        await tokenResponse.Content.ReadAsStringAsync()).access_token;
        //}
        public async Task SyncWithYandexCalendar(Note note)
        {
            string email = "Romylik22R@yandex.ru";
            string appPassword = "rlymksrarnbchsiz";
            string calendarUrl = $"https://caldav.yandex.ru/calendars/{Uri.EscapeDataString(email)}/events-default/";

            using var client = new HttpClient();

            // 1. Настройка таймаутов
            client.Timeout = TimeSpan.FromSeconds(30);

            // 2. Добавляем обязательные заголовки
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{appPassword}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            client.DefaultRequestHeaders.Add("Depth", "1");
            client.DefaultRequestHeaders.Add("Prefer", "return-minimal");

            // 3. Формируем iCalendar событие
            var icalEvent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Example Corp.//CalDAV Client//EN
BEGIN:VEVENT
UID:{Guid.NewGuid()}
DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}
DTSTART:{note.StartDateTime.ToUniversalTime():yyyyMMddTHHmmssZ}
DTEND:{note.EndDateTime.ToUniversalTime():yyyyMMddTHHmmssZ}
SUMMARY:{note.Title}
TRANSP:OPAQUE
END:VEVENT
END:VCALENDAR";

            var content = new StringContent(icalEvent, Encoding.UTF8, "text/calendar");

            // 4. Пробуем несколько раз с задержкой
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await client.PutAsync(
                        $"{calendarUrl}{Guid.NewGuid()}.ics",
                        content);

                    if (response.IsSuccessStatusCode)
                        return;

                    if ((int)response.StatusCode >= 500)
                    {
                        await Task.Delay(1000 * (i + 1)); // Экспоненциальная задержка
                        continue;
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"CalDAV error ({response.StatusCode}): {error}");
                }
                catch (TaskCanceledException) when (i < maxRetries - 1)
                {
                    await Task.Delay(1000 * (i + 1));
                    continue;
                }
            }

            throw new Exception("Не удалось синхронизировать с календарём после нескольких попыток");
        }
    }
}
