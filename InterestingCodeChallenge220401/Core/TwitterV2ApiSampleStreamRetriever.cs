using JhaChallengeTweetStreamer.Interfaces;
using JhaChallengeTweetStreamer.Model;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JhaChallengeTweetStreamer.Core
{
    /// <summary>
    ///  Performs connection to Twitter v2 API and retrieves messages.  
    ///  Watches for connection error messages.
    /// </summary>
    public class TwitterV2ApiSampleStreamRetriever : IApiMessageStreamRetriever<ReceivedMessage>
    {
        private readonly ILogger<TwitterV2ApiSampleStreamRetriever> _logger;
        private readonly IQueueProvider<ReceivedMessage> _receivedMessages;
        private RestClient _restClient;

        private string _apiUrl;
        private string _apiEndpoint;
        private string _bearerToken;

        public TwitterV2ApiSampleStreamRetriever(ILogger<TwitterV2ApiSampleStreamRetriever> logger,
                                                IQueueProvider<ReceivedMessage> receivedMessages)
        {
            _logger = logger;
            _receivedMessages = receivedMessages;
        }

        public bool Initialize(string apiUrl, string apiEndpoint, string bearerToken)
        {
            try
            {
                _logger.LogInformation("   ... is Initializing at: {time}", DateTimeOffset.Now);
                _apiUrl = apiUrl;
                _apiEndpoint = apiEndpoint;
                _bearerToken = bearerToken;
                JwtAuthenticator authentication = new JwtAuthenticator(_bearerToken);
                _restClient = new RestClient(_apiUrl);
                _restClient.Authenticator = authentication;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Initialize() Error :: {ex.Message}");
            }
            return false;
        }




        /// <summary>
        /// 1) Establish Connection to Twitter v2 API
        /// 2) Open Sample Stream
        /// 3) Enqueue Messages with Timestamp for downstream processing
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <param name="messageQueue"></param>
        /// <returns></returns>
        public async Task EnqueueRetrievedMessageStreamAsync(CancellationToken stoppingToken, IQueueProvider<ReceivedMessage> messageQueue)
        {
            try
            {
                var request = new RestRequest(_apiEndpoint) { CompletionOption = HttpCompletionOption.ResponseHeadersRead };
                var stream = await _restClient.DownloadStreamAsync(request);
                var reader = new StreamReader(stream!);
                while (!reader.EndOfStream && !stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var content = await reader.ReadLineAsync();
                        if (!String.IsNullOrEmpty(content))
                        {
                            if (RecievedSpecialApiConnectionMessage(content))
                            {
                                throw new Exception(content);
                            }

                            //  Enqueue the raw response content without any validation or processing
                            //    This Message Queue will be processed by other services
                            _receivedMessages.Enqueue(
                                new ReceivedMessage()
                                {
                                    ReceivedTimestamp = DateTime.Now,
                                    Message = content
                                });
                        }
                    }
                    catch (Exception exInner)
                    {
                        _logger.LogError($"EnqueueRetrievedMessageStreamAsync(A) :: {exInner.Message}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EnqueueRetrievedMessageStreamAsync(B) Error :: {ex.Message}");
            }
            return;
        }


        #region  .... Alternate Stream Connection Method ....

        /// <summary>
        /// Alternate method of reading stream.  Not used for this exercise.
        /// 
        /// Attempting to get HTTP Response Status, but not yet able to get 
        /// codes with RestSharp streaming.  Should be possible... just need
        /// more time.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <param name="messageQueue"></param>
        /// <returns></returns>
        public async Task EnqueueRetrievedMessageStreamAlternateAsync(CancellationToken stoppingToken, IQueueProvider<ReceivedMessage> messageQueue)
        {
            try
            {
                await foreach(var response in StreamSamples(stoppingToken))
                {
                    //  Enqueue the raw response content without any validation or processing
                    //    This Message Queue will be processed by other services
                    _receivedMessages.Enqueue(
                        new ReceivedMessage()
                        {
                            ReceivedTimestamp = DateTime.Now,
                            Message = response.ToString()
                        });
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError($"EnqueueRetrievedMessageStreamAsync(B) Error :: {ex.Message}");
            }
            return;
        }

        public async IAsyncEnumerable<SampleResponse> StreamSamples([EnumeratorCancellation] CancellationToken stoppingToken = default)
        {
            var response = _restClient.StreamJsonAsync<TwitterSingleObject<SampleResponse>>(_apiEndpoint, stoppingToken);
            await foreach (var item in response.WithCancellation(stoppingToken))
            {
                yield return item.Data;               
            }
        }

        record TwitterSingleObject<T>(T Data);

        public record SampleResponse(string Id, string Text);
        
        #endregion


        /// <summary>
        ///   Method determines if any special messages were returned by the API endpoint
        ///     that would prevent normal processing.  The hard-coded text strings should
        ///     be moved to a configuration system of some sort since there is a possibiltiy
        ///     that the endpoint may send differing and new messages over time.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool RecievedSpecialApiConnectionMessage(string content)
        {
            if (content.Contains("\"title\":\"ConnectionException\"") && 
                content.Contains("\"connection_issue\":\""))
                return true;

            if (content.Contains(",\"status\":40"))
                return true;

            if (content.Contains(",\"status\":50"))
                return true;

            return false;
        }

        public void Dispose()
        {
            _restClient.Dispose();
        }

       

    }
}
