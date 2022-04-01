using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using System.Collections.Generic;

namespace JhaChallengeTweetStreamer.DAL
{
    /// <summary>
    /// Represents a Message Broker Queue for FIFO handling of messages
    ///    that are to be persisted to some form of permenant storage.
    /// To be replaced by external Message Broker Queue (RabbitMq, 
    ///     Azure Service Bus, etc) for production-scale processing
    /// </summary>
    public class BusinessMessageInMemoryQueueProvider : IQueueProvider<BusinessMessage>
    {
        private Queue<BusinessMessage> _msgQueue { get; set; }

        public BusinessMessageInMemoryQueueProvider()
        {
            _msgQueue = new Queue<BusinessMessage>();
        }

        public void Enqueue(BusinessMessage busMsg)
        {
            _msgQueue.Enqueue(busMsg);            
        }

        public BusinessMessage Dequeue()
        {
            return _msgQueue.Dequeue();
        }

        public int Count()
        {
            return _msgQueue.Count;
        }

    }
}
