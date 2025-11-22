using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private PlayerConfig playerConfig;
    private PlayerController playerController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerConfig = ConfigManager.Instance.playerConfig;
        playerController = GetComponent<PlayerController>();
        Physics2D.queriesStartInColliders = false;
    }

    public void Walk()
    {
        //Hata kontrolü
        if (rb == null || playerConfig == null)
        {
            Debug.LogError("PlayerMovement is not properly initialized.");
            return;
        }
        //Yöne göre hýz verir
        rb.velocity = new Vector2(playerController.inputHandler.MoveDirection * playerConfig.moveSpeed, rb.velocity.y);
    }
    public void StopWalking()
    {
        //Durdururken yatay hýzý sýfýrlar
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
        
    public void Jump()
    {
        if (playerController.IsGrounded)
        {
            //Yukarý doðru zýplama kuvveti uygular
            rb.velocity = new Vector2(rb.velocity.x, playerConfig.jumpForce);
            GetComponent<InteractiveObjectHandler>().PlatformJump();
        }   
    }
}