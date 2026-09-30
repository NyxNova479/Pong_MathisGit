using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleInput : NetworkBehaviour
{

    [SerializeField] private float speed = 5f;


    // Vitesse de convergence entre startPos et targetPos (toutes les secondes)
    [SerializeField, Min(0f)] private float displayRate = 12f;

    private NetworkVariable<float> yPos = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Dernière intention envoyée par ce client pour ne pas envoyer à chaque image
    private float m_lastSentDirection;

    // Intention retenu par le serveur et appliqué par la simulation
    private float m_serverDirection;


    private void SendMyIntention()
    {
        // IsOwner : je ne pilote que MON paddle
        // IsClient : ce code n'a rien à faire sur un serveur sans client
        if (!IsOwner || !IsClient) return;

        float direction = ReadVerticalIntent(); // -1.0f, 0.0f et 1.0f

        if (Mathf.Approximately(direction, m_lastSentDirection)) return;

        m_lastSentDirection = direction;
        SubmitInputServerRpc(direction);


    }

    private void Update()
    {
        SendMyIntention();
        if (IsServer)
        {
            SimulateServer();
        }
        SmoothDisplay();
    }

    private void SmoothDisplay()
    {
        // Le serveur détient la vérité : il n'a rien lisser
        if (IsServer) return;

        // Fractioon exponentielle : même vitesse de convergence quelque soit le framerate
        float convergence = 1f - Mathf.Exp(-displayRate * Time.deltaTime);

        Vector3 pos = transform.position;
        float startPos = pos.y;
        float targetPos = yPos.Value;

        pos.y = Mathf.Lerp(startPos, targetPos, convergence);
        transform.position = pos;

        
    }

    private void SimulateServer()
    {
        // Aucun mouvement à simuler = inutile de republier la même position
        if (Mathf.Approximately(m_serverDirection, 0f)) return;

        // On calcule la nouvelle position
        Vector3 pos = transform.position;
        pos.y += m_serverDirection * speed * Time.deltaTime;
        transform.position = pos;

        // La nouvelle position descends vers tous les clients
        yPos.Value = pos.y;
    }

    [ServerRpc]
    private void SubmitInputServerRpc(float direction)
    {
        // Le serveur valide l'intention reçu : unb client ne choisit jamais sa vitesse
        if(float.IsNaN(direction) || float.IsInfinity(direction)) return;

        m_serverDirection = Mathf.Clamp(direction, -1.0f, 1.0f);
    }

    private float ReadVerticalIntent()
    {
        Keyboard keyboard = Keyboard.current;

        if(keyboard == null) return 0f;

        float direction = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction += 1f;
        
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction -= 1f;




        return Mathf.Clamp(direction, -1f, 1f);
    }
}
