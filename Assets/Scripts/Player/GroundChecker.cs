using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private PlayerController playerController;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float checkDistance = 0.43f;
    [SerializeField] private float coyoteTime = 0.1f;

    Vector2 origin;
    RaycastHit2D hit;
    private float coyoteTimer;
    private void Awake()
    {
        if (playerController == null) playerController = GetComponentInParent<PlayerController>();
    }
    private void Start()    
    {
        boxSize = new Vector2(transform.localScale.x, 0.01f);
    }

    private void Update()
    {
        origin = transform.position;
        hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, checkDistance, groundLayer);

        if (hit.collider != null)
        {
            coyoteTimer = 0f;
            playerController.IsGrounded = true;
        }
        else
        {
            coyoteTimer += Time.deltaTime;
            playerController.IsGrounded = coyoteTimer < coyoteTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerController == null) return;
        Gizmos.color = playerController.IsGrounded ? Color.green : Color.red;   
        Vector3 origin = transform.position;
        Gizmos.DrawWireCube(origin + Vector3.down * checkDistance, boxSize);
    }
}
