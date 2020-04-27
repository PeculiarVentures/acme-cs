using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NLog;

namespace PeculiarVentures.ACME.Server.Services
{
    public class BaseService
    {
        protected ILogger Logger { get; } = LogManager.GetLogger("ACME.Service");

        public BaseService(IOptions<ServerOptions> options)
        {
            Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }

        public ServerOptions Options { get; }

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
