using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Composition.Models
{
    [Function("compose")]
    public class ComposeFunctionMessage : FunctionMessage
    {
        [Parameter("address", "owner")]
        public virtual string Owner { get; set; } = null!;

        [Parameter("uint256", "tokenId", 2)]
        public virtual uint TokenId { get; set; }

        [Parameter("uint256[]", "attachIds", 3)]
        public virtual List<uint> AttachIds { get; set; } = new();

        [Parameter("uint256[]", "detachIds", 4)]
        public virtual List<uint> DetachIds { get; set; } = new();
    }
}