using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JhaChallengeTweetStreamer.Services
{

    /// <summary>
    ///   This class acts as a type of Message Broker.  
    ///   Reads Messages from retrieval cache, performs basic formating,
    ///   and enqueues appropriate message type into message queues 
    ///   for downstream services to perform business logic
    ///   and persisting to storage.
    ///   
    ///   This step in the architecture decouples retreival from 
    ///   processing. Using queues would allow for production-scale
    ///   performance by moving various services to microservices
    ///   and instantiating as many as needed to meet message rates.
    /// </summary>
    public class MessageCacheProcessorService : BackgroundService
    {
        private readonly ILogger<MessageCacheProcessorService> _logger;
        private readonly IQueueProvider<ReceivedMessage> _receivedMessages;
        private readonly IQueueProvider<BusinessMessage> _businessMessages;
        private readonly IQueueProvider<BusinessMessageMetadata> _businessMessageMetadata;
        private readonly IMessageValidator _messageValidator;
        private readonly IBusinessMessageGenerator _messageGenerator;

        public MessageCacheProcessorService( ILogger<MessageCacheProcessorService> logger
                                            ,IQueueProvider<ReceivedMessage> receivedMessages
                                            ,IQueueProvider<BusinessMessage> businessMessages
                                            ,IQueueProvider<BusinessMessageMetadata> businessMessageMetadata
                                            ,IMessageValidator validationValidator
                                            ,IBusinessMessageGenerator messageGenerator
                                            )
        {
            _logger = logger;
            _messageValidator = validationValidator;
            _receivedMessages = receivedMessages;
            _businessMessages = businessMessages;
            _businessMessageMetadata = businessMessageMetadata;
            _messageGenerator = messageGenerator;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("   ...is starting at: {time}", DateTimeOffset.Now);
            return base.StartAsync(cancellationToken);
        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("   ...is stopping at: {time}", DateTimeOffset.Now);
            // Ensure all messages in the queue are processed before shutting down
            while (_receivedMessages.Count() > 0)
            {
                ProcessReceivedMessages();
                Task.Delay(250, cancellationToken);
            }            
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Read Messages from Cache and Raise Events to Subscribers to perform Separate, Distinct Logic
                ProcessReceivedMessages();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void ProcessReceivedMessages()
        {
            var currentQueueDepth = _receivedMessages.Count();
            for(int i = 0; i < currentQueueDepth; i++)
            {
                ReceivedMessage receivedMessage = null;
                try
                {
                    receivedMessage = _receivedMessages.Dequeue();
                    if (!String.IsNullOrEmpty(receivedMessage.Message))
                    {
                        bool isValidBusinessMessage = false;
                        long messageId = 0;

                        // Only want to Store Valid Messages to Permenant storage
                        if (_messageValidator.MessageIsValid(receivedMessage.Message))
                        {
                            isValidBusinessMessage = true;
                            var busMsg = _messageGenerator.BuildBusinessMessageFromApiMessage(receivedMessage);
                            messageId = busMsg.Id;
                            _businessMessages.Enqueue(busMsg);
                        }
                        else
                        {
                            // In real-world scenario, we would want to persist
                            // invalid message to storage for some other type of 
                            // review or processing
                            isValidBusinessMessage = false;
                            throw new Exception("Invalid Message Structure.  Unable to Deserialize Data.");
                        }

                        // We still want to monitor total system performance and messaging rates
                        //  so regardless of the validity, we will enqueue metadata for analysis
                        var busMsgMeta = _messageGenerator.BuildBusinessMessageMetadataFromApiMessage(receivedMessage, 
                                                                                                      isValidBusinessMessage, 
                                                                                                      messageId);
                        _businessMessageMetadata.Enqueue(busMsgMeta);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ProcessReceivedMessages() Message >>  {receivedMessage.Message}  << ERROR :: {ex.ToString()}");
                }
            }            
        }

        
    }

}
