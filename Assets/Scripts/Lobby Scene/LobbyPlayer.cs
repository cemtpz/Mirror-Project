using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("Görsel Referanslar")]
    public SpriteRenderer spriteRenderer;
    public Sprite greySprite;
    public Sprite redSprite;
    public Sprite blueSprite;

    [Header("DOMove Parametreleri")]
    [SerializeField] float animationDuration = 1f;
    [SerializeField] float moveOfset = 6f;
    [SerializeField] float scaleOfset = 2;
    private float originalScaleOfset;
    private bool isAnimating = false;

    [SyncVar (hook = nameof(OnColorChanged))]
    public int stateIndex = 0;

    [SyncVar]
    public string selectedCharacter;

    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady = false;

    private void Awake()
    {
        originalScaleOfset = transform.localScale.x;
    }
    private void Update()
    {
        if (!isOwned) return;
        if (isAnimating) return;
        if (!isReady)
        {
            switch (stateIndex)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        //Sol
                        isAnimating = true;
                        CmdChangeState(1, "RED");
                    }
                    else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        //Sað
                        isAnimating = true;
                        CmdChangeState(2, "BLUE");
                    }
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        //Orta
                        isAnimating = true;
                        CmdChangeState(0, "GREY");
                    }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        //Orta
                        isAnimating = true;
                        CmdChangeState(0, "GREY");
                    }
                    break;
            }   
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (stateIndex == 0 && !isReady) return;
            CmdToggleReady(!isReady);
        }
    }

    [Command]
    void CmdChangeState(int newIndex, string character)
    {
        stateIndex = newIndex;
        selectedCharacter = character;
    }
    [Command]
    void CmdToggleReady(bool readyStatus)
    {
        if (readyStatus == false)
        {
            isReady = false;
            return;
        }

        if (stateIndex == 0) return;

        LobbyPlayer[] allPlayers = FindObjectsOfType<LobbyPlayer>();
        foreach (LobbyPlayer player in allPlayers)
        {
            if (player == this)
            {
                continue;
            }

            if (player.isReady && player.stateIndex == this.stateIndex)
            {
                return; // Ýsteði reddet, 'isReady = true' YAPMA
            }
        }
        isReady = true;
        CheckIfAllPlayersAreReady();
    }

    private void CheckIfAllPlayersAreReady()
    {
        LobbyPlayer[] allPlayers = FindObjectsOfType<LobbyPlayer>();

        if (allPlayers.Length < 2) return;

        // 2. Oyuncularýn HEPSÝ hazýr mý?
        //    Linq kullanarak: allPlayers listesindeki .All(her bir 'p' için) p.isReady true mu?
        if (allPlayers.All(p => p.isReady))
        {
            // HERKES HAZIR!
            // Oyunu baþlatmasý için NetworkManager'a haber ver.
            // (AdvancedNetworkManager script'inde bir 'Singleton' olmalý)
            AdvancedNetworkManager.instance.StartGame();
        }
    }

    void OnColorChanged(int oldIndex, int newIndex)
    {
        transform.DOKill();
        spriteRenderer.DOKill();

        Tween mainTween;

        switch (newIndex)
        {
            case 0:
                //Orta - Siyah
                mainTween = transform.DOMoveX(0f, animationDuration);
                transform.DOScale(new Vector3(originalScaleOfset, originalScaleOfset, originalScaleOfset), animationDuration);
                spriteRenderer.DOColor(Color.black, animationDuration);
                break;
            case 1:
                //Sol - Kýrmýzý
                spriteRenderer.sprite = redSprite;
                mainTween = transform.DOMoveX(-moveOfset, animationDuration);
                transform.DOScale(new Vector3(scaleOfset, scaleOfset, scaleOfset), animationDuration);
                spriteRenderer.DOColor(Color.white, animationDuration);
                break;
            case 2:
                //Sað - Mavi
                spriteRenderer.sprite = blueSprite;
                mainTween = transform.DOMoveX(moveOfset, animationDuration);
                transform.DOScale(new Vector3(scaleOfset, scaleOfset, scaleOfset), animationDuration);
                spriteRenderer.DOColor(Color.white, animationDuration);
                break;
            default:
                //Hata Durumu - Sýfýrla
                mainTween = transform.DOMoveX(0f, 0.1f);
                transform.DOScale(new Vector3(originalScaleOfset, originalScaleOfset, originalScaleOfset), 0.1f);
                spriteRenderer.DOColor(Color.black, 0.1f);
                isAnimating = false;
                break;
        }

        if (isOwned)
        {
            mainTween.OnComplete(() =>
            {
                isAnimating = false;
            });
        }
    }
    void OnReadyChanged(bool oldReady, bool newReady)
    {

    }

}
