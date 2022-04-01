using System.Threading;
using System.Threading.Tasks;

namespace JhaChallengeTweetStreamer.Interfaces
{
    /// <summary>
    /// Generic definition of any Class that connects to a Streamed API service endpoint
    ///    to retrieve messages
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IApiMessageStreamRetriever<TEntity> where TEntity : class
    {
        bool Initialize(string apiUrl, string apiEndpoint, string bearerToken);

        Task EnqueueRetrievedMessageStreamAsync(CancellationToken cancellationToken, IQueueProvider<TEntity> messageQueue);

        void Dispose();
    }
}
