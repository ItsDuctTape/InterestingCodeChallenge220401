using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;

namespace JhaChallengeTweetStreamer.Services
{

    /// <summary>
    ///   Reads Business Messages from queue and commits them to persistent storage.  
    ///   
    ///   This could be built as a microservice and instances increased to
    ///   meet production-scale processing.
    /// </summary>
    public class MessagePersistorService : BackgroundService
    {
        private readonly ILogger<MessagePersistorService> _logger;
        private readonly IQueueProvider<BusinessMessage> _businessMessages;

        public MessagePersistorService(ILogger<MessagePersistorService> logger
                                       ,IQueueProvider<BusinessMessage> businessMessages
                                        )
        {
            _logger = logger;
            _businessMessages = businessMessages;
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
            while (_businessMessages.Count() > 0)
            {
                PersistCurrentMessagesToPermenantAsync();
                Task.Delay(250, cancellationToken);
            }
            return base.StopAsync(cancellationToken);
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                PersistCurrentMessagesToPermenantAsync();
                await Task.Delay(2000, stoppingToken);
            }
        }

        private void PersistCurrentMessagesToPermenantAsync()
        {
            try
            {
                // Write all Messages Received so far to Permenant Storage
                var queueDepth = _businessMessages.Count();
                for (int i = 0; i < queueDepth; i++)
                {
                    var m = _businessMessages.Dequeue();
                    if (m != null)
                    {
                        // Replace This with Calls to DAL Classes to persist data
                        _logger.LogDebug($"MessagePersistorService Mock Store of: {m.OriginalMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }


    }

}
