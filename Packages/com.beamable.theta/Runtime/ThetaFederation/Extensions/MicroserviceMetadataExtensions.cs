using System;
using System.Collections.Concurrent;
using System.Reflection;
using Beamable.Common;
using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class MicroserviceMetadataExtensions
    {
        private static readonly ConcurrentDictionary<Type, MicroserviceInfo> Cache = new();

        public static MicroserviceInfo GetMetadata<TService, TIdentity>()
            where TService : Microservice, IFederatedLogin<TIdentity>, new()
            where TIdentity : IThirdPartyCloudIdentity, new()
        {
            return Cache.GetOrAdd(typeof(TService), _ =>
            {
                var microservice = new TService();
                var microserviceName = microservice.GetType().GetCustomAttribute<MicroserviceAttribute>()!.MicroserviceName;
                var identity = new TIdentity();
                var microserviceNamespace = identity.UniqueName;

                return new MicroserviceInfo(microserviceName, microserviceNamespace);
            });
        }
    }

    public class MicroserviceInfo
    {
        public string MicroserviceName { get; set; }
        public string MicroserviceNamespace { get; set; }

        public MicroserviceInfo(string microserviceName, string microserviceNamespace)
        {
            MicroserviceName = microserviceName;
            MicroserviceNamespace = microserviceNamespace;
        }
    }
}