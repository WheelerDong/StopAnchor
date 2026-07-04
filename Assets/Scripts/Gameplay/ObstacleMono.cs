using System.Collections;
using UnityEngine;

public class ObstacleMono : MonoBehaviour
{
    [Header("Rotation Target")]
    [SerializeField] private Transform rotateTarget;

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

    private WorldMono ownerWorld;

    private Vector3 defaultOffsetFromCenter;
    private Quaternion defaultRotation;

    // rotateTarget 当前相对初始状态的旋转角度
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

    private void Start()
    {
        InitializeDefaultState();
    }

    public void BindWorld(WorldMono world)
    {
        ownerWorld = world;

        if (rotationCenter == null && ownerWorld != null)
        {
            rotationCenter = ownerWorld.WorldCenter;
        }

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
        canRotate = true;
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
        if (IsPaused)
        {
            return;
        }

        if (isNormalObstacle)
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

        if (!CanReceiveWorldControl())
        {
            return;
        }

        if (!InitializeDefaultState())
        {
            return;
        }

        canRotate = false;

        StopReturnCoroutineOnly();

        returnCoroutine = StartCoroutine(ReturnToWorldAngle());
    }

    private IEnumerator ReturnToWorldAngle()
    {
        // 暂停期间不计入 returnDelay
        yield return WaitForUnpausedSeconds(returnDelay);

        // 如果 delay 正好结束时处于暂停状态，等恢复后再继续
        yield return WaitUntilUnpaused();

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.GiveAnchorBack();
        }

        if (!CanReceiveWorldControl())
        {
            returnCoroutine = null;
            canRotate = true;
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
                    returnSpeed * Time.deltaTime
                );

                RotateToAngle(nextAngle);

                yield return null;
            }

            if (CanReceiveWorldControl())
            {
                SnapToAngle(GetWorldTargetAngle());
            }
        }

        returnCoroutine = null;

        if (unlockAfterReturn)
        {
            canRotate = true;
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

        RotateAroundCenter(deltaAngle);

        currentAngle = targetAngle;
    }

    private void RotateAroundCenter(float deltaAngle)
    {
        if (rotateTarget == null || rotationCenter == null)
        {
            return;
        }

        rotateTarget.RotateAround(
            rotationCenter.position,
            Vector3.forward,
            deltaAngle
        );
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

        Quaternion angleRotation = Quaternion.AngleAxis(
            angle,
            Vector3.forward
        );

        rotateTarget.position =
            rotationCenter.position + angleRotation * defaultOffsetFromCenter;

        rotateTarget.rotation =
            angleRotation * defaultRotation;

        currentAngle = angle;
    }

    private bool InitializeDefaultState()
    {
        if (initialized)
        {
            return true;
        }

        if (rotateTarget == null)
        {
            Debug.LogWarning($"{name} 没有设置 rotateTarget", this);
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
        // 例如：玩家死亡、重开关卡、播放特效等
        GameplayManager.Instance.RestartLevel();
    }
}