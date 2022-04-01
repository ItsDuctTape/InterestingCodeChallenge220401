using Newtonsoft.Json;

namespace JhaChallengeTweetStreamer.Model.Twitter
{
    /// <summary>
    /// Simple class representing a Twitter Sample Stream message object
    /// Used for Desieralization and Data Validation
    /// </summary>
    public class TweetSampleMessage
    {
        [JsonProperty("data")]
        public TweetProperties TweetProperties { get; set; }        
    }

    public class TweetProperties
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
