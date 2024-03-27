namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    public class UpdateMetadataRequest
    {
        public uint TokenId { get; set; }
        public NftExternalMetadata Metadata { get; set; } = null!;
    }
}