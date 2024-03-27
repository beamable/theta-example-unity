using Beamable.Common.Dependencies;
using Beamable.Microservices.ThetaFederation;
using Beamable.Server.Editor;

namespace Beamable.Editor.Theta.Hooks
{
    public class ThetaFederationBuildHook : IMicroserviceBuildHook<ThetaFederation>
    {
        const string SourceBasePath = "Packages/com.beamable.theta/Runtime/ThetaFederation";
        
        public void Execute(IMicroserviceBuildContext ctx)
        {
            ctx.AddDirectory($"{SourceBasePath}/Solidity", "Solidity");
        }
    }
    
    [BeamContextSystem]
    public class Registrations
    {
        [RegisterBeamableDependencies(-1, RegistrationOrigin.EDITOR)]
        public static void Register(IDependencyBuilder builder)
        {
            builder.AddSingleton<IMicroserviceBuildHook<ThetaFederation>, ThetaFederationBuildHook>();
        }
    }
}