using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model.Twitter;
using Newtonsoft.Json;
using System;

namespace JhaChallengeTweetStreamer.Core
{
    /// <summary>
    /// Contains Data Specific Validation Logic
    /// </summary>
    public class TwitterV2ApiSampleStreamMessageValidator : IMessageValidator
    {
        public bool MessageIsValid(string messageContent)
        {
            try
            {
                TweetSampleMessage dataMsg = JsonConvert.DeserializeObject<Model.Twitter.TweetSampleMessage>(messageContent);
                if (dataMsg.TweetProperties.Id <= 0)
                    throw new Exception("Unable to Parse Message Id");

                if ( dataMsg.TweetProperties.Id < 1000000000000000000 || 
                     dataMsg.TweetProperties.Id > 7000000000000000000)
                    throw new Exception("Message id out of range.");

                if (String.IsNullOrEmpty(dataMsg.TweetProperties.Text))
                    throw new Exception("Unable to Parse Message Text/Content");

                return true;
            }
            catch
            {
                // Deserialize or Parsing failed

                return false;
            }
        }

        



    }
}
