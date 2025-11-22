using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InteractiveObjectHandler))]
public class ManualPlayerSync : NetworkBehaviour
{
    private Rigidbody2D rb;

    // SyncVar yerine Unreliable RPC ile yayýn yapacaðýz
    // Hedefler client tarafýnda tutulacak
    private Vector2 targetPosition;
    private Vector2 targetVelocity;

    [Header("Smoothing Settings")]
    [SerializeField] private float lerpRate = 15f;
    [SerializeField] private float snapThreshold = 5f; // Uzaksa ýþýnla

    //[Header("Network Send Settings")] 
    //[SerializeField] private float sendRate = 30f; // Hz
    //private float sendTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer) return;
        
        // Uzak karakterlerde gravity'yi kapat
        rb.gravityScale = 0;
        targetPosition = rb.position;
        targetVelocity = rb.velocity;
        
    }

    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            // YEREL OYUNCU: Normal fizik çalýþýyor
            // Transformu sýnýrlý hýzda sunucuya/diðer clientlara ilet
            //sendTimer += Time.fixedDeltaTime;
            //float interval = (sendRate > 0f) ? 1f / sendRate : 0f;
            //if (interval == 0f || sendTimer >= interval)
            //{
            //    sendTimer = 0f;
            //}
                

            Vector2 pos = rb.position;
            Vector2 vel = rb.velocity;

            // Host modunda server doðrudan clientlara yayýnlar
            if (isServer) RpcSyncTransform(pos, vel);
            // Client modundaysa sunucuya gönder (Unreliable)
            else CmdSendTransform(pos, vel);
        }
        else
        {
            // UZAK OYUNCU: Sunucudan gelen hedefe doðru interpolate et
            float distance = Vector2.Distance(rb.position, targetPosition);

            if (distance > snapThreshold)
            {
                // Çok uzaktaysa ýþýnla
                rb.position = targetPosition;
            }
            else
            {
                // Yumuþak geçiþ
                rb.position = Vector2.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * lerpRate);
            }

            rb.velocity = targetVelocity;
        }
    }

    [Command(channel = Channels.Unreliable)]
    void CmdSendTransform(Vector2 pos, Vector2 vel)
    {
        RpcSyncTransform(pos, vel);
    }

    [ClientRpc(channel = Channels.Unreliable)]
    void RpcSyncTransform(Vector2 pos, Vector2 vel)
    {
        if (isLocalPlayer) return;

        targetPosition = pos;
        targetVelocity = vel;
    }
}