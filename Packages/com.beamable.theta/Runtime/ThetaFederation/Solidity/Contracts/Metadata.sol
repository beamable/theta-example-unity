// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "openzeppelin/contracts/access/Ownable.sol";

abstract contract Metadata is ERC1155, Ownable {    

    mapping(uint256 => string) internal _metadata;
    string internal _baseURI;
    string internal _contractURI;

    function setBaseURI(string memory newBaseUri) public onlyOwner {
        _baseURI = newBaseUri;
    }

    function getTokenURI(uint256 tokenId) public view returns (string memory tokenURI) {
        string storage metadata = _metadata[uint48(tokenId)];
        return bytes(metadata).length > 0 ? string(abi.encodePacked(_baseURI, metadata)) : super.uri(tokenId);
    }

    function contractURI() public view returns (string memory) {
        return _contractURI;
    }

    function setContractURI(string memory newUri) public onlyOwner {
        _contractURI = newUri;
    }

    function setTokenMetadataHash(
        uint256 tokenId,
        string memory metadataHash
    ) public onlyOwner {
        _metadata[uint48(tokenId)] = metadataHash;
        emit URI(getTokenURI(tokenId), tokenId);
    }

    function setTokenMetadataHashes(
        uint256[] calldata tokenIds,
        string[] calldata metadataHashes
    ) public onlyOwner {
        for (uint256 i = 0; i < tokenIds.length; i++) {
            setTokenMetadataHash(tokenIds[i], metadataHashes[i]);
        }
    }
}