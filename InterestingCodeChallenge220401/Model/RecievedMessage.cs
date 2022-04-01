using System;

namespace JhaChallengeTweetStreamer.Model
{

    /// <summary>
    ///  Simple Message Object that wraps the original
    ///  API Stream provided message content and provideds 
    ///  a timestamp of the time of receipt.
    /// </summary>
    public class ReceivedMessage
    {
        public ReceivedMessage()
        {

        }

        public ReceivedMessage(DateTime receivedTimestamp, string message)
        {
            this.ReceivedTimestamp = receivedTimestamp;
            this.Message = message; 
        }

        public DateTime ReceivedTimestamp { get; set; }

        public string Message { get; set; }
    }
}
