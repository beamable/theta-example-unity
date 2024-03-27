using Beamable.Server;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class ExtensionsSetup
    {
        public static void SetupExtensions(this IServiceInitializer initializer)
        {
            initializer.SetupMongoExtensions();
        }
    }
}