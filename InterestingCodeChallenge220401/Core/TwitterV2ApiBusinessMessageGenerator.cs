using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Model.Twitter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace JhaChallengeTweetStreamer.Core
{
    /// <summary>
    /// Contains Non-Trivial Constructor methods for special message objects
    ///   that are used in message queues
    /// </summary>
    public class TwitterV2ApiBusinessMessageGenerator : IBusinessMessageGenerator
    {

        /// <summary>
        ///  Creates a BusinessMessage object that is used for logic to persist
        ///     the message to permenant storage.
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <returns></returns>
        public BusinessMessage BuildBusinessMessageFromApiMessage(ReceivedMessage receivedMessage)
        {
            try
            {
                TweetSampleMessage dataMsg = JsonConvert.DeserializeObject<Model.Twitter.TweetSampleMessage>(receivedMessage.Message);
                if (dataMsg.TweetProperties.Id <= 0)
                    throw new Exception("Unable to Parse Message Id");
                if (String.IsNullOrEmpty(dataMsg.TweetProperties.Text))
                    throw new Exception("Unable to Parse Message Text/Content");

                var retObj = new BusinessMessage()
                {
                    Id = dataMsg.TweetProperties.Id,
                    Payload = dataMsg.TweetProperties.Text,
                    OriginalMessage = receivedMessage.Message,
                    ReceivedTimestamp = receivedMessage.ReceivedTimestamp
                };
                return retObj;
            }
            catch
            {
                // Deserialize failed
            }
            return null;
        }


        /// <summary>
        /// Creates a BusinessMessageMetadata object for use by Analysis Logic
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <param name="isValid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public BusinessMessageMetadata BuildBusinessMessageMetadataFromApiMessage(ReceivedMessage receivedMessage, bool isValid, long id = 0)
        {
            var retObj = new BusinessMessageMetadata();
            retObj.IsValid = isValid;
            retObj.DataLength = receivedMessage.Message.Length;
            retObj.Id = 0;
            retObj.ReceivedTimestamp = receivedMessage.ReceivedTimestamp;

            try
            {
                if (id > 0)
                {
                    JObject data = JObject.Parse(receivedMessage.Message);
                    var keyValuePairs = JObject.Parse(data["data"].ToString());
                    var idStringValue = keyValuePairs["id"];
                    var idVal = Convert.ToInt64(idStringValue.ToString());
                    if (idVal <= 0)
                        throw new Exception("Unable to Parse Message Id");
                    var textValue = keyValuePairs["text"];
                    if (String.IsNullOrEmpty(textValue.ToString()))
                        throw new Exception("Unable to Parse Message Text/Content");
                }
            }
            catch { }
            
            return retObj;
        }

    }
}
