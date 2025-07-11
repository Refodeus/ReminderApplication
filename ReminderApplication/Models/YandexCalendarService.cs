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
        public async Task SyncWithYandexCalendar(Note note)
        {
            string email = "Romylik22R@yandex.ru";
            string appPassword = "rlymksrarnbchsiz";
            string calendarUrl = $"https://caldav.yandex.ru/calendars/{Uri.EscapeDataString(email)}/events-default/";

            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(30);

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{appPassword}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            client.DefaultRequestHeaders.Add("Depth", "1");
            client.DefaultRequestHeaders.Add("Prefer", "return-minimal");

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
                        await Task.Delay(1000 * (i + 1));
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
