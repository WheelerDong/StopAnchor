using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpSpeed = 8f;
    public float rechargeTouchTime = 0.15f;

    [Header("Ground Check")]
    public LayerMask groundLayer;

    [Tooltip("地面检测圆的半径")]
    public float groundCheckRadius = 0.08f;

    [Tooltip("从玩家底部向下额外检测的距离，不要太大，否则会提前判定落地")]
    public float groundCheckExtraDistance = 0.03f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;

    private int maxJumpCount = 1;
    private int currentJumpCount = 0;

    private float groundTouchTimer = 0f;
    private bool jumpPressed = false;

    private bool hasBeenAirborne = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        bool grounded = IsGrounded();

        if (grounded)
        {
            currentJumpCount = maxJumpCount;
            hasBeenAirborne = false;
        }
        else
        {
            currentJumpCount = 0;
            hasBeenAirborne = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        HandleJump();
        HandleJumpRecharge();
    }

    private void HandleJump()
    {
        if (!jumpPressed)
            return;

        jumpPressed = false;

        if (currentJumpCount <= 0)
            return;

        currentJumpCount--;

        groundTouchTimer = 0f;
        hasBeenAirborne = false;

        // 世界坐标垂直向上跳跃
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
    }

    private void HandleJumpRecharge()
    {
        bool grounded = IsGrounded();

        if (!grounded)
        {
            hasBeenAirborne = true;
            groundTouchTimer = 0f;
            return;
        }

        // 必须离开过地面，并且现在不是向上运动，才开始恢复跳跃
        if (hasBeenAirborne && rb.velocity.y <= 0.01f)
        {
            groundTouchTimer += Time.fixedDeltaTime;

            if (groundTouchTimer >= rechargeTouchTime && currentJumpCount < maxJumpCount)
            {
                currentJumpCount = maxJumpCount;
                hasBeenAirborne = false;
                groundTouchTimer = 0f;
            }
        }
        else
        {
            groundTouchTimer = 0f;
        }
    }

    private bool IsGrounded()
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = bounds.center;

        // 从玩家中心向世界坐标正下方检测
        float castDistance = bounds.extents.y + groundCheckExtraDistance;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            groundCheckRadius,
            Vector2.down,
            castDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (bodyCollider == null)
            return;

        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = bounds.center;
        float castDistance = bounds.extents.y + groundCheckExtraDistance;
        Vector2 end = origin + Vector2.down * castDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
        Gizmos.DrawLine(origin, end);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(end, groundCheckRadius);
    }
}