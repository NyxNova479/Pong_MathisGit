using Unity.Netcode;
using UnityEngine;

public class PaddleSpawnPosition : NetworkBehaviour
{

    [SerializeField]private float xOffset = 7f;

    public override void OnNetworkSpawn()
    {
        //Seul le serveur peut définir la position des paddles
        if(!IsServer) return;

        //Pong à 2 joueurs : host à gauche et client à droite
        bool isHostPlayer = OwnerClientId == NetworkManager.ServerClientId;
        float xPosition = isHostPlayer ? -xOffset : xOffset;

        
        transform.position = new Vector3(xPosition,0f,0f);

        
    }



}
