using System;
using System.Collections.Generic;

using TwitterAnalyser.Core.JsonConverters;

using Newtonsoft.Json;

namespace TwitterAnalyser.Core.DTO
{
    public class TwitterSearchResponse_v1
    {
        [JsonProperty("statuses")]
        public IEnumerable<Status> Statuses { get; set; }

        [JsonProperty("search_metadata")]
        public SearchMetadata Metadata { get; set; }

        public class Status
        {
            [JsonProperty("created_at")]
            [JsonConverter(typeof(StringToDateTimeConverter_v1))]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("id_str")]
            public string Id { get; set; }
        }

        public class SearchMetadata
        {
            [JsonProperty("max_id")]
            public long MaxId { get; set; }

            [JsonProperty("next_results")]
            public string NextResults { get; set; }
        }
    }
}
