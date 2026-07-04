using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMono : MonoBehaviour
{
    [Header("Stick Settings")]
    [SerializeField] private KeyCode stickKey = KeyCode.S;
    [SerializeField] private KeyCode detachKey = KeyCode.Space;

    private Rigidbody2D rb;

    private stickPoint currentStickPoint;
    private stickPoint stickingPoint;

    private bool isSticking = false;

    private RigidbodyType2D originBodyType;
    private float originGravityScale;

    // 玩家相对 stickPoint 的位置偏移
    private Vector3 localPositionOffset;

    // 玩家相对 stickPoint 的旋转偏移，2D 只需要 Z 轴角度
    private float rotationOffsetZ;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        originBodyType = rb.bodyType;
        originGravityScale = rb.gravityScale;
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

    private void FixedUpdate()
    {
        if (!isSticking || stickingPoint == null)
        {
            return;
        }

        Transform stickTrans = stickingPoint.StickTransform;

        // 根据 stickPoint 当前的位置和旋转，重新计算玩家应该在的位置
        Vector3 targetPosition = stickTrans.TransformPoint(localPositionOffset);

        // 保持玩家相对 stickPoint 的旋转偏移
        float targetRotationZ = stickTrans.eulerAngles.z + rotationOffsetZ;

        rb.MovePosition(targetPosition);
        rb.MoveRotation(targetRotationZ);
    }

    private void StickToPoint(stickPoint point)
    {
        if (point == null)
        {
            return;
        }

        isSticking = true;
        stickingPoint = point;

        Transform stickTrans = point.StickTransform;

        // 记录当前玩家相对 stickPoint 的位置偏移
        localPositionOffset = stickTrans.InverseTransformPoint(transform.position);

        // 记录当前玩家相对 stickPoint 的旋转偏移
        rotationOffsetZ = Mathf.DeltaAngle(stickTrans.eulerAngles.z, transform.eulerAngles.z);

        originBodyType = rb.bodyType;
        originGravityScale = rb.gravityScale;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void DetachFromPoint()
    {
        isSticking = false;
        stickingPoint = null;

        rb.bodyType = originBodyType;
        rb.gravityScale = originGravityScale;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
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