using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : NetworkBehaviour
{
    public InputHandler inputHandler { get; private set;}
    private PlayerConfig playerConfig;
    private IState currentState;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [SerializeField] private bool isGrounded;
    private bool isWalking;
    void Awake()
    {
        if(inputHandler == null) inputHandler = GetComponent<InputHandler>();
        if(playerConfig == null) playerConfig = ConfigManager.Instance.playerConfig;

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //Baþlangýç rengi ayarlama
        spriteRenderer.color = playerConfig.PlayerColor;
        //if (!isLocalPlayer)
        //{
        //    this.enabled = false;
        //}
        //Baþlangýç durumu ayarlama
        currentState = new IdleState();
        currentState.EnterState(this);
    }

    #region Renk Deðiþtirme Olaylarý
    private void OnEnable()
    {
        if (playerConfig != null)
        {
            //abone ol
            playerConfig.OnColorChanged += SetColor;
        }
    }
    private void OnDisable()
    {
        if (playerConfig != null)
        {
            //abonelikten çýk
            playerConfig.OnColorChanged -= SetColor;
        }
    }
    private void SetColor(Color newColor)
    {
        //renk deðiþtir
        spriteRenderer.color = newColor;
        Debug.Log("Player color changed to: " + newColor);
    }
    #endregion

    void Update()
    {
        if (!isLocalPlayer) return;

        currentState.UpdateState(this);
        currentState.StateProcess(this);
    }
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        currentState.FixedStateProcess(this);
    }

    public void ChangeState(IState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    #region Kontroller
    //Yürüyüp yürümediðini kontrol et
    public bool IsWalking
    {
        get
        {
            return isWalking;
        }
        set 
        { 
            if (isWalking != value)
            {
                isWalking = value;
            }
        }
    }

    //Zýplayýp zýplamadýðýný kontrol et
    public bool IsJumping
    {
        get
        {
            return inputHandler.JumpPressed;
        }
    }

    //Yerde olup olmadýðýný kontrol et
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
        set
        {
            if (isGrounded != value)
            {
                isGrounded = value;
            }
        }
    }
    #endregion
}