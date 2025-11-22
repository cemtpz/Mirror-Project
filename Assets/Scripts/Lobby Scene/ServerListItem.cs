using Mirror;
using Mirror.Discovery;
using System;
using TMPro; // TextMeshPro kullanýyorsan
using UnityEngine;
using UnityEngine.UI;

// Bu script listedeki her bir satýr prefab'ýnýn üzerinde olmalý
public class ServerListItem : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Sunucu IP adresini gösterecek Text alaný")]
    public TextMeshProUGUI serverAddressText;

    [Tooltip("Katýl butonunu buraya sürükleyin")]
    public Button joinButton;

    public event Action OnJoinClicked;

    private ServerResponse serverInfo;
    private AdvancedNetworkManager manager;

    void Start()
    {
        // Join butonuna týklandýðýnda OnJoin fonksiyonunu çaðýr
        joinButton.onClick.AddListener(OnJoin);
    }

    // LobbyDiscoveryController tarafýndan çaðrýlýr
    public void Setup(ServerResponse info, AdvancedNetworkManager networkManager)
    {
        serverInfo = info;
        manager = networkManager;

        // Text alanýna sunucunun IP adresini yaz
        serverAddressText.text = info.EndPoint.Address.ToString();
    }

    public void OnJoin()
    {
        Debug.Log($"Sunucuya baðlanýlýyor: {serverInfo.EndPoint.Address}...");

        OnJoinClicked?.Invoke();

        FindObjectOfType<NetworkDiscovery>().StopDiscovery();
        manager.networkAddress = serverInfo.EndPoint.Address.ToString();
        manager.StartClient();

    }
}