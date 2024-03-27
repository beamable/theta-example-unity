// SPDX-License-Identifier: MIT
pragma solidity 0.8.20;

import "Metadata.sol";

abstract contract Inventory is Metadata {

    mapping(address => uint256[]) internal _tokensPerAddress;
    mapping(address => mapping(uint256 => bool)) internal _tokensPerAddressPresence;

    function getInventory(address account)
        public
        view
        returns (
            uint256[] memory tokenIds,
            uint256[] memory tokenAmounts,
            string[] memory metadataHashes
        )
    {
        uint256[] storage ids = _tokensPerAddress[account];

        // Filter only present tokens
        uint256 presentCount = 0;
        uint256[] memory filteredIds = new uint256[](ids.length);
        for (uint256 i = 0; i < ids.length; i++) {
            if (_tokensPerAddressPresence[account][ids[i]]) {
                filteredIds[presentCount] = ids[i];
                presentCount++;
            }
        }

        uint256[] memory presentIds = new uint256[](presentCount);
        for (uint256 i = 0; i < presentCount; i++) {
            presentIds[i] = filteredIds[i];
        }

        // Fetch amounts and metadata for present tokens
        uint256[] memory amounts = new uint256[](presentIds.length);
        string[] memory metadata = new string[](presentIds.length);
        for (uint256 i = 0; i < presentIds.length; i++) {
            uint256 tokenId = presentIds[i];
            amounts[i] = balanceOf(account, tokenId);
            metadata[i] = _metadata[tokenId];
        }
        return (presentIds, amounts, metadata);
    }

    function updateInventoryAfterTransfer(
        address from,
        address to,
        uint256[] memory ids,
        uint256[] memory
    ) internal virtual {
        for (uint256 i = 0; i < ids.length; i++) {
            uint256 tokenId = ids[i];
            if (to != address(0) && !_tokensPerAddressPresence[to][tokenId]) {
                _tokensPerAddress[to].push(tokenId);
                _tokensPerAddressPresence[to][tokenId] = true;
            }
            if (from != address(0) && balanceOf(from, tokenId) == 0) {
                _tokensPerAddressPresence[from][tokenId] = false;
            }
        }
    }
}