using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Beamable.Microservices.ThetaFederation.Features.SolcWrapper.Models
{
    public class SolidityCompilerInput
    {
        public SolidityCompilerInput(string contractFile, IEnumerable<string> contractOutputSelection)
        {
            Sources = new InputSource
            {
                Contract = new InputSource.InputContract
                {
                    ContractUrls = new List<string> { contractFile }
                }
            };
            Settings = new OutputSettings
            {
                OutputSelection = new OutputSettings.Selection
                {
                    Contract = new Dictionary<string, List<string>>
                    {
                        { "*", contractOutputSelection.ToList() }
                    }
                }
            };
        }

        [JsonProperty("language")]
        public string Language { get; set; } = "Solidity";

        [JsonProperty("sources")]
        public InputSource Sources { get; set; }

        [JsonProperty("settings")]
        public OutputSettings Settings { get; set; }

        public class InputSource
        {
            [JsonProperty("contract")]
            public InputContract Contract { get; set; } = null!;

            public class InputContract
            {
                [JsonProperty("urls")]
                public List<string> ContractUrls { get; set; } = null!;
            }
        }

        public class OutputSettings
        {
            [JsonProperty("outputSelection")]
            public Selection OutputSelection { get; set; } = null!;

            [JsonProperty("evmVersion")]
            public string EvmVersion { get; set; } = "paris"; //Theta specific

            public class Selection
            {
                [JsonProperty("contract")]
                public IDictionary<string, List<string>> Contract { get; set; } = null!;
            }
        }
    }
}