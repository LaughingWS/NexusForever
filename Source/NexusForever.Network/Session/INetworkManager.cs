﻿using NexusForever.Shared;

namespace NexusForever.Network.Session
{
    public interface INetworkManager<T> : IEnumerable<T>, IUpdate where T : class, INetworkSession
    {
        /// <summary>
        /// Initialise <see cref="INetworkManager{T}"/> and any related resources.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Start <see cref="INetworkManager{T}"/> and any related resources.
        /// </summary>
        void Start();

        /// <summary>
        /// Shutdown <see cref="INetworkManager{T}"/> and any related resources.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Update session existing id with a new supplied id.
        /// </summary>
        /// <remarks>
        /// This should be used when the default session id can be replaced with a known unique id.
        /// </remarks>
        void UpdateSessionId(T session, string id);

        /// <summary>
        /// Return session with supplied id.
        /// </summary>
        T GetSession(string id);

        /// <summary>
        /// Return session that meets supplied predicate.
        /// </summary>
        /// <remarks>
        /// An exception will be thrown if multiple sessions meet the predicate.
        /// </remarks>
        T GetSession(Func<T, bool> func);
    }
}