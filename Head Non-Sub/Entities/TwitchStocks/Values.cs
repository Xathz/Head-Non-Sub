using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Entities.TwitchStocks {

    public class Values {

        [JsonProperty("data")]
        public List<List<double>> Data { get; set; }

    }

}
