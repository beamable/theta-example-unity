namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Models
{
    public class CollectionMetadata
    {
        public string name;
        public string description;
        public string image;
        public string external_link;

        public static CollectionMetadata Construct()
        {
            return new CollectionMetadata
            {
                name = Configuration.CollectionName != "" ? Configuration.CollectionName : "Default collection",
                description = Configuration.CollectionDescription,
                image = Configuration.CollectionImage,
                external_link = Configuration.CollectionLink
            };
        }
    }
}