using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PaddleView : NetworkBehaviour
{

    [SerializeField] private PaddleInput source; // le root, qui détient yPos

    [SerializeField] private Transform target; // le Visual

    [SerializeField] private float smoothTime = 0.08f;

    private float m_velY; // mémoire de vitesse du ressort entre root et visaul entre 2 frames

    public override void OnNetworkSpawn()
    {
        if (source == null) source = GetComponent<PaddleInput>();

        if (target == null) target = transform.Find("Visual");

        target.SetParent(null);
    }

    public override void OnNetworkDespawn()
    {
        if (target != null) Destroy(target.gameObject);
    }

    // On en aura besoin pour mesurer la taille du sprite.
    public Transform GetVisual() => target;

    // Update is called once per frame
    void Update()
    {

        if (source == null || target == null) return;

        Vector3 p = target.position;

        p.x = transform.position.x;

        p.y = Mathf.SmoothDamp(p.y, source.transform.position.y, ref m_velY, smoothTime);

        target.position = p;

    }
}
