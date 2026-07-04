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

    [SerializeField] private bool unlockAfterReturn = true;

    private WorldMono ownerWorld;

    private Vector3 defaultOffsetFromCenter;
    private Quaternion defaultRotation;

    // rotateTarget 当前相对初始状态的旋转角度
    private float currentAngle = 0f;

    private bool canRotate = true;
    private bool initialized = false;

    private Coroutine returnCoroutine;

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
        yield return new WaitForSeconds(returnDelay);

        GameplayManager.Instance.GiveAnchorBack();
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
}