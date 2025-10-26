using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fairmark.AppServices
{
    public sealed class FairmarkNoteItem
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;
        [JsonPropertyName("emoji")]
        public string Emoji { get; set; } = "📋";
        [JsonPropertyName("colors")]
        public Windows.UI.Color[] Colors { get; set; } = null;
    }
}
