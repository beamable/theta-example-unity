using System.Linq;
using Beamable;
using Beamable.Server.Clients;
using Beamable.Theta.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Beamable.Common.Api.Inventory;


public class GameHandler : MonoBehaviour
{
    private BeamContext Context => BeamContext.Default;
    private ThetaFederationClient _federationClient;


    [SerializeField] private TextMeshProUGUI uiPlayerId;
    [SerializeField] private TextMeshProUGUI uiWallet;
    [SerializeField] private Button btnInitWallet;
    [SerializeField] private Button btnOpenWallet;
    [SerializeField] private Button btnOpenInventory;
    [SerializeField] private TextMeshProUGUI uiInventoryCoins;
    [SerializeField] private TextMeshProUGUI uiInventoryNft;
    [SerializeField] private GameObject inventorySection;
    async void Start()
    {
        inventorySection.SetActive(false);

        uiPlayerId.text = "initializing...";
        btnInitWallet.gameObject.SetActive(false);
        btnOpenWallet.gameObject.SetActive(false);

        btnInitWallet.onClick.AddListener(InitWallet);
        btnOpenWallet.onClick.AddListener(OpenExplorer);
        btnOpenInventory.onClick.AddListener(OpenInventory);

        await Context.OnReady;
        await Context.Accounts.OnReady;
        uiPlayerId.text = Context.PlayerId.ToString();

        btnInitWallet.gameObject.SetActive(true);

        ShowWalletInfo();

        Context.Api.InventoryService.Subscribe(SyncInventory);
    }

    private async void InitWallet()
    {
        if (!Context.Accounts.Current.ExternalIdentities.Any())
        {
            uiWallet.text = "initializing...";
            await Context.Accounts.AddExternalIdentity<ThetaCloudIdentity, ThetaFederationClient>("");
        }
        else
        {
            Debug.Log("Wallet already initialized");
        }

        ShowWalletInfo();
    }

    private void ShowWalletInfo()
    {
        if (Context.Accounts.Current.ExternalIdentities.Any())
        {
            uiWallet.text = Context.Accounts.Current.ExternalIdentities.First().userId;
            btnOpenWallet.gameObject.SetActive(true);
            inventorySection.SetActive(true);
        }
    }

    private async void OpenExplorer()
    {
        _federationClient = Context.Microservices().ThetaFederation();
        var contract = await _federationClient.GetDefaultContract();
        Application.OpenURL($"https://beta-explorer.thetatoken.org/account/{contract}");
    }

    private void OpenInventory()
    {
        Application.OpenURL($"https://portal.beamable.com/{Context.Cid}/games/{Context.Pid}/realms/{Context.Pid}/players/{Context.PlayerId}/inventory");
    }

    private void SyncInventory(InventoryView inventory)
    {
        uiInventoryCoins.text = inventory.currencies.Sum(c => c.Value).ToString();
        uiInventoryNft.text = inventory.items.Count.ToString();
    }
}