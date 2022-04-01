using System;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Core;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using JhaChallengeTweetStreamer.Interfaces;

namespace JhaChallengeTweetStreamer.Tests
{
    public class ConnectionTests
    {
        private readonly Mock<ILogger<TwitterV2ApiSampleStreamRetriever>> _logger;
        private readonly Mock<IQueueProvider<ReceivedMessage>> _receivedMsgQueue;
        
        public ConnectionTests()
        {
            _logger = new Mock<ILogger<TwitterV2ApiSampleStreamRetriever>>();
            _receivedMsgQueue = new Mock<IQueueProvider<ReceivedMessage>>();
        }

        [Theory]
        [InlineData("{\"title\":\"ConnectionException\",\"detail\":\"This stream is currently at the maximum allowed connection limit.\",\"connection_issue\":\"TooManyConnections\",\"type\":\"https://api.twitter.com/2/problems/streaming-connection\"}")]
        public void ConnectionException_ShouldBeRecognized(string msg)
        {
            // Arrange
            var retreiver = new TwitterV2ApiSampleStreamRetriever(_logger.Object, _receivedMsgQueue.Object);

            // Act
            bool actual = retreiver.RecievedSpecialApiConnectionMessage(msg);

            // Assert
            actual.Should().Be(true);
        }


    }
}
