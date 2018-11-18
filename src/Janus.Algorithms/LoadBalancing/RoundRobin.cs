using System.Threading.Tasks;
using Janus.Core.Algorithms;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System;
using System.Runtime.Serialization;

namespace Janus.Algorithms.LoadBalancing
{
    public class RoundRobin<T> : IRouteAlgorithm<T>, IDisposable
    {

        #region [Fields]

        protected ConcurrentDictionary<int, T> entities;
        protected ReaderWriterLockSlim entityLock;
        protected int nextIndex;
        protected int[] keys;

        #endregion

        #region [Construction/Destruction]

        public RoundRobin()
        {
            this.entities = new ConcurrentDictionary<int, T>();
            this.entityLock = new ReaderWriterLockSlim();
            this.nextIndex = 0;
        }

        public RoundRobin(IEnumerable<T> entities, int startAt=0)
        {
            this.entityLock = new ReaderWriterLockSlim();
            this.entities = new ConcurrentDictionary<int, T>(entities.ToDictionary(x=>x.GetHashCode()));
            this.keys = this.entities.Keys.ToArray();

            if (startAt > this.keys.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(startAt));
            this.nextIndex = startAt;
        }

        ~RoundRobin()
        {
            if (this.entityLock != null)
                this.entityLock.Dispose();
        }

        #endregion

        #region [IDisposable]

        public void Dispose()
        {
            if (this.entityLock != null)
                this.entityLock.Dispose();
        }

        #endregion

        #region [IRouteAlgorithm]

        public virtual Task<T> AddRoutedEntity(T routedEntity) => Task.FromResult(
            this.entities.GetOrAdd(routedEntity.GetHashCode(), routedEntity)
        );

        public virtual Task<T> GetNext()
        {
            throw new System.NotImplementedException();
        }

        public virtual Task<T> InsertRoutedEntity(T routedEntity, int index)
        {
            throw new System.NotImplementedException();
        }

        public virtual Task RemoveRoutedEntity(T routedEntity)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveRoutedEntityAt(int index)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }


    public class NothingToRouteException : Exception
    {
        public NothingToRouteException() : base("No routed entities available!") { }

        public NothingToRouteException(string message) : base(message) { }

        public NothingToRouteException(string message, Exception innerException) : base(message, innerException) { }

        public NothingToRouteException(SerializationInfo info, StreamingContext streamingContext) : base(info, streamingContext) { }
    }
}