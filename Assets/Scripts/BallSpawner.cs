using Unity.Netcode;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{

    [SerializeField] private NetworkObject ballPrefab;

    private bool m_ballSpawn;

    public override void OnNetworkSpawn()
    {
        // Seul le serveur peut instantier la balle
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }
    public override void OnNetworkDespawn()
    {
        // Seul le serveur peut instantier la balle
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager == null) return;

        // Si c'est uniquement le host qui est connecté, on attend un autre client
        if(IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        // Instantiation de la balle sur le serveur
        NetworkObject ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);

        var ballServer = ball.GetComponent<BallServer>();
        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform paddle = client.PlayerObject != null ? client.PlayerObject.transform : null;

            if(paddle == null) continue;
            
            if(paddle.position.x < 0f) ballServer.LeftPaddle = paddle;
            else ballServer.RightPaddle = paddle;
        }

        // Spawn de la balle sur tous les clients
        ball.Spawn();

        m_ballSpawn = true;
    }


}
