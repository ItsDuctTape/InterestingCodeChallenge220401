using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using System.Collections.Generic;

namespace JhaChallengeTweetStreamer.DAL
{
    /// <summary>
    /// Represents a Message Broker Queue for FIFO handling of messages
    ///    that are received by API Message Stream.  These messages
    ///    are processed by broker logic to pass validated and processed
    ///    business messages and enqueued for down stream processing.
    /// To be replaced by external Message Broker Queue (RabbitMq, 
    ///     Azure Service Bus, etc) for production-scale processing    
    /// </summary>
    public class ReceivedMessageInMemoryQueueProvider : IQueueProvider<ReceivedMessage>
    {
        private Queue<ReceivedMessage> _msgQueue { get; set; }

        public ReceivedMessageInMemoryQueueProvider()
        {
            _msgQueue = new Queue<ReceivedMessage>();
        }

        public void Enqueue(ReceivedMessage message)
        {
            _msgQueue.Enqueue(message);            
        }

        public ReceivedMessage Dequeue()
        {
            return _msgQueue.Dequeue();
        }

        public int Count()
        {
            return _msgQueue.Count;
        }
    }
}
