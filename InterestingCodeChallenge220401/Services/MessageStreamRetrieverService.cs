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
    ///   Coordinates the retrieval of messages from API Stream
    ///   by calling specialized class that is designed for the
    ///   appropriate, specific API Endpoint/Provider
    ///   
    ///   This service logic is scalable by specialized account
    ///   permissions at Twitter which enables multiple connections
    ///   and historical retreival of messages up to 5 minutes in 
    ///   the past.
    /// </summary>
    public class MessageStreamRetrieverService : BackgroundService
    {
        private readonly ILogger<MessageStreamRetrieverService> _logger;        
        private readonly IQueueProvider<ReceivedMessage> _receivedMessages;
        private readonly IApiMessageStreamRetriever<ReceivedMessage> _messageStreamRetriever;
        private readonly int _reconnectDelaySeconds = 10;

        public MessageStreamRetrieverService(ILogger<MessageStreamRetrieverService> logger,                                                
                                             IApiMessageStreamRetriever<ReceivedMessage> messageStreamRetriever,
                                             IQueueProvider<ReceivedMessage> receivedMessages
                                             )
        {
            _logger = logger;            
            _messageStreamRetriever = messageStreamRetriever;
            _receivedMessages = receivedMessages;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("   ...is starting at: {time}", DateTimeOffset.Now);
                _messageStreamRetriever.Initialize(Program.AppSettings.ApiUrl, 
                                                   Program.AppSettings.ApiEndpoint, 
                                                   Program.AppSettings.BearerToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"StartAsync() Error during Start: {ex.Message}");
            }
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("   ... is stopping at: {time}", DateTimeOffset.Now);
            _messageStreamRetriever.Dispose();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("   ...is Executing at: {time}", DateTimeOffset.Now);
                try
                {
                    await _messageStreamRetriever.EnqueueRetrievedMessageStreamAsync(stoppingToken, _receivedMessages);
                }
                catch (Exception ex)
                {
                    _logger.LogError($" ExecuteAsync() :: {ex.ToString()}");                    
                }                
                return;
            }
        }




    }

}
