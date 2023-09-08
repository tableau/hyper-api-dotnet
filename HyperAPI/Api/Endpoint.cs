using System;
using System.Collections.Generic;
using System.Text;

using Tableau.HyperAPI.Impl;

namespace Tableau.HyperAPI
{
    /// <summary>
    /// Network endpoint at which a Hyper server is accessible.
    /// </summary>
    /// <remarks>
    /// Use <see cref="HyperProcess"/>'s <see cref="HyperProcess.Endpoint"/>
    /// property to get the endpoint to connect to it.
    /// </remarks>
    public sealed class Endpoint
    {
        /// <summary>
        /// Gets the connection descriptor.
        /// </summary>
        public string ConnectionDescriptor { get; }

        /// <summary>
        /// Gets the user agent string.
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Creates an endpoint object.
        /// </summary>
        /// <param name="connectionDescriptor">A Tableau connection descriptor.</param>
        /// <param name="userAgent">A user agent string which will be used in logging.</param>
        public Endpoint(string connectionDescriptor, string userAgent)
        {
            Util.CheckArgument(!string.IsNullOrEmpty(connectionDescriptor), "connection descriptor must not be empty");
            ConnectionDescriptor = connectionDescriptor;
            UserAgent = userAgent;
        }
    }

}
