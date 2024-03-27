using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Beamable.Api.Autogenerated.Models;
using Beamable.Api.Autogenerated.Content;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.Content;
using Beamable.Microservices.ThetaFederation.Features.Contracts;
using Beamable.Microservices.ThetaFederation.Features.Contracts.Functions.Metadata.Models;
using Beamable.Microservices.ThetaFederation.Features.Minting.Exceptions;
using Beamable.Microservices.ThetaFederation.Features.Minting.Json;
using Beamable.Microservices.ThetaFederation.Features.Minting.Storage;
using Beamable.Server.Api.Content;
using Newtonsoft.Json;
using TaskExtensions = Beamable.Microservices.ThetaFederation.Extensions.TaskExtensions;

namespace Beamable.Microservices.ThetaFederation.Features.Minting
{
    public class MetadataService : IService
    {
        private readonly IContentApi _contentApi;
        private readonly MintCollection _mintCollection;
        private readonly IMicroserviceContentApi _contentService;
        private readonly HttpClient _httpClient = new();

        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            Converters = { new ObjectAsPrimitiveConverter() }
        };

        public MetadataService(IContentApi contentApi, MintCollection mintCollection)
        {
            _contentApi = contentApi;
            _mintCollection = mintCollection;
        }

        public async Task<string> GetBaseUri()
        {
            var binaryResponse = await _contentApi.PostBinary(new SaveBinaryRequest
            {
                binary = new[]
                {
                    new BinaryDefinition
                    {
                        id = "metadata",
                        checksum = "01",
                        uploadContentType = "plain/text"
                    }
                }
            });

            var uri = new Uri(binaryResponse.binary.First().uri);

            // Remove the last segment
            var segments = uri.Segments.Take(uri.Segments.Length - 1);
            var baseUriString = $"{uri.Scheme}://{uri.Host}{string.Concat(segments)}";

            return baseUriString;
        }

        public async Task<string> SaveMetadata(NftExternalMetadata metadata)
        {
            var uriString = await StoreExternalMetadata(metadata);
            BeamableLogger.Log("Metadata saved at {uri}", uriString);
            var uri = new Uri(uriString);
            return uri.Segments.Last();
        }

        public async Task<SetTokenMetadataHashesFunctionMessage> StoreNewMetadata(IList<UpdateMetadataRequest> requests)
        {
            var tokenIds = new List<uint>();
            var tokenMetadataHashes = new List<string>();


            foreach (var request in requests)
            {
                var metadataHash = await SaveMetadata(request.Metadata);
                tokenIds.Add(request.TokenId);
                tokenMetadataHashes.Add(metadataHash);
            }

            return new SetTokenMetadataHashesFunctionMessage
            {
                TokenIds = tokenIds,
                MetadataHashes = tokenMetadataHashes
            };
        }

        public async Task<string> StoreExternalMetadata(NftExternalMetadata metadata)
        {
            var metadataJsonString = JsonConvert.SerializeObject(metadata, JsonSerializerSettings);
            return await StoreExternalMetadata(metadataJsonString);
        }

        public async Task<string> StoreExternalMetadata(string metadataJson)
        {
            var metadataPayload = Encoding.UTF8.GetBytes(metadataJson);

            using (var md5 = MD5.Create())
            {
                var md5Bytes = md5.ComputeHash(metadataPayload);
                var payloadChecksum = BitConverter.ToString(md5Bytes).Replace("-", "");

                var saveBinaryResponse = await TaskExtensions.WithRetry(
                    async () =>
                    {
                        return await _contentApi.PostBinary(new SaveBinaryRequest
                        {
                            binary = new[]
                            {
                                new BinaryDefinition
                                {
                                    id = "metadata",
                                    checksum = payloadChecksum,
                                    uploadContentType = "application/json"
                                }
                            }
                        });
                    },
                    5,
                    500);

                var binaryResponse = saveBinaryResponse.binary.First();
                var signedUrl = binaryResponse.uploadUri;

                var content = new ByteArrayContent(metadataPayload);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentMD5 = md5Bytes;

                var putContentResponse = await TaskExtensions.WithRetry(
                    async () => await _httpClient.PutAsync(signedUrl, content),
                    5,
                    500);

                putContentResponse.EnsureSuccessStatusCode();

                return binaryResponse.uri;
            }
        }

        // Build for update
        public async Task<NftExternalMetadata> BuildMetadata(string contentId, uint tokenId, Dictionary<string, string> requestProperties)
        {
            // 1. Fetch existing metadata
            var existingMint = await _mintCollection.GetTokenMint(ContractService.DefaultContractName, tokenId);
            if (existingMint is null) throw new MintNotFoundException(tokenId.ToString());

            var metadata = existingMint.Metadata;

            // 2. Override props with content and request
            metadata.Update(requestProperties);
            return metadata;
        }

        // Build for new mint
        public NftExternalMetadata BuildMetadata(uint tokenId, string contentId, Dictionary<string, string> requestProperties)
        {
            var metadata = new NftExternalMetadata(requestProperties);
            return metadata;
        }
    }
}