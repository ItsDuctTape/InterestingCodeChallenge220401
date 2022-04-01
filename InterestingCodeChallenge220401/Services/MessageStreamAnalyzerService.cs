using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Interfaces;

namespace JhaChallengeTweetStreamer.Services
{
    /// <summary>
    ///   Reads Metadata Messages from queue and performs analysis of the API stream.  
    /// </summary>
    public class MessageStreamAnalyzerService : BackgroundService
    {
        private readonly ILogger<MessageStreamAnalyzerService> _logger;
        private readonly IQueueProvider<BusinessMessageMetadata> _businessMessageMetadata;
        private long _totalBytesReceived;
        private int _totalMsgsReceived;
        private int _invalidMessageCount;
        private List<BusinessMessageMetadata> _recentMessages;
        private DateTime _startTime;

        public MessageStreamAnalyzerService(ILogger<MessageStreamAnalyzerService> logger,
                                            IQueueProvider<BusinessMessageMetadata> businessMessageMetadata
                                            )
        {
            _logger = logger;
            _businessMessageMetadata = businessMessageMetadata;            
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("   ...is starting at: {time}", DateTimeOffset.Now);
            _recentMessages = new List<BusinessMessageMetadata>();
            _totalBytesReceived = 0;
            _startTime = DateTime.Now;
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("   ...is stopping at: {time}", DateTimeOffset.Now);
            // Ensure all messages in the queue are processed before shutting down
            while (_businessMessageMetadata.Count() > 0)
            {
                AnalyzeReceivedMessages();
                Task.Delay(250, cancellationToken);
            }
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool waitingForData = true;
            await Task.Delay(1000, stoppingToken);
            Console.WriteLine(" ");
            Console.Write("  Waiting for data .");
            while (!stoppingToken.IsCancellationRequested)
            {                
                if (_businessMessageMetadata.Count() > 0)
                {
                    if (waitingForData)
                    {
                        waitingForData = false;
                        Console.WriteLine(".");
                        Console.WriteLine(" ");
                    }
                    GetEnqueuedMessages();
                    RemoveOldMessages();
                    AnalyzeReceivedMessages();
                }
                else
                {
                    Console.Write(".");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void GetEnqueuedMessages()
        {
            var currentQueueDepth = _businessMessageMetadata.Count();
            for (int i = 0; i < currentQueueDepth; i++)
            {
                var metadatum = _businessMessageMetadata.Dequeue();
                if (metadatum != null)
                {
                    _recentMessages.Add(metadatum);
                    _totalBytesReceived += metadatum.DataLength;
                    _totalMsgsReceived++;
                    if (!metadatum.IsValid)
                        _invalidMessageCount++;
                }
            }
        }

        private void RemoveOldMessages()
        {
            DateTime datetimeThreshold = DateTime.Now.AddMinutes(-1);
            _recentMessages.RemoveAll(m => m.ReceivedTimestamp < datetimeThreshold);
        }

        private void AnalyzeReceivedMessages()
        {               
            var elapsedTimeSpan = DateTime.Now - _startTime;
            var msgCount = _recentMessages.Count;
            var msgsPerSecondAvg = _totalMsgsReceived / elapsedTimeSpan.TotalSeconds;                 
            var msgsPerMinuteAvg = _totalMsgsReceived / (elapsedTimeSpan.TotalSeconds / 60);
            var totalKBReceived = _totalBytesReceived / 1024.0;

            Console.WriteLine($"Elapsed: {elapsedTimeSpan.TotalSeconds.ToString("0").PadLeft(3)} seconds." +
                              $"  |  Total Msgs: {_totalMsgsReceived.ToString().PadLeft(5)}" +
                              $"  |  Invalid Msgs: {_invalidMessageCount.ToString().PadLeft(5)}" +
                              $"  |  Past 1 min Msgs: {msgCount.ToString().PadLeft(5)}" +
                              $"  |  Avg. Rate: {msgsPerMinuteAvg.ToString("0.0").PadLeft(6)}/min" +
                              $"  |  Avg. Rate: {msgsPerSecondAvg.ToString("0.0").PadLeft(4)}/sec" +
                              $"  |  Total Data: {totalKBReceived.ToString("0.0").PadLeft(6)} KB");

        }



    }
}
