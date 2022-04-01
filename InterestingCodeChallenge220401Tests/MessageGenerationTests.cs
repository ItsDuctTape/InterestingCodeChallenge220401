using System;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Core;
using Xunit;
using FluentAssertions;

namespace JhaChallengeTweetStreamer.Tests
{
    public class MessageGenerationTests
    {
        [Theory]
        [InlineData("{\"data\":{\"id\":\"1509299071248441344\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"1509299071252635651\",\"text\":\"More benign tweet message text with url http://www.google.com\"}}")]
        public void GenerateReceivedMessageWithValidMessage_ShouldParseId(string msg)
        {
            // Arrange & Act
            var generator = new JhaChallengeTweetStreamer.Core.TwitterV2ApiBusinessMessageGenerator();
            var receivedMsg = new ReceivedMessage(DateTime.Now, msg);
            BusinessMessage actual = generator.BuildBusinessMessageFromApiMessage(receivedMsg);

            // Assert
            actual.Id.Should().BeGreaterThan(0,"Id Not Parsed");
        }

        [Theory]
        [InlineData("{\"data\":{\"id\":\"1509299071248441344\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"1509299071252635651\",\"text\":\"More benign tweet message text with url http://www.google.com\"}}")]
        [InlineData("{\"data\":{\"id\":\"1509299071248441344\",\"text\":\"test\"}}")]
        public void GenerateReceivedMessageWithValidMessage_ShouldParseText(string msg)
        {
            // Arrange & Act
            var generator = new JhaChallengeTweetStreamer.Core.TwitterV2ApiBusinessMessageGenerator();
            var receivedMsg = new ReceivedMessage(DateTime.Now, msg);
            BusinessMessage actual = generator.BuildBusinessMessageFromApiMessage(receivedMsg);

            // Assert
            actual.Payload.Length.Should().BeGreaterThan(0, "Payload is zero length or null");
        }


        [Theory]
        [InlineData("{\"data\":{\"id\":\"-1\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"\",\"text\":\"\"}}")]
        [InlineData("{\"data\":{\"id\":\"0\",\"text\":\"More benign tweet message text with url http://www.google.com\"}}")]
        [InlineData("{\"data\":{\"id\":\"1509299071248441344\",\"text\":\"\"}}")]
        public void GenerateReceivedMessageWithInvalidText_ShouldFail(string msg)
        {
            // Arrange & Act
            var generator = new JhaChallengeTweetStreamer.Core.TwitterV2ApiBusinessMessageGenerator();
            var receivedMsg = new ReceivedMessage(DateTime.Now, msg);
            BusinessMessage actual = generator.BuildBusinessMessageFromApiMessage(receivedMsg);

            // Assert
            Assert.Null(actual);
        }


    }
}
