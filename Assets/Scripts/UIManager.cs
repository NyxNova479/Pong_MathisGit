using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{
    [SerializeField] private string address = "127.0.0.1";
    [SerializeField] private ushort port = 7777;
    [SerializeField] private GameObject connectPanel;
    private NetworkManager m_networkManager;

    private NetworkManager Manager => m_networkManager != null ? m_networkManager : NetworkManager.Singleton != null ? NetworkManager.Singleton : FindAnyObjectByType<NetworkManager>();
    private UnityTransport Transport => (UnityTransport)Manager.NetworkConfig.NetworkTransport;

    private void Start()
    {
        m_networkManager = Manager;
        if (m_networkManager == null)
        {
            Debug.Log("NET NetworkManager not found in scene");
            return;
        }
        m_networkManager.OnServerStarted += HandleServerStarted;
        m_networkManager.OnClientConnectedCallback += HandleClientConnection;
        m_networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (m_networkManager == null) return;
        m_networkManager.OnServerStarted -= HandleServerStarted;
        m_networkManager.OnClientConnectedCallback -= HandleClientConnection;
        m_networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    public void StartHost()
    {
        // NetworkManager.Singleton.StartHost();
        Transport.SetConnectionData("0.0.0.0", port);
        var manager = Manager;

        if (manager == null) return;

        bool ok = manager.StartHost();
        Debug.Log($"[NET] Host sur 0.0.0.0: {port} started: {ok}");
    }

    public void StartClient()
    {
        //NetworkManager.Singleton.StartClient();
        Transport.SetConnectionData(address, port);
        var manager = Manager;

        if (manager == null) return;

        bool ok = manager.StartClient();
        Debug.Log($"[NET] Client vers {address}: {port} started: {ok}");
    }

    private void HandleServerStarted() => ShowConnectPanel(false);

    private void HandleClientConnection(ulong clientId)
    {
        if (Manager != null && clientId == Manager.LocalClientId)
        {
            ShowConnectPanel(false);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (Manager != null && clientId == Manager.LocalClientId)
        {
            ShowConnectPanel(true);
        }
    }

    private void ShowConnectPanel(bool visible)
    {
        if (connectPanel != null)
        {
            connectPanel.SetActive(visible);
        }
    }
}