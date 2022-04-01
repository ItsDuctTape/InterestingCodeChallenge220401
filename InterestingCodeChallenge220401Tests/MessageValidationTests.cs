using System;
using JhaChallengeTweetStreamer.Model;
using JhaChallengeTweetStreamer.Core;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace JhaChallengeTweetStreamer.Tests
{
    public class MessageValidationTests
    {

        [Theory]
        [InlineData("{\"data\":{\"id\":\"1509299071248441344\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"1509299071252635651\",\"text\":\"More benign tweet message text with url http://www.google.com\"}}")]
        public void MessageWithValidId_ShouldPass(string msg)
        {
            // Arrange & Act
            var validator = new TwitterV2ApiSampleStreamMessageValidator();
            bool actual = validator.MessageIsValid(msg);

            // Assert
            actual.Should().Be(true);
        }

        [Theory]
        [InlineData("{\"data\":{\"id\":\"-1\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"\",\"text\":\"RT @AUser: Some benign tweet message text\"}}")]
        [InlineData("{\"data\":{\"id\":\"0\",\"text\":\"More benign tweet message text with url http://www.google.com\"}}")]
        public void MessageWithInvalidId_ShouldFail(string msg)
        {
            // Arrange & Act
            var validator = new TwitterV2ApiSampleStreamMessageValidator();
            bool actual = validator.MessageIsValid(msg);

            // Assert
            actual.Should().Be(false);
        }


    }
}
