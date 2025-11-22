using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AdvancedNetworkManager))]
[RequireComponent(typeof(NetworkDiscovery))]
public class LobbyDiscoveryController : MonoBehaviour
{
    // --- Deðiþken Tanýmlamalarý (Aynen kalýyor) ---
    [Header("Lobby UI References")]
    public GameObject lobbyMenuPanel;
    public Transform serverListContainer;
    public GameObject serverListItemPrefab;

    [Header("In-Game UI References")]
    public Button stopHostButton;
    public Button disconnectButton;
    public TMPro.TextMeshPro stateText;

    private AdvancedNetworkManager manager;
    private NetworkDiscovery discovery;
    private readonly Dictionary<long, GameObject> discoveredServers = new Dictionary<long, GameObject>();

    // --- Awake (Aynen kalýyor) ---
    void Awake()
    {
        manager = GetComponent<AdvancedNetworkManager>();
        discovery = GetComponent<NetworkDiscovery>();
    }

    // --- Start (GÜNCELLENDÝ) ---
    void Start()
    {
        // Butonlara týklandýðýnda ne yapacaklarýný ata
        lobbyMenuPanel.transform.Find("HostButton").GetComponent<Button>().onClick.AddListener(OnHost);
        lobbyMenuPanel.transform.Find("FindServersButton").GetComponent<Button>().onClick.AddListener(OnFindServers);

        // --- DEÐÝÞEN BÖLÜM: NetworkManager Olaylarýný Dinleme ---
        // 'AddListener' deðil, C# event'i olan '+=' kullanýyoruz.
        manager.OnHostStarted += OnHostStarted;
        manager.OnClientStarted += OnClientStarted;
        manager.OnHostStopped += OnHostStopped;
        manager.OnClientStopped += OnClientStopped;

        // Yeni butonlarýmýza listener ekleyelim
        stopHostButton.onClick.AddListener(OnStopHostClicked);
        disconnectButton.onClick.AddListener(OnDisconnectClicked);

        // Baþlangýçta sadece lobi menüsü görünsün
        ShowLobbyMenu();
    }

    // --- OnEnable (Aynen kalýyor) ---
    void OnEnable()
    {
        discovery.OnServerFound.AddListener(OnDiscoveredServer);
    }

    // --- OnDisable (GÜNCELLENDÝ) ---
    void OnDisable()
    {
        discovery.OnServerFound.RemoveListener(OnDiscoveredServer);

        // --- DEÐÝÞEN BÖLÜM: Olay Dinlemeyi Býrakma ---
        // '-=' kullanýyoruz.
        if (manager != null)
        {
            manager.OnHostStarted -= OnHostStarted;
            manager.OnClientStarted -= OnClientStarted;
            manager.OnHostStopped -= OnHostStopped;
            manager.OnClientStopped -= OnClientStopped;
        }
    }

    //
    // --- GERÝ KALAN TÜM KODLAR (ShowLobbyMenu, OnHost, OnClientStarted vb.) ---
    // --- DEÐÝÞÝKLÝK GEREKMEZ ---
    //

    private void ShowLobbyMenu()
    {
        lobbyMenuPanel.SetActive(true);
        stopHostButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(false);
    }

    private void ShowHostMenu()
    {
        lobbyMenuPanel.SetActive(false);
        stopHostButton.gameObject.SetActive(true);
        disconnectButton.gameObject.SetActive(false);
    }

    private void ShowClientMenu()
    {
        lobbyMenuPanel.SetActive(false);
        stopHostButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(true);
    }

    private void OnHostStarted()
    {
        Debug.Log("Host Baþladý. Host Menüsü Gösteriliyor.");
        ShowHostMenu();
        if (discovery != null) discovery.StopDiscovery();
    }

    private void OnClientStarted()
    {
        if (!NetworkServer.active)
        {
            Debug.Log("Client Baþladý. Client Menüsü Gösteriliyor.");
            ShowClientMenu();
            if (discovery != null) discovery.StopDiscovery();
        }
    }

    private void OnHostStopped()
    {
        Debug.Log("Host Durdu. Lobi Menüsüne Dönülüyor.");
        ShowLobbyMenu();
    }

    private void OnClientStopped()
    {
        if (!NetworkServer.active)
        {
            Debug.Log("Client Durdu. Lobi Menüsüne Dönülüyor.");
            ShowLobbyMenu();
        }
    }

    public void OnHost()
    {
        Debug.Log("Host baþlatýlýyor...");
        ClearServerList();
        manager.StartHost();
        discovery.AdvertiseServer();
        lobbyMenuPanel.SetActive(false);
    }

    public void OnFindServers()
    {
        Debug.Log("Sunucular aranýyor...");
        ClearServerList();
        discovery.StartDiscovery();
    }

    private void OnStopHostClicked()
    {
        Debug.Log("Host durduruluyor...");
        manager.StopHost();
    }

    private void OnDisconnectClicked()
    {
        Debug.Log("Baðlantý kesiliyor...");
        manager.StopClient();
    }

    private void OnDiscoveredServer(ServerResponse info)
    {
        if (discoveredServers.ContainsKey(info.serverId))
            return;

        GameObject serverItem = Instantiate(serverListItemPrefab, serverListContainer);
        ServerListItem itemScript = serverItem.GetComponent<ServerListItem>();

        if (itemScript != null)
        {
            itemScript.Setup(info, manager);

            itemScript.OnJoinClicked += () => {
                lobbyMenuPanel.SetActive(false);
            };
        }
        discoveredServers.Add(info.serverId, serverItem);
    }

    private void ClearServerList()
    {
        foreach (GameObject item in discoveredServers.Values)
        {
            Destroy(item);
        }
        discoveredServers.Clear();
    }
}