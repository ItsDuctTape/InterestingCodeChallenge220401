using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using System.Collections.Generic;

namespace JhaChallengeTweetStreamer.DAL
{
    /// <summary>
    /// Represents a Message Broker Queue for FIFO handling of messages
    ///    that are used by stream rate/data analysis logic.
    /// To be replaced by external Message Broker Queue (RabbitMq, 
    ///     Azure Service Bus, etc) for production-scale processing
    /// </summary>
    public class BusinessMessageMetadataInMemoryQueueProvider : IQueueProvider<BusinessMessageMetadata>
    {
        private Queue<BusinessMessageMetadata> _msgQueue { get; set; }

        public BusinessMessageMetadataInMemoryQueueProvider()
        {
            _msgQueue = new Queue<BusinessMessageMetadata>();
        }

        public void Enqueue(BusinessMessageMetadata metadata)
        {
            _msgQueue.Enqueue(metadata);            
        }

        public BusinessMessageMetadata Dequeue()
        {
            return _msgQueue.Dequeue();
        }

        public int Count()
        {
            return _msgQueue.Count;
        }
    }
}
