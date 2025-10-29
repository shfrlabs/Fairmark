using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.UI;

namespace Fairmark.AppServices
{
    public sealed class AppServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private AppServiceConnection _connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (details == null)
            {
                _deferral.Complete();
                _deferral = null;
                return;
            }

            _connection = details.AppServiceConnection;
            _connection.RequestReceived += OnRequestReceived;
            _connection.ServiceClosed += OnServiceClosed;
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            var requestDeferral = args.GetDeferral();

            try
            {
                string noteListJson = await GetNoteList();

                var response = new ValueSet
                {
                    { "Result", noteListJson }
                };

                await args.Request.SendResponseAsync(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnRequestReceived failed: {ex}");
            }
            finally
            {
                requestDeferral.Complete();
            }
        }

        private void OnServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            if (_connection != null)
            {
                _connection.RequestReceived -= OnRequestReceived;
                _connection.ServiceClosed -= OnServiceClosed;
                _connection = null;
            }

            _deferral?.Complete();
            _deferral = null;
        }

        private async Task<string> GetNoteList()
        {
            try
            {
                (string Id, string Name, string Emoji, Color[] Colors, bool IsPinned, DateTimeOffset? LastModified)[] values = await Fairmark.Helpers.NoteCollectionHelper.GetNoteListAsync();
                List<FairmarkNoteItem> noteItems = new List<FairmarkNoteItem>();

                Debug.WriteLine(values.Count());
                foreach (var value in values)
                {
                    noteItems.Add(new FairmarkNoteItem
                    {
                        ID = value.Id,
                        Name = value.Name,
                        Emoji = value.Emoji,
                        Colors = value.Colors,
                        IsPinned = value.IsPinned,
                        LastModified = value.LastModified
                    });
                }

                var sortedNoteItems = noteItems.OrderByDescending(note => note.LastModified.Value.Ticks).ToList();

                foreach (var item in sortedNoteItems)
                {
                    Debug.WriteLine(item.Name + ": " + item.LastModified.Value.Ticks);
                }

                Debug.Write(System.Text.Json.JsonSerializer.Serialize(sortedNoteItems[0]));
                return System.Text.Json.JsonSerializer.Serialize(sortedNoteItems);
            }
            catch
            {
                Debug.WriteLine("Failed to get note list.");
                return "[]";
            }
        }
    }
}
