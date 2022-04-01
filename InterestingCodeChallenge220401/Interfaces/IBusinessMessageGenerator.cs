using JhaChallengeTweetStreamer.Model;

namespace JhaChallengeTweetStreamer.Interfaces
{
    /// <summary>
    /// Generic definition of any class that is responsible
    ///  to convert received text-based messages into proper
    ///  business logic messages.
    /// </summary>
    public interface IBusinessMessageGenerator
    {
        BusinessMessage BuildBusinessMessageFromApiMessage(ReceivedMessage message);

        BusinessMessageMetadata BuildBusinessMessageMetadataFromApiMessage(ReceivedMessage message, bool isValid, long messageId);
    }
}
