using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : IState
{
    private PlayerMovement playerMovement;
    public void EnterState(PlayerController player)
    {
        if (playerMovement == null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    public void ExitState(PlayerController player)
    {
        
    }

    //Update mantýðý ile çalýþýyor
    public void StateProcess(PlayerController player)
    {
        
    }
    //FixedUpdate mantýðý ile çalýþýyor
    public void FixedStateProcess(PlayerController player)
    {
        playerMovement.Walk();
    }

    public void UpdateState(PlayerController player)
    {
        if (!player.IsWalking && !player.IsJumping)
        {
            player.ChangeState(new IdleState());
            
        }
        else if (player.IsJumping)
        {
            player.ChangeState(new JumpingState());
        }
    }
}
