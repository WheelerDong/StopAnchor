using UnityEngine;

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

    // 玩家相对 stickPoint 的旋转偏移，2D 只需要 Z 轴角度
    private float rotationOffsetZ;

    // 用于脱离时继承速度
    private Vector3 lastFollowPosition;
    private float lastFollowRotationZ;
    private Vector2 followVelocity;
    private float followAngularVelocity;
    private bool hasLastFollowPose = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        originBodyType = rb.bodyType;
        originGravityScale = rb.gravityScale;
        originSimulated = rb.simulated;
    }

    private void Update()
    {
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

    private void LateUpdate()
    {
        if (!isSticking || stickingPoint == null)
        {
            return;
        }

        FollowStickPointImmediately();
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
        float targetRotationZ = stickTrans.eulerAngles.z + rotationOffsetZ;

        // 计算粘住期间的跟随速度，方便脱离时继承
        if (hasLastFollowPose && Time.deltaTime > 0f)
        {
            followVelocity = (targetPosition - lastFollowPosition) / Time.deltaTime;
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

        // 记录玩家相对 stickPoint 的旋转偏移
        rotationOffsetZ = Mathf.DeltaAngle(stickTrans.eulerAngles.z, transform.eulerAngles.z);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 关键点：
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