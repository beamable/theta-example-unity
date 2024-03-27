using System.Linq;
using System.Reflection;
using Beamable.Common.Dependencies;
using Beamable.Microservices.ThetaFederation.Extensions;
using Beamable.Microservices.ThetaFederation.Features.Transactions;

namespace Beamable.Microservices.ThetaFederation
{
    public static class ServiceRegistration
    {
        public static void AddFeatures(this IDependencyBuilder builder)
        {
            Assembly.GetExecutingAssembly()
                .GetDerivedTypes<IService>()
                .ToList()
                .ForEach(serviceType => builder.AddSingleton(serviceType));

            builder.AddScoped<TransactionManager>();
        }
    }
}