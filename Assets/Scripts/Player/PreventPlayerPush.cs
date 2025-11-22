using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PreventPlayerPush : MonoBehaviour
{
    private Rigidbody2D rb;

    [Tooltip("Tag to detect other players (default: Player)")]
    public string playerTag = "Player";

    [Tooltip("If contact normal's |y| is less than this, treat as side collision")]
    public float horizontalCollisionThreshold = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // only care collisions with other players
        if (!collision.collider.CompareTag(playerTag) && !collision.gameObject.CompareTag(playerTag))
            return;

        // determine if there's a mostly-horizontal contact (side collision)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.y) < horizontalCollisionThreshold)
            {
                // cancel horizontal push by zeroing horizontal velocity
                rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }
        }
    }
}
