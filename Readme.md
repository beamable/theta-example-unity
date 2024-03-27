# Beamable Theta Sample

Welcome to the Beamable Theta Sample project! This is a Unity project that demonstrates how
to integrate the [Theta](https://theta.technology/) blockchain into a [Beamable](https://beamable.com/)
powered game.

## Getting Started

Before getting started, please head to [Beamable](https://beamable.com/) and sign up.
You should have the following tools installed on your development machine.

1. [Unity 2022](https://unity.com/download)
2. [Docker](https://www.docker.com/products/docker-desktop/)
3. [Net6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
4. [Git](https://git-scm.com/downloads)


## ERC1155
ERC1155 is a hybrid token standard supporting both fungible and non-fungible tokens. It allows for the creation
of a single smart contract that can support multiple tokens, making it more cost-efficient than creating separate
contracts for each token. This is particularly useful in gaming, where a game may have a large number of 
unique items or characters that need to be represented as tokens. Additionally, ERC1155 supports batch 
transactions, which means that multiple tokens can be transferred in a single transaction, reducing gas costs 
and improving the overall user experience. Our integration uses a single ERC1155 smart contract to represent 
all game tokens, making it more efficient and cost-effective for both developers and users.  
Source: [Default Smart Contract](https://github.com/beamable/theta-example-unity/blob/main/Packages/com.beamable.theta/Runtime/ThetaFederation/Solidity/Contracts/GameToken.sol)  

## Solidity
We are using the **solc** Solidity compiler wrapped inside of the integration microservice to compile the smart contract
at runtime. You can clone this repository and modify the smart contract per your requirements. We tried to create a contract
that is generic as possible for most use-cases.

## Sample Project
To get started with this sample project, use `git` to clone this project, and open it
with Unity 2022.

## Importing Into Your Project
The Theta integration is portable to other Beamable Unity projects. The Microservice and
Microstorage code can be imported into a target project. The best way is to use Unity Package Manager
and add a part of this repository as an external package.

**Before doing this, please ensure that the target project already has Beamable installed**

In the Unity Package Manager, [add a package via git-link](https://docs.unity3d.com/Manual/upm-ui-giturl.html).
for `com.beamable.theta` and use the following git-link.
```shell
https://github.com/beamable/theta-example-unity.git?path=/Packages/com.beamable.theta#0.0.1
```

Note: the end of the link includes the version number. You view the available versions by looking
at this repositories git tags.

## Federated content
This sample project includes one Theta federated item - BeamSword, and one Theta federated currency - BeamCoin.
You can enable federation on any item or currency.

## Try it out
* Set your RPC URI as a realm config value "RPCEndpoint" (see Configuration)
  * Theta Testnet RPC: https://eth-rpc-api-testnet.thetatoken.org/rpc
  * Theta Mainnet RPC: https://eth-rpc-api.thetatoken.org/rpc
* Publish the **ThetaFederation** microservice along with the **ThetaStorage** microstorage.
* Open the Portal an wait for the microservice to get into the **running** state.
* Explore the microservice logs and microstorage data. Microservice should create and store your developer wallet on first run.
* Use a [Theta Web Wallet](https://wallet.thetatoken.org/) to request some test TFUEL tokens for your developer wallet.
  * To find your Developer wallet address visit the Beamable Portal under Operate > Microservices
  * Under Microservice Actions click Docs
  * This will open Microservice Swagger documentation
  * Call GetRealmAccount endpoint
  * This output your developer wallet address
* Initialize Default Contract
  * Call InitializeContract endpoint
  * This compile and publish the smart contract and output its address

NOTE: First request to the microservice will initiate the compile and deploy procedure for the smart contract. Depending on your RPC endpoint, it may result in a timeout. Be sure to check the microservice logs.

## NFT metadata
We're using our existing Content System backed by AWS S3 and AWS CloudFront CDN for storing NFT metadata. Every property you specify in the Inventory `CreateItemRequest` will become a NFT metadata property.
To specify a root level metadata property, like "image" or "description", you just prefix the property name with a '$' character.  
Example `CreateItemRequest`:
```json
{
  "contentId": "items.sword",
  "properties": [
    {
      "name": "$image",
      "value": "someimage"
    },
    {
      "name": "$description",
      "value": "strong sword"
    },
    {
      "name": "damage",
      "value": "500"
    },
    {
      "name": "price",
      "value": "350"
    }
  ]
}
```
Resulting metadata file:
```json
{
  "image": "someimage",
  "description": "strong sword",
  "properties": [
    {
      "name": "damage",
      "value": "500"
    },
    {
      "name": "price",
      "value": "350"
    }
  ]
}
```

## Configuration
Configuration defaults are hard-coded inside **Runtime/ThetaFederation/Configuration.cs**  
You can override the values using the realm configuration.  
![Realm Configuration Example](Screenshots/realm-config.png)

**Default values:**

| **Namespace**      | **Key**                       | **Default value** | **Description**                                        |
|--------------------|-------------------------------|-------------------|--------------------------------------------------------|
| federation_theta   | RPCEndpoint                   |                   | Cluster RPC API URI                                    |
| federation_theta   | AllowManagedAccounts          | true              | Allow custodial wallets for players                    |
| federation_theta   | AuthenticationChallengeTtlSec | 600               | Authentication challenge TTL                           |
| federation_theta   | ReceiptPoolIntervalMs         | 200               | Pooling interval when fetching a transaction receipt   |
| federation_theta   | TransactionRetries            | 10                | Failed transaction retry count                         |
| federation_theta   | MaximumGas                    | 2_000_000         | Max transaction gas amount                             |
| federation_theta   | GasPriceCacheTtlSec           | 3                 | Cache time for previous transaction gas amount         |
| federation_theta   | GasExtraPercent               | 0                 | Increase transaction gas amount                        |
| federation_theta   | CollectionName                |                   | Collection name                                        |
| federation_theta   | CollectionDescription         |                   | Collection description                                 |
| federation_theta   | CollectionImage               |                   | Collection image URL                                   |
| federation_theta   | CollectionLink                |                   | Collection external link                               |

**IMPORTANT:** Configuration is loaded when the service starts. Any configuration change requires a service restart.