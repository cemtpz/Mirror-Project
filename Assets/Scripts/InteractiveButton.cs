using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;
using System;

public class InteractiveButton : NetworkBehaviour
{
    private Vector3 originalPosition;
    [SerializeField] private LinkedWindow windowToToggle;
    [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] private float animationDuration = 0.2f;
    private bool isFirstPlayer;

    [SyncVar(hook = nameof(OnPressedStateChanged))]
    private int playersOnButton = 0;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    [Server]
    public void AddPlayer()
    {
        isFirstPlayer = (playersOnButton == 0);
        playersOnButton++;

        if (isFirstPlayer && windowToToggle != null)
        {
            windowToToggle.ServerToggleWindow();
        }
    }
    [Server]
    public void RemovePlayer()
    {
        playersOnButton--;
    }

    private void OnPressedStateChanged(int oldPlayers, int newPlayers)
    {
        if (newPlayers > 0 && oldPlayers == 0)
        {
            transform.DOMove(originalPosition + pressedOffset, animationDuration);
        }
        else if (newPlayers == 0 && oldPlayers > 0)
        {
            transform.DOMove(originalPosition, animationDuration);
        }
    }
}
