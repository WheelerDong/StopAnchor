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
    private PlayerMono playerMono;

    private int maxJumpCount = 1;
    private int currentJumpCount = 0;

    private float groundTouchTimer = 0f;
    private bool jumpPressed = false;

    private bool hasBeenAirborne = false;

    // 最近一次脚下碰撞体的法线
    private Vector2 lastGroundNormal = Vector2.up;

    private bool IsPaused
    {
        get
        {
            return GameplayManager.Instance != null && GameplayManager.Instance.IsPaused;
        }
    }

    private bool IsSticking
    {
        get
        {
            return playerMono != null && playerMono.IsSticking;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        // PlayerController 和 PlayerMono 在同一个 GameObject 上
        playerMono = GetComponent<PlayerMono>();
    }

    private void Start()
    {
        bool grounded = TryGetGroundHit(out RaycastHit2D hit);

        if (grounded)
        {
            lastGroundNormal = hit.normal.normalized;
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
        if (IsPaused)
        {
            jumpPressed = false;
            return;
        }

        // 注意：
        // 这里不要判断 IsSticking。
        // 只记录玩家是否按下了跳跃键。
        // 真正能不能跳，在 HandleJump() 里判断。
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        if (IsPaused)
        {
            jumpPressed = false;
            return;
        }

        // 注意：
        // 这里也不要因为 IsSticking 直接 return。
        // 只在真正执行跳跃时判断是否吸附。
        HandleJump();
        HandleJumpRecharge();
    }

    private void HandleJump()
    {
        if (!jumpPressed)
            return;

        jumpPressed = false;

        // 只在玩家真的尝试跳跃时，检测是否正在吸附
        if (IsSticking)
            return;

        if (currentJumpCount <= 0)
            return;

        currentJumpCount--;

        groundTouchTimer = 0f;
        hasBeenAirborne = false;

        Vector2 jumpNormal = lastGroundNormal;

        // 跳跃这一帧，再尝试刷新一次脚下法线，确保方向是最新的
        if (TryGetGroundHit(out RaycastHit2D hit))
        {
            jumpNormal = hit.normal.normalized;
            lastGroundNormal = jumpNormal;
        }

        // 保留沿地面切线方向的速度，只替换法线方向速度
        Vector2 currentVelocity = rb.velocity;
        float normalVelocity = Vector2.Dot(currentVelocity, jumpNormal);
        Vector2 tangentVelocity = currentVelocity - jumpNormal * normalVelocity;

        rb.velocity = tangentVelocity + jumpNormal * jumpSpeed;
    }

    private void HandleJumpRecharge()
    {
        bool grounded = TryGetGroundHit(out RaycastHit2D hit);

        if (!grounded)
        {
            hasBeenAirborne = true;
            groundTouchTimer = 0f;
            return;
        }

        lastGroundNormal = hit.normal.normalized;

        // 不再使用 rb.velocity.y，而是使用角色沿地面法线方向的速度
        float velocityAlongGroundNormal = Vector2.Dot(rb.velocity, lastGroundNormal);

        // 必须离开过地面，并且现在不是沿地面法线向外运动，才恢复跳跃
        if (hasBeenAirborne && velocityAlongGroundNormal <= 0.01f)
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
        return TryGetGroundHit(out _);
    }

    private bool TryGetGroundHit(out RaycastHit2D hit)
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = bounds.center;
        float castDistance = bounds.extents.y + groundCheckExtraDistance;

        hit = Physics2D.CircleCast(
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

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(end, end + lastGroundNormal * 0.6f);
        }
    }
}