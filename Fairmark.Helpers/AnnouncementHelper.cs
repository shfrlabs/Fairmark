using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public static class AnnouncementHelper
    {
        internal static string url = "https://raw.githubusercontent.com/shef3r/announcements/refs/heads/main/apps/fairmark.json";
        public static int[] dismissedAnnoucements
        {
            get
            {
                ApplicationData.Current.LocalSettings.Values.TryGetValue("dismissedAnnoucements", out object value);
                if (value != null && value is string json)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<int[]>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                return new int[] { };
            }
            set
            {
                    string json = System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    ApplicationData.Current.LocalSettings.Values["dismissedAnnoucements"] = json;

            }
        }
        public static void DismissAnnouncement(int id)
        {
            var dismissed = dismissedAnnoucements.ToList();
            if (!dismissed.Contains(id))
            {
                dismissed.Add(id);
                dismissedAnnoucements = dismissed.ToArray();
            }
        }
        public static async Task<Announcement> GetCurrentAnnoucement()
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage req = await client.GetAsync(url);
                req.EnsureSuccessStatusCode();
                string responseBody = await req.Content.ReadAsStringAsync();
                Debug.WriteLine(responseBody);
                Announcement[] anns = System.Text.Json.JsonSerializer.Deserialize<Announcement[]>(responseBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return CheckAnnouncements(anns);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching announcements: {ex.Message}");
                return null;
            }
        }

        private static Announcement CheckAnnouncements(Announcement[] anns)
        {
            foreach (var ann in anns)
            {
                Debug.WriteLine($"Checking announcement {ann.id} from {ann.from} until {ann.until}");
                if (DateTime.TryParseExact(ann.from, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out DateTime fromDate) && DateTime.TryParseExact(ann.until, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out DateTime untilDate))
                {
                    if (DateTime.Now >= fromDate && DateTime.Now <= untilDate)
                    {
                        if (!dismissedAnnoucements.Contains(ann.id))
                        {
                            return ann;
                        }
                        else
                        {
                            Debug.WriteLine($"Announcement {ann.id} has been dismissed.");
                        }
                    }
                    else
                    {
                            Debug.WriteLine($"Announcement {ann.id} ineligible for display.");
                    }
                }
            }
            return null;
        }
    }
}
