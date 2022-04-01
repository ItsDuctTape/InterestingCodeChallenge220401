namespace JhaChallengeTweetStreamer.Interfaces
{
    /// <summary>
    /// Generic definition of any class that is responsible
    ///  validating received text-based messages.
    /// </summary>
    public interface IMessageValidator
    {
        bool MessageIsValid(string messageContent);

    }
}
