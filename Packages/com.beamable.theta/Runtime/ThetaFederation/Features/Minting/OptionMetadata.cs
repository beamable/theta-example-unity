namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    public class OptionMetadata
    {
        public string ContentId { get; }
        public string OptionDisplayName { get; }
        public bool IsNft { get; }
        public bool AccountBound { get; }

        public OptionMetadata(string contentId, string optionDisplayName, bool isNft, bool accountBound)
        {
            ContentId = contentId;
            OptionDisplayName = optionDisplayName;
            IsNft = isNft;
            AccountBound = accountBound;
        }
    }
}