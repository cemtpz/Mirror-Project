using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class LinkedWindow : NetworkBehaviour
{
    [SerializeField] private float openPositionY = 5;
                     private Vector3 closedPosition;
    [SerializeField] private float animationDuration = 0.5f;

    private float lastY;
    [HideInInspector] public float deltaY;

    [SyncVar]
    public bool isOpen = false;

    private void Awake()
    {
        closedPosition = transform.position;
        lastY = transform.position.y;
    }

    private void FixedUpdate()
    {
        deltaY = transform.position.y - lastY;
        lastY = transform.position.y;
    }

    //Bir client sonradan katýlýrsa durumu senkronize et
    public override void OnStartClient()
    {
        if (isOpen)
        {
            transform.position = new Vector3(closedPosition.x, openPositionY, closedPosition.z);
        }
    }

    [Server]
    public void ServerToggleWindow()
    {
        isOpen = !isOpen;
        RpcPlayAnimation(isOpen);
    }
    [ClientRpc]
    private void RpcPlayAnimation(bool newState)
    {
        transform.DOKill();
        if (newState)
        {
            transform.DOMoveY(openPositionY, animationDuration).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Fixed);
            //DOTween.To(() => rb.position, x => rb.MovePosition(x), new Vector2(rb.position.x, openPositionY), animationDuration).SetUpdate(UpdateType.Fixed);
        }
        else
        {
            transform.DOMoveY(closedPosition.y, animationDuration).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Fixed);
            //DOTween.To(() => rb.position, x => rb.MovePosition(x), new Vector2(rb.position.x, closedPosition.y), animationDuration).SetUpdate(UpdateType.Fixed);
        }
    }
}
