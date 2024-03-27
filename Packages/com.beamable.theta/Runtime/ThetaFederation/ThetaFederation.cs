using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.ThetaFederation.Endpoints;
using Beamable.Microservices.ThetaFederation.Extensions;
using Beamable.Microservices.ThetaFederation.Features.Accounts;
using Beamable.Microservices.ThetaFederation.Features.Contracts;
using Beamable.Microservices.ThetaFederation.Features.Transactions;
using Beamable.Theta.Common;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Microservices.ThetaFederation
{
    [Microservice("ThetaFederation", CustomAutoGeneratedClientPath =
        "Packages/com.beamable.theta/Runtime/Client/Autogenerated/ThetaFederationClient")]
    public class ThetaFederation : Microservice, IFederatedInventory<ThetaCloudIdentity>
    {
        private static bool _initialized;

        [ConfigureServices]
        public static void Configure(IServiceBuilder serviceBuilder)
        {
            var dependencyBuilder = serviceBuilder.Builder;
            dependencyBuilder.AddFeatures();
            dependencyBuilder.AddEndpoints();
        }

        [InitializeServices]
        public static async Task Initialize(IServiceInitializer initializer)
        {
            try
            {
                initializer.SetupExtensions();

                // Load realm configuration
                var realmConfigService = initializer.GetService<IMicroserviceRealmConfigService>();
                Configuration.RealmConfig = await realmConfigService.GetRealmConfigSettings();

                // Validate configuration
                if (string.IsNullOrEmpty(Configuration.RPCEndpoint))
                {
                    throw new ConfigurationException($"{nameof(Configuration.RPCEndpoint)} is not defined in realm config. Please apply the configuration and restart the service to make it operational.");
                }

                // Initialize Realm account
                await initializer.GetService<AccountsService>().GetOrCreateRealmAccount();

                var defaultContract = await initializer.Provider.GetService<ContractProxy>().GetDefaultContractOrDefault();
                if (defaultContract is null)
                {
                    BeamableLogger.LogWarning("Default contract is not initialized. Make sure to initialize it before using the service.");
                }
                TransactionManager.SetServiceInitialized();

            }
            catch (Exception ex)
            {
                BeamableLogger.LogException(ex);
                BeamableLogger.LogWarning("Service initialization failed. Please fix the issues before using the service.");
            }
        }

        [ClientCallable]
        public async Promise<string> InitializeContract()
        {
            if (string.IsNullOrEmpty(Configuration.RPCEndpoint))
            {
                throw new ConfigurationException($"{nameof(Configuration.RPCEndpoint)} is not defined in realm config. Please apply the configuration and restart the service to make it operational.");
            }

            var contract = await Provider.GetService<ContractService>()
                .GetOrCreateDefaultContract();
            return contract.PublicKey;
        }

        [ClientCallable]
        public async Promise<string> GetRealmAccount()
        {
            var account = await Provider.GetService<AccountsService>()
                .GetOrCreateRealmAccount();
            return account.Address;
        }

        [ClientCallable]
        public async Promise<string> GetDefaultContract()
        {
            return await Provider.GetService<GetContractAddressEndpoint>()
                .GetContractAddress();
        }

        public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution)
        {
            return await Provider.GetService<AuthenticateEndpoint>()
                .Authenticate(token, challenge, solution);
        }

        public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
        {
            return await Provider.GetService<GetInventoryStateEndpoint>()
                .GetInventoryState(id);
        }

        public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<FederatedItemCreateRequest> newItems, List<FederatedItemDeleteRequest> deleteItems,
            List<FederatedItemUpdateRequest> updateItems)
        {
            var endpoint = Provider.GetService<StartInventoryTransactionEndpoint>();
            await endpoint.ValidateRequest(id, transaction, currencies, newItems, deleteItems, updateItems);
            return await endpoint
                .StartInventoryTransaction(id, transaction, currencies, newItems, deleteItems, updateItems);
        }
    }
}