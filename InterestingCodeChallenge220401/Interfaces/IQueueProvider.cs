namespace JhaChallengeTweetStreamer.Interfaces
{
    /// <summary>
    ///  Generic Interface representing any type of Message Broker that
    ///   implements queue data structure logic
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueueProvider<TEntity> where TEntity : class
    {
        TEntity Dequeue();

        void Enqueue(TEntity o);

        int Count();
    }
}