using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Nethereum.KeyStore.Model;

namespace Beamable.Microservices.ThetaFederation
{
    internal static class Configuration
    {
        private const string ConfigurationNamespace = "theta_federation";

        public static readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET");
        public static RealmConfig RealmConfig { get; internal set; }

        public static string RPCEndpoint => GetValue(nameof(RPCEndpoint), "");
        public static bool AllowManagedAccounts => GetValue(nameof(AllowManagedAccounts), true);
        public static int AuthenticationChallengeTtlSec => GetValue(nameof(AuthenticationChallengeTtlSec), 600);
        public static int ReceiptPoolIntervalMs => GetValue(nameof(ReceiptPoolIntervalMs), 200);
        public static int ReceiptPoolTimeoutMs => GetValue(nameof(ReceiptPoolTimeoutMs), 20000);
        public static int TransactionRetries => GetValue(nameof(TransactionRetries), 10);
        public static int MaximumGas => GetValue(nameof(MaximumGas), 2_000_000);
        public static int GasPriceCacheTtlSec => GetValue(nameof(GasPriceCacheTtlSec), 3);
        public static int GasExtraPercent => GetValue(nameof(GasExtraPercent), 0);

        public static string CollectionName => GetValue(nameof(CollectionName), "");
        public static string CollectionDescription => GetValue(nameof(CollectionDescription), "");
        public static string CollectionImage => GetValue(nameof(CollectionImage), "");
        public static string CollectionLink => GetValue(nameof(CollectionLink), "");

        public static ScryptParams GetPlayerWalletScryptParams()
        {
            var n = GetValue("PlayerWalletScryptParams_N", 65536);
            return new() {Dklen = 32, N = n, R = 1, P = 8};
        }

        private static T GetValue<T>(string key, T defaultValue) where T : IConvertible
        {
            var namespaceConfig = RealmConfig.GetValueOrDefault(ConfigurationNamespace) ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            var value = namespaceConfig.GetValueOrDefault(key);
            if (value is null)
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }

    class ConfigurationException : MicroserviceException
    {
        public ConfigurationException(string message) : base((int)HttpStatusCode.BadRequest, "ConfigurationError", message)
        {
        }
    }
}