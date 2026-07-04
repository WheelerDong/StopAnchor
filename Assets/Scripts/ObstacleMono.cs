using System.Collections;
using UnityEngine;

public class ObstacleMono : MonoBehaviour
{
    [Header("Rotation Center")]
    [SerializeField] private Transform rotationCenter;

    [Header("Return Settings")]
    [SerializeField] private float returnDelay = 3f;       // 点击后多久开始回弹
    [SerializeField] private float returnSpeed = 180f;     // 回弹速度，单位：度/秒

    [Header("Options")]
    [SerializeField] private bool rotateSelf = true;
    [SerializeField] private bool unlockAfterReturn = true;

    private Vector3 defaultOffsetFromCenter;
    private Quaternion defaultRotation;

    // 当前相对初始状态的旋转角度
    private float currentAngle = 0f;

    private bool canRotate = true;
    private bool initialized = false;

    private Coroutine returnCoroutine;

    private void Start()
    {
        InitializeDefaultState();
    }

    private void Update()
    {
        if (!canRotate)
        {
            return;
        }

        HandleRotateInput();
    }

    private void HandleRotateInput()
    {
        RotateToAngle(GetGameplayTargetAngle());
    }

    private void OnMouseDown()
    {
        LockAndReturn();
    }

    public void LockAndReturn()
    {
        if (!InitializeDefaultState())
        {
            return;
        }

        canRotate = false;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        returnCoroutine = StartCoroutine(ReturnToGameplayAngle());
    }

    private IEnumerator ReturnToGameplayAngle()
    {
        yield return new WaitForSeconds(returnDelay);

        while (true)
        {
            // 每一帧都重新读取 GameplayManager 的角度
            // 如果 GameplayManager 里的值变化，障碍物会实时追踪新的目标
            float targetAngle = GetGameplayTargetAngle();

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

        // 最后强制对齐到 GameplayManager 的最新角度
        SnapToAngle(GetGameplayTargetAngle());

        returnCoroutine = null;

        if (unlockAfterReturn)
        {
            canRotate = true;
        }
    }

    private float GetGameplayTargetAngle()
    {
        if (GameplayManager.Instance == null)
        {
            return currentAngle;
        }

        return GameplayManager.Instance.GetCurrentGlobelAngle();
    }

    private void RotateToAngle(float targetAngle)
    {
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
        if (rotationCenter == null)
        {
            return;
        }

        Quaternion oldRotation = transform.rotation;

        transform.RotateAround(
            rotationCenter.position,
            Vector3.forward,
            deltaAngle
        );

        if (!rotateSelf)
        {
            transform.rotation = oldRotation;
        }
    }

    private void SnapToAngle(float angle)
    {
        if (rotationCenter == null)
        {
            return;
        }

        Quaternion angleRotation = Quaternion.AngleAxis(
            angle,
            Vector3.forward
        );

        transform.position =
            rotationCenter.position + angleRotation * defaultOffsetFromCenter;

        if (rotateSelf)
        {
            transform.rotation = angleRotation * defaultRotation;
        }
        else
        {
            transform.rotation = defaultRotation;
        }

        currentAngle = angle;
    }

    private bool InitializeDefaultState()
    {
        if (initialized)
        {
            return true;
        }

        if (rotationCenter == null)
        {
            Debug.LogWarning($"{name} 没有设置 rotationCenter");
            return false;
        }

        defaultOffsetFromCenter =
            transform.position - rotationCenter.position;

        defaultRotation = transform.rotation;

        currentAngle = 0f;
        initialized = true;

        return true;
    }

    private void OnValidate()
    {
        returnDelay = Mathf.Max(0f, returnDelay);
        returnSpeed = Mathf.Max(0f, returnSpeed);
    }
}