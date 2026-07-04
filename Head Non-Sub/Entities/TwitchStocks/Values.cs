using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.TwitchStocks {

    public class Values {

        [JsonPropertyName("data")]
        public List<List<double>> Data { get; set; }

    }

}
