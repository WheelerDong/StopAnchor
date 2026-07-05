using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObstacleMono : MonoBehaviour
{
    private Transform rotateTarget;
    private Rigidbody2D rotateTargetRigidbody2D;

    [Header("Rotation Center")]
    [SerializeField] private Transform rotationCenter;

    [Header("Return Settings")]
    [SerializeField] private float returnDelay = 3f;
    [SerializeField] private float returnSpeed = 180f;

    [Header("Options")]
    [Tooltip("true：普通障碍物，只跟随 World 旋转；false：点击后会锁定并回弹")]
    [SerializeField] private bool isNormalObstacle = false;

    [Tooltip("true：陷阱障碍，玩家踩到后会触发陷阱逻辑")]
    [SerializeField] private bool isTrapObstacle = false;

    [SerializeField] private bool unlockAfterReturn = true;

    [Header("Gear")]
    [Tooltip("当 isNormalObstacle 为 false 时必须赋值。锁定时 Pin，解除锁定时 Unpin。")]
    [SerializeField] private GearMono gearMono;

    private WorldMono ownerWorld;

    private Vector3 defaultOffsetFromCenter;
    private Quaternion defaultRotation;

    // 板子当前相对初始状态的旋转角度
    private float currentAngle = 0f;

    private bool canRotate = true;
    private bool initialized = false;

    private Coroutine returnCoroutine;

    private bool IsPaused
    {
        get
        {
            return GameplayManager.Instance != null && GameplayManager.Instance.IsPaused;
        }
    }

    private void Reset()
    {
        CacheAndConfigureRigidbody2D();
    }

    private void Awake()
    {
        CacheAndConfigureRigidbody2D();
    }

    private void Start()
    {
        ValidateGearRequirement();
        InitializeDefaultState();
    }

    private void CacheAndConfigureRigidbody2D()
    {
        // 现在 ObstacleMono 就放在板子本身上，所以 rotateTarget 永远是自身 transform
        rotateTarget = transform;

        rotateTargetRigidbody2D = GetComponent<Rigidbody2D>();

        if (rotateTargetRigidbody2D == null)
        {
            return;
        }

        // 旋转平台建议使用 Kinematic，由代码控制运动
        rotateTargetRigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        // 平台自身不受重力影响
        rotateTargetRigidbody2D.gravityScale = 0f;

        // 开启插值，视觉上更平滑
        rotateTargetRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 防止旋转/移动过快时产生明显穿透
        rotateTargetRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 确保物理模拟开启
        rotateTargetRigidbody2D.simulated = true;

        // Kinematic 物体也产生更完整的碰撞接触
        rotateTargetRigidbody2D.useFullKinematicContacts = true;

        // 不冻结位置和旋转，否则 MovePosition / MoveRotation 可能表现异常
        rotateTargetRigidbody2D.constraints = RigidbodyConstraints2D.None;
    }

    public void BindWorld(WorldMono world)
    {
        ownerWorld = world;

        if (rotationCenter == null && ownerWorld != null)
        {
            rotationCenter = ownerWorld.WorldCenter;
        }

        ValidateGearRequirement();
        InitializeDefaultState();
    }

    public void OnOwnerWorldActiveChanged(bool active)
    {
        if (active)
        {
            return;
        }

        StopReturnCoroutineOnly();

        // World 失活后，不保留锁死状态，防止之后重新进入时无法继续跟随
        UnlockObstacle();
    }

    public void FollowWorldAngle(float worldAngle)
    {
        if (IsPaused)
        {
            return;
        }

        if (!CanReceiveWorldControl())
        {
            return;
        }

        if (!canRotate)
        {
            return;
        }

        RotateToAngle(worldAngle);
    }

    private void OnMouseDown()
    {
        Debug.Log("鼠标点击");

        if (IsPaused)
        {
            return;
        }

        if (isNormalObstacle)
        {
            return;
        }

        if (!canRotate)
        {
            return;
        }

        if (!ValidateGearRequirement())
        {
            return;
        }

        if (!CanReceiveWorldControl())
        {
            return;
        }

        if (GameplayManager.Instance.TryUseAnchor())
        {
            LockAndReturn();
        }
    }

    public void LockAndReturn()
    {
        if (IsPaused)
        {
            return;
        }

        if (!canRotate)
        {
            return;
        }

        if (!ValidateGearRequirement())
        {
            return;
        }

        if (!CanReceiveWorldControl())
        {
            return;
        }

        if (!InitializeDefaultState())
        {
            return;
        }

        canRotate = false;
        PinGear();

        StopReturnCoroutineOnly();

        returnCoroutine = StartCoroutine(ReturnToWorldAngle());
    }

    private IEnumerator ReturnToWorldAngle()
    {
        yield return WaitForUnpausedSeconds(returnDelay);
        yield return WaitUntilUnpaused();

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.GiveAnchorBack();
        }

        if (!CanReceiveWorldControl())
        {
            returnCoroutine = null;
            UnlockObstacle();
            yield break;
        }

        if (returnSpeed <= 0f)
        {
            SnapToAngle(GetWorldTargetAngle());
        }
        else
        {
            while (CanReceiveWorldControl())
            {
                if (IsPaused)
                {
                    yield return null;
                    continue;
                }

                float targetAngle = GetWorldTargetAngle();

                if (Mathf.Abs(currentAngle - targetAngle) <= 0.01f)
                {
                    break;
                }

                float nextAngle = Mathf.MoveTowards(
                    currentAngle,
                    targetAngle,
                    returnSpeed * Time.fixedDeltaTime
                );

                RotateToAngle(nextAngle);

                yield return new WaitForFixedUpdate();
            }

            if (CanReceiveWorldControl())
            {
                SnapToAngle(GetWorldTargetAngle());
            }
        }

        returnCoroutine = null;

        if (unlockAfterReturn)
        {
            UnlockObstacle();
        }
    }

    private IEnumerator WaitForUnpausedSeconds(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            if (!IsPaused)
            {
                timer += Time.deltaTime;
            }

            yield return null;
        }
    }

    private IEnumerator WaitUntilUnpaused()
    {
        while (IsPaused)
        {
            yield return null;
        }
    }

    private bool CanReceiveWorldControl()
    {
        EnsureOwnerWorld();

        if (ownerWorld == null)
        {
            return false;
        }

        return ownerWorld.IsActiveWorld;
    }

    private void EnsureOwnerWorld()
    {
        if (ownerWorld != null)
        {
            return;
        }

        WorldMono world = GetComponentInParent<WorldMono>();

        if (world != null)
        {
            BindWorld(world);
        }
    }

    private float GetWorldTargetAngle()
    {
        EnsureOwnerWorld();

        if (ownerWorld == null)
        {
            return currentAngle;
        }

        return ownerWorld.GetCurrentGlobalAngle();
    }

    private void RotateToAngle(float targetAngle)
    {
        if (IsPaused)
        {
            return;
        }

        if (!InitializeDefaultState())
        {
            return;
        }

        float deltaAngle = targetAngle - currentAngle;

        if (Mathf.Abs(deltaAngle) < 0.001f)
        {
            return;
        }

        MoveRigidbodyToAngle(targetAngle);

        currentAngle = targetAngle;
    }

    private void SnapToAngle(float angle)
    {
        if (IsPaused)
        {
            return;
        }

        if (!InitializeDefaultState())
        {
            return;
        }

        MoveRigidbodyToAngle(angle);

        currentAngle = angle;
    }

    private void MoveRigidbodyToAngle(float angle)
    {
        if (rotateTargetRigidbody2D == null || rotationCenter == null)
        {
            return;
        }

        Quaternion angleRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 targetWorldPosition =
            rotationCenter.position + angleRotation * defaultOffsetFromCenter;

        Quaternion targetWorldRotation =
            angleRotation * defaultRotation;

        rotateTargetRigidbody2D.MovePosition(targetWorldPosition);
        rotateTargetRigidbody2D.MoveRotation(targetWorldRotation.eulerAngles.z);
    }

    private bool InitializeDefaultState()
    {
        if (initialized)
        {
            return true;
        }

        CacheAndConfigureRigidbody2D();

        if (rotateTarget == null)
        {
            Debug.LogWarning($"{name} 没有设置 rotateTarget", this);
            return false;
        }

        if (rotateTargetRigidbody2D == null)
        {
            Debug.LogError($"{name} 没有 Rigidbody2D，无法使用 MoveRotation", this);
            return false;
        }

        if (rotationCenter == null)
        {
            Debug.LogWarning($"{name} 没有设置 rotationCenter", this);
            return false;
        }

        defaultOffsetFromCenter =
            rotateTarget.position - rotationCenter.position;

        defaultRotation = rotateTarget.rotation;

        currentAngle = 0f;
        initialized = true;

        return true;
    }

    private bool ValidateGearRequirement()
    {
        if (isNormalObstacle)
        {
            return true;
        }

        if (gearMono != null)
        {
            return true;
        }

        Debug.LogError($"{name} 的 isNormalObstacle 为 false，必须设置 GearMono 参数", this);
        return false;
    }

    private void PinGear()
    {
        if (gearMono == null)
        {
            return;
        }

        gearMono.Pin();
    }

    private void UnpinGear()
    {
        if (gearMono == null)
        {
            return;
        }

        gearMono.Unpin();
    }

    private void UnlockObstacle()
    {
        canRotate = true;
        UnpinGear();
    }

    private void StopReturnCoroutineOnly()
    {
        if (returnCoroutine == null)
        {
            return;
        }

        StopCoroutine(returnCoroutine);
        returnCoroutine = null;
    }

    private void OnValidate()
    {
        returnDelay = Mathf.Max(0f, returnDelay);
        returnSpeed = Mathf.Max(0f, returnSpeed);

        CacheAndConfigureRigidbody2D();

        if (!isNormalObstacle && gearMono == null)
        {
            Debug.LogError($"{name} 的 isNormalObstacle 为 false，必须设置 GearMono 参数", this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerTrap(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerTrap(other);
    }

    private void TryTriggerTrap(Collider2D other)
    {
        if (!isTrapObstacle)
        {
            return;
        }

        if (IsPaused)
        {
            return;
        }

        PlayerMono player = other.GetComponentInParent<PlayerMono>();

        if (player == null)
        {
            return;
        }

        TriggerTrap(player);
    }

    private void TriggerTrap(PlayerMono player)
    {
        GameplayManager.Instance.RestartLevel();
    }
}