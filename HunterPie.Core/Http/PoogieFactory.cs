using HunterPie.Core.Client;
using HunterPie.Core.Domain.Constants;
using HunterPie.Core.Domain.Features;
using System;

namespace HunterPie.Core.Http
{
    public static class PoogieFactory
    {
        const string CLIENT_ID = "X-Client-Id";
        const string APP_VERSION = "X-App-Version";
        const string CLIENT_TYPE = "X-HunterPie-Client";
        const string USER_AGENT = "User-Agent";

        public readonly static string[] Hosts =
        {
            "https://api.hunterpie.com",
            "https://mirror.hunterpie.com/mirror"
        };

        public readonly static string[] Documentation =
        {
            "https://docs.hunterpie.com"
        };

        /// <summary>
        /// Returns the default Http client with default headers and pointing to the default API endpoint
        /// </summary>
        /// <returns>Default HTTP client</returns>
        public static PoogieBuilder Default()
        {
            string clientId = RegistryConfig.Exists("client_id")
                ? RegistryConfig.Get<string>("client_id")
                : "Unknown";

            bool shouldRedirect = FeatureFlagManager.IsEnabled(FeatureFlags.FEATURE_REDIRECT_POOGIE);
            PoogieBuilder builder = shouldRedirect 
                                    ? new PoogieBuilder(ClientConfig.Config.Development.PoogieApiHost)
                                    : new PoogieBuilder(Hosts);

            return builder.WithTimeout(TimeSpan.FromSeconds(5))
                          .WithHeader(CLIENT_ID, clientId)
                          .WithHeader(APP_VERSION, ClientInfo.Version.ToString())
                          .WithHeader(CLIENT_TYPE, "v2")
                          .WithHeader(USER_AGENT, GetUserAgent());
        }

        public static PoogieBuilder Docs()
        {
            return new PoogieBuilder(Documentation);
        }

        private static string GetUserAgent()
        {
            string clientVersion = ClientInfo.Version.ToString();
            string platformVersion = Environment.OSVersion.ToString();
            return $"HunterPie/{clientVersion} ({platformVersion})";
        }
    }
}
