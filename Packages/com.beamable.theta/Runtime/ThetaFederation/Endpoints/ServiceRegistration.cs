using Beamable.Common.Dependencies;

namespace Beamable.Microservices.ThetaFederation.Endpoints
{
    public static class ServiceRegistration
    {
        public static void AddEndpoints(this IDependencyBuilder builder)
        {
            builder.AddScoped<AuthenticateEndpoint>();
            builder.AddScoped<GetRealmAccountEndpoint>();
            builder.AddScoped<GetContractAddressEndpoint>();
            builder.AddScoped<GetInventoryStateEndpoint>();
            builder.AddScoped<StartInventoryTransactionEndpoint>();
        }
    }
}