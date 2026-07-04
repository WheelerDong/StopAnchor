using UnityEngine;

[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMono : MonoBehaviour
{
    [Header("Stick Settings")]
    [SerializeField] private KeyCode stickKey = KeyCode.S;
    [SerializeField] private KeyCode detachKey = KeyCode.Space;

    [Header("Detach Settings")]
    [SerializeField] private bool inheritStickPointVelocityOnDetach = false;

    private Rigidbody2D rb;

    private stickPoint currentStickPoint;
    private stickPoint stickingPoint;

    private bool isSticking = false;

    private RigidbodyType2D originBodyType;
    private float originGravityScale;
    private bool originSimulated;

    // 玩家相对 stickPoint 的位置偏移
    private Vector3 localPositionOffset;

    // 粘住时玩家自己的世界旋转角度。粘住期间保持这个角度不变。
    private float fixedStickRotationZ;

    // 用于脱离时继承速度
    private Vector3 lastFollowPosition;
    private float lastFollowRotationZ;
    private Vector2 followVelocity;
    private float followAngularVelocity;
    private bool hasLastFollowPose = false;

    // Pause Freeze
    private bool isPauseFrozen = false;

    private Vector3 pausePosition;
    private Quaternion pauseRotation;

    private Vector2 pauseVelocity;
    private float pauseAngularVelocity;

    private bool pauseRbSimulated;
    private RigidbodyType2D pauseBodyType;
    private float pauseGravityScale;

    private bool IsPaused
    {
        get
        {
            return GameplayManager.Instance != null && GameplayManager.Instance.IsPaused;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        originBodyType = rb.bodyType;
        originGravityScale = rb.gravityScale;
        originSimulated = rb.simulated;
    }

    private void Update()
    {
        HandlePauseFreezeState();

        if (isPauseFrozen)
        {
            return;
        }

        if (!isSticking)
        {
            if (currentStickPoint != null && Input.GetKeyDown(stickKey))
            {
                StickToPoint(currentStickPoint);
            }
        }
        else
        {
            if (Input.GetKeyDown(detachKey))
            {
                DetachFromPoint();
            }
        }
    }

    private void FixedUpdate()
    {
        HandlePauseFreezeState();

        if (isPauseFrozen)
        {
            KeepPauseFrozenPose();
        }
    }

    private void LateUpdate()
    {
        HandlePauseFreezeState();

        if (isPauseFrozen)
        {
            KeepPauseFrozenPose();
            return;
        }

        if (!isSticking || stickingPoint == null)
        {
            return;
        }

        FollowStickPointImmediately();
    }

    private void HandlePauseFreezeState()
    {
        if (IsPaused)
        {
            if (!isPauseFrozen)
            {
                EnterPauseFreeze();
            }

            KeepPauseFrozenPose();
        }
        else
        {
            if (isPauseFrozen)
            {
                ExitPauseFreeze();
            }
        }
    }

    private void EnterPauseFreeze()
    {
        isPauseFrozen = true;

        pausePosition = transform.position;
        pauseRotation = transform.rotation;

        pauseVelocity = rb.velocity;
        pauseAngularVelocity = rb.angularVelocity;

        pauseRbSimulated = rb.simulated;
        pauseBodyType = rb.bodyType;
        pauseGravityScale = rb.gravityScale;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 暂停期间关闭物理模拟，确保重力、速度、碰撞都不会继续推动玩家
        rb.simulated = false;

        KeepPauseFrozenPose();
    }

    private void KeepPauseFrozenPose()
    {
        transform.SetPositionAndRotation(pausePosition, pauseRotation);

        rb.position = pausePosition;
        rb.rotation = pauseRotation.eulerAngles.z;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void ExitPauseFreeze()
    {
        transform.SetPositionAndRotation(pausePosition, pauseRotation);

        rb.position = pausePosition;
        rb.rotation = pauseRotation.eulerAngles.z;

        if (isSticking)
        {
            // 粘住状态恢复后，仍然保持粘住，但不要恢复物理模拟
            rb.bodyType = originBodyType;
            rb.gravityScale = originGravityScale;
            rb.simulated = false;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            RefreshStickOffsetFromCurrentPose();

            hasLastFollowPose = false;
            followVelocity = Vector2.zero;
            followAngularVelocity = 0f;
        }
        else
        {
            // 非粘住状态，恢复暂停前的物理状态
            rb.bodyType = pauseBodyType;
            rb.gravityScale = pauseGravityScale;
            rb.simulated = pauseRbSimulated;

            rb.velocity = pauseVelocity;
            rb.angularVelocity = pauseAngularVelocity;
        }

        isPauseFrozen = false;
    }

    private void RefreshStickOffsetFromCurrentPose()
    {
        if (!isSticking || stickingPoint == null)
        {
            return;
        }

        Transform stickTrans = stickingPoint.StickTransform;

        if (stickTrans == null)
        {
            return;
        }

        localPositionOffset = stickTrans.InverseTransformPoint(transform.position);

        // 暂停恢复后，以当前玩家世界旋转作为继续固定的角度
        fixedStickRotationZ = transform.eulerAngles.z;
    }

    private void FollowStickPointImmediately()
    {
        Transform stickTrans = stickingPoint.StickTransform;

        if (stickTrans == null)
        {
            DetachFromPoint();
            return;
        }

        Vector3 targetPosition = stickTrans.TransformPoint(localPositionOffset);

        // 重点：粘住后玩家自己的旋转保持不变，不再跟随 stickPoint 旋转
        float targetRotationZ = fixedStickRotationZ;

        // 计算粘住期间的跟随速度，方便脱离时继承
        if (hasLastFollowPose && Time.deltaTime > 0f)
        {
            followVelocity = (targetPosition - lastFollowPosition) / Time.deltaTime;

            // 因为玩家旋转保持不变，所以正常情况下这里会是 0
            followAngularVelocity = Mathf.DeltaAngle(lastFollowRotationZ, targetRotationZ) / Time.deltaTime;
        }

        lastFollowPosition = targetPosition;
        lastFollowRotationZ = targetRotationZ;
        hasLastFollowPose = true;

        // 粘住期间直接同步 Transform，不再通过物理系统追赶
        transform.SetPositionAndRotation(
            targetPosition,
            Quaternion.Euler(0f, 0f, targetRotationZ)
        );
    }

    private void StickToPoint(stickPoint point)
    {
        if (point == null)
        {
            return;
        }

        Transform stickTrans = point.StickTransform;

        if (stickTrans == null)
        {
            return;
        }

        isSticking = true;
        stickingPoint = point;

        // 记录原始物理状态
        originBodyType = rb.bodyType;
        originGravityScale = rb.gravityScale;
        originSimulated = rb.simulated;

        // 记录玩家相对 stickPoint 的位置偏移
        localPositionOffset = stickTrans.InverseTransformPoint(transform.position);

        // 记录粘住瞬间玩家自己的世界旋转角度
        fixedStickRotationZ = transform.eulerAngles.z;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 粘住期间关闭 Rigidbody2D 模拟，避免物理系统和 Transform 跟随互相打架
        rb.simulated = false;

        hasLastFollowPose = false;
        followVelocity = Vector2.zero;
        followAngularVelocity = 0f;

        // 立刻同步一次，避免按下 S 的这一帧产生视觉延迟
        FollowStickPointImmediately();
    }

    private void DetachFromPoint()
    {
        if (!isSticking)
        {
            return;
        }

        isSticking = false;
        stickingPoint = null;

        // 恢复物理模拟
        rb.simulated = originSimulated;
        rb.bodyType = originBodyType;
        rb.gravityScale = originGravityScale;

        // 确保 Rigidbody2D 的物理位置与当前 Transform 一致
        rb.position = transform.position;
        rb.rotation = transform.eulerAngles.z;

        if (inheritStickPointVelocityOnDetach)
        {
            rb.velocity = followVelocity;

            // 玩家粘住期间自身不旋转，所以这里通常是 0
            rb.angularVelocity = followAngularVelocity;
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        hasLastFollowPose = false;

        // 脱离后重新等待触发检测
        currentStickPoint = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        stickPoint point = other.GetComponent<stickPoint>();

        if (point != null)
        {
            currentStickPoint = point;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        stickPoint point = other.GetComponent<stickPoint>();

        if (point == null)
        {
            return;
        }

        if (currentStickPoint == point && !isSticking)
        {
            currentStickPoint = null;
        }
    }
}