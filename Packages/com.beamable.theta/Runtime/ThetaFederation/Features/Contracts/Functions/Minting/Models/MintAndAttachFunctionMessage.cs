using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Minting.Models
{
    [Function("mintAndAttach")]
    public class MintAndAttachFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; } = null!;

        [Parameter("uint256", "tokenId", 2)]
        public virtual uint TokenId { get; set; }

        [Parameter("uint256", "amount", 3)]
        public virtual uint Amount { get; set; }

        [Parameter("string", "metadataHash", 4)]
        public virtual string MetadataHash { get; set; } = null!;

        [Parameter("uint256[]", "attachIds", 5)]
        public virtual List<uint> AttachIds { get; set; } = new();
    }
}