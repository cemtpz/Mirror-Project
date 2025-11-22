using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private PlayerMovement playerMovement;
    public void EnterState(PlayerController player)
    {
        if (playerMovement == null) playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.StopWalking();
    }

    public void ExitState(PlayerController player)
    {
        //Debug.Log("Exited Idle State");
    }

    //Update mantýðý ile çalýþýyor
    public void StateProcess(PlayerController player)
    {
        
    }
    //FixedUpdate mantýðý ile çalýþýyor
    public void FixedStateProcess(PlayerController player)
    {

    }

    public void UpdateState(PlayerController player)
    {
        if (player.IsWalking && !player.IsJumping)
        {
            player.ChangeState(new WalkingState());
        }
        else if (player.IsJumping)
        {
            player.ChangeState(new JumpingState());
        }
    }
}
