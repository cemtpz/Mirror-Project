using Mirror;
using Mirror.Examples.CouchCoop;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractiveObjectHandler : NetworkBehaviour
{
    private Rigidbody2D rb;
    private bool justJumped = false;

    [SyncVar(hook = nameof(OnPlatformChanged))]
    [HideInInspector] public Transform currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLocalPlayer || !other.CompareTag("Button")) return;

        NetworkIdentity buttonIdentity = other.GetComponentInParent<NetworkIdentity>();
        if (buttonIdentity == null) return;

        CmdSetButtonState(buttonIdentity.netId, true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isLocalPlayer || !other.CompareTag("Button")) return;

        NetworkIdentity buttonIdentity = other.GetComponentInParent<NetworkIdentity>();
        if (buttonIdentity == null) return;

        CmdSetButtonState(buttonIdentity.netId, false);
    }

    #region Moving Platform Handling
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!isLocalPlayer) return;
        if (other.collider.CompareTag("MovingPlatform"))
        {
            foreach (ContactPoint2D contact in other.contacts)
            {
                if (contact.normal.y > 0.1f) // Üstüne bastýk
                {
                    CmdSetPlatform(other.transform);
                    break;
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        if (!isLocalPlayer) return;
        if (other.collider.CompareTag("MovingPlatform"))
        {
            CmdSetPlatform(null);
            currentPlatform = null;
            if (!justJumped)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }
    /*
    [Command]
    private void CmdSetParent(GameObject platform)
    {
        currentPlatform = platform.transform;
        RpcSetParent(platform);
    }
    [Command]
    private void CmdLeaveParent()
    {
        currentPlatform = null;
        RpcLeaveParent();
    }
    [ClientRpc]
    private void RpcSetParent(GameObject platform)
    {
        transform.SetParent(platform.transform);
    }
    [ClientRpc]
    private void RpcLeaveParent()
    {
        transform.SetParent(null);
    }
    */

    [Command]
    private void CmdSetPlatform(Transform newPlatform)
    {
        currentPlatform = newPlatform;
    }
    private void OnPlatformChanged(Transform oldPlatform, Transform newPlatform)
    {
        currentPlatform = newPlatform;
    }

    private void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            LinkedWindow platform = currentPlatform.GetComponent<LinkedWindow>();
            if (platform != null)
            {
                float platformVelocityY = platform.deltaY / Time.fixedDeltaTime;
                rb.velocity = new Vector2(rb.velocity.x, platformVelocityY);
            }
        }
    }

    public void PlatformJump()
    {
        currentPlatform = null;
        StartCoroutine(JumpCooldown());
    }
    private IEnumerator JumpCooldown()
    {
        justJumped = true;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        justJumped = false;
    }
    #endregion
    [Command]
    private void CmdSetButtonState(uint buttonNetId, bool isEntering)  
    {
        if (NetworkServer.spawned.TryGetValue(buttonNetId, out NetworkIdentity buttonIdentity))
        {
            InteractiveButton button = buttonIdentity.GetComponent<InteractiveButton>();
            if (button != null)
            {
                if (isEntering)
                {
                    button.AddPlayer();
                }
                else
                {
                    button.RemovePlayer();
                }
            }
        }
    }
}
