// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "openzeppelin/contracts/access/Ownable.sol";
import "openzeppelin/contracts/token/ERC1155/extensions/ERC1155Supply.sol";
import "openzeppelin/contracts/token/common/ERC2981.sol";
import "openzeppelin/contracts/utils/structs/EnumerableSet.sol";

import "Metadata.sol";
import "Inventory.sol";

contract GameToken is
    ERC1155,
    ERC1155Supply,
    ERC2981,
    Ownable,
    Metadata,
    Inventory
{

    using EnumerableSet for EnumerableSet.UintSet;

    constructor() ERC1155("") Ownable(msg.sender) {}

    // METADATA
    function uri(uint256 tokenId) public view override returns (string memory) {
        return getTokenURI(tokenId);
    }

    // MINTING
    function mint(
        address to,
        uint256 tokenId,
        uint256 amount,
        string memory metadataHash
    ) private {
        _mint(to, tokenId, amount, "");
        _metadata[tokenId] = metadataHash;
    }

    function batchMint(
        address to,
        uint256[] memory tokenIds,
        uint256[] memory amounts,
        string[] memory metadataHashes
    ) external onlyOwner {
        require(
            tokenIds.length == amounts.length,
            "tokenIds and amounts length mismatch"
        );
        require(
            tokenIds.length == metadataHashes.length,
            "tokenIds and metadataHashes length mismatch"
        );

        for (uint256 i = 0; i < tokenIds.length; i++) {
            mint(to, tokenIds[i], amounts[i], metadataHashes[i]);
        }
    }

    // UPDATE HOOK
    function _update(
        address from,
        address to,
        uint256[] memory ids,
        uint256[] memory values
    ) internal virtual override(ERC1155, ERC1155Supply) {
        super._update(from, to, ids, values);
        updateInventoryAfterTransfer(from, to, ids, values);
    }

    // ROYALTIES
    function setDefaultRoyalty(address receiver, uint96 feeNumerator)
        public
        onlyOwner
    {
        _setDefaultRoyalty(receiver, feeNumerator);
    }

    function supportsInterface(bytes4 interfaceId)
        public
        view
        virtual
        override(ERC1155, ERC2981)
        returns (bool)
    {
        return
            super.supportsInterface(interfaceId);
    }
}