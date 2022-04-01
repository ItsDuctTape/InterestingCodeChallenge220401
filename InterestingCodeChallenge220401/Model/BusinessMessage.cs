using System;

namespace JhaChallengeTweetStreamer.Model
{
    /// <summary>
    ///  Message Object that can be used for various 
    ///  downstream business logic processing
    /// </summary>
    public class BusinessMessage
    {       
        public Int64 Id { get; set; }
        
        public string Payload { get; set; }

        public string OriginalMessage { get; set; }

        public DateTime ReceivedTimestamp { get; set; }

    }
}
