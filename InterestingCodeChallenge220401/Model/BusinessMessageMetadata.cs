using System;

namespace JhaChallengeTweetStreamer.Model
{
    /// <summary>
    ///  Message Object that can be used for downstream 
    ///  analysis of message stream health and performance
    /// </summary>
    public class BusinessMessageMetadata  
    {
        public Int64 Id { get; set; }
        
        public DateTime ReceivedTimestamp { get; set; }

        public int DataLength { get; set; }

        public bool IsValid { get; internal set; }

    }
}
