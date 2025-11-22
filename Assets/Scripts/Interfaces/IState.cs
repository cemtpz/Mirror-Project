using UnityEngine;

public interface IState
{
    void EnterState(PlayerController player);
    void ExitState(PlayerController player);
    void UpdateState(PlayerController player);
    void StateProcess(PlayerController player);
    void FixedStateProcess(PlayerController player);
}