using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NLog;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Base service
    /// </summary>
    public class BaseService
    {
        protected ILogger Logger { get; } = LogManager.GetLogger("ACME.Service");

        public BaseService(IOptions<ServerOptions> options)
        {
            Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Server options
        /// </summary>
        public ServerOptions Options { get; }

        /// <summary>
        /// Returns id of kid
        /// </summary>
        /// <param name="kid">URL key</param>
        /// <returns cref="MalformedException"/>
        public int GetKeyIdentifier(string kid)
        {
            var match = new Regex("([0-9]+)$").Match(kid);
            if (!match.Success)
            {
                throw new MalformedException("Cannot get key identifier from the 'kid'");
            }
            return int.Parse(match.Groups[1].Value);
        }
    }
}
