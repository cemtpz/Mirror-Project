using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // Olaylar (Events) için bu GEREKLÝ
using UnityEngine.SceneManagement;

// OzelNetworkManager'ýn yeni adý.
public class AdvancedNetworkManager : NetworkManager
{
    public static AdvancedNetworkManager instance;

    [Header("Lobby Spawn Points")]
    public Transform lobbyHostSpawn;
    public Transform lobbyClientSpawn;

    [Header("Game Character Prefabs")]
    public GameObject redCharacterPrefab;
    public GameObject blueCharacterPrefab;

    [Scene]
    public string lobbyScene;

    private Dictionary<NetworkConnectionToClient, string> playerChoices = new Dictionary<NetworkConnectionToClient, string>();

    // --- YENÝ BÖLÜM: UI Script'inin Dinlemesi Ýçin Olaylar (Events) ---
    // LobbyDiscoveryController bu olaylarý dinleyecek.
    public event Action OnHostStarted;
    public event Action OnClientStarted;
    public event Action OnHostStopped;
    public event Action OnClientStopped;


    // --- YENÝ BÖLÜM: NetworkManager Metotlarýný Ezme (Override) ---
    // NetworkManager'ýn ana fonksiyonlarýný ezip, kendi olaylarýmýzý tetikliyoruz.
    public override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        autoCreatePlayer = true;
    }
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (newSceneName == "MainMenu")
        {
            autoCreatePlayer = false; // "Oyun sahnesine geçerken YENÝ oyuncu ÝSTEME"
        }
        else if (newSceneName == lobbyScene)
        {
            autoCreatePlayer = true; // "Lobiye dönerken oyuncu ÝSTE"
        }
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }
    public override void OnStartHost()
    {
        base.OnStartHost(); // Mirror'ýn temel kodunu çalýþtýr
        Debug.Log("AdvancedNetworkManager: Host Baþladý. Olay tetikleniyor.");
        OnHostStarted?.Invoke(); // Bizim 'OnHostStarted' olayýmýzý tetikle
    }

    public override void OnStartClient()
    {
        base.OnStartClient(); // Mirror'ýn temel kodunu çalýþtýr
        Debug.Log("AdvancedNetworkManager: Client Baþladý. Olay tetikleniyor.");
        OnClientStarted?.Invoke(); // Bizim 'OnClientStarted' olayýmýzý tetikle
    }

    public override void OnStopHost()
    {
        base.OnStopHost(); // Mirror'ýn temel kodunu çalýþtýr
        Debug.Log("AdvancedNetworkManager: Host Durdu. Olay tetikleniyor.");
        OnHostStopped?.Invoke(); // Bizim 'OnHostStopped' olayýmýzý tetikle
    }

    public override void OnStopClient()
    {
        base.OnStopClient(); // Mirror'ýn temel kodunu çalýþtýr
        Debug.Log("AdvancedNetworkManager: Client Durdu. Olay tetikleniyor.");
        OnClientStopped?.Invoke(); // Bizim 'OnClientStopped' olayýmýzý tetikle
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (onlineScene == lobbyScene)
        {
            // ... (Lobi spawn etme kodun burada) ...
            Transform spawnPoint = (numPlayers == 0) ? lobbyHostSpawn : lobbyClientSpawn;
            if (spawnPoint == null) return;
            GameObject lobbyPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.AddPlayerForConnection(conn, lobbyPlayer);
        }
        
    }

    public void StartGame()
    {
        // 1. Lobi'deki tüm oyuncularý bul
        LobbyPlayer[] allPlayers = FindObjectsOfType<LobbyPlayer>();

        // 2. Seçimlerini hafýzaya (Dictionary) kaydet
        playerChoices.Clear(); // Önceki oyundan kalanlarý temizle
        foreach (LobbyPlayer player in allPlayers)
        {
            playerChoices.Add(player.connectionToClient, player.selectedCharacter);
        }

        // 3. Oyun sahnesine geçiþ yap
        // (gameScene deðiþkenini Inspector'dan "GameScene" olarak ayarlamýþ olmalýsýn)
        Debug.Log("Herkes hazýr! Oyun sahnesine geçiliyor...");
        ServerChangeScene("MainMenu");
    }

    // (Buraya LobiOyuncusu'ndan seçimleri kaydeden fonksiyonlar ekleyebilirsin)
    public void RecordPlayerChoice(NetworkConnectionToClient conn, string choice)
    {
        // ...
    }

    // Oyun sahnesi yüklendiðinde...
    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "LobbyScene")
        {
            playerChoices.Clear();
            autoCreatePlayer = true;
            return;
        }

        if (sceneName == "MainMenu")
        {
            autoCreatePlayer = false;
            Transform spawnHost = null;
            GameObject spawnHostObj = GameObject.FindWithTag("MenuScene_Host");
            if (spawnHostObj != null)
            {
                spawnHost = spawnHostObj.transform;
            }

            Transform spawnClient = null;
            GameObject spawnClientObj = GameObject.FindWithTag("MenuScene_Client");
            if (spawnClientObj != null)
            {
                spawnClient = spawnClientObj.transform;
            }
            if (spawnHost == null || spawnClient == null) return;

            int playerIndex = 0;

            foreach (var entry in playerChoices)
            {
                NetworkConnectionToClient conn = entry.Key;
                string characterChoice = entry.Value;

                // 1. Doðru prefab'ý seç
                GameObject prefabToSpawn;
                if (characterChoice == "RED")
                {
                    prefabToSpawn = redCharacterPrefab;
                }
                else if (characterChoice == "BLUE")
                {
                    prefabToSpawn = blueCharacterPrefab;
                }
                else
                {
                    Debug.LogWarning("Geçersiz karakter seçimi, varsayýlan Kýrmýzý atanýyor.");
                    prefabToSpawn = redCharacterPrefab; // Varsayýlan
                }

                // 2. Doðru spawn noktasýný seç
                Transform spawnPoint = (playerIndex == 0) ? spawnHost : spawnClient;

                // 3. Karakteri oluþtur
                GameObject gamePlayer = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

                // (Burada scale gibi ekstra ayarlarý yapabilirsin)
                // gamePlayer.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                // 4. Oyuncunun Lobi Karakterini SÝL ve yerine YENÝ Oyun Karakterini ata
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);

                playerIndex++;
            }
        }
    }

    public void CheckAllPlayerStates()
    {
        Debug.Log("--- Tüm Oyuncu Durumlarý Kontrol Ediliyor ---");

        // Sahnedeki TÜM LobbyPlayer script'lerini bulur
        LobbyPlayer[] allPlayers = FindObjectsOfType<LobbyPlayer>();

        if (allPlayers.Length == 0)
        {
            Debug.Log("Sahnede oyuncu bulunamadý.");
            return;
        }

        // Bulduðu her oyuncu için bir döngü baþlatýr
        foreach (LobbyPlayer player in allPlayers)
        {
            // Her oyuncunun 'isOwned' durumuna ve 'stateIndex'ine bak
            if (player.isOwned)
            {
                // Bu, 'FindObjectsOfType'ý çalýþtýran KÝÞÝNÝN kendi oyuncusudur
                Debug.Log($"Yerel Oyuncu (LocalPlayer) durumu: {player.stateIndex}");
            }
            else
            {
                // Bu, diðer oyuncularýn kopyasýdýr
                Debug.Log($"Uzak Oyuncu (Non-Local) durumu: {player.stateIndex}");
            }

            // Gördüðün gibi, her iki stateIndex deðerine de sorunsuz eriþtik.
        }
    }
}