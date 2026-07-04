using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldMono : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private Transform worldCenter;

    [Tooltip("游戏开始时是否默认激活这个 World。通常只给初始 World 勾选。")]
    [SerializeField] private bool activeOnStart = false;

    [Header("Rotation Settings")]
    [SerializeField] private float rotateSpeed = 180f;

    [SerializeField] private float currentGlobalAngle = 0f;

    [Header("Select Priority")]
    [SerializeField] private int priority = 0;

    [Header("Runtime State")]
    [SerializeField] private bool isActiveWorld = false;

    private readonly List<ObstacleMono> obstacles = new List<ObstacleMono>();

    private Collider2D worldCollider;
    private bool initialized = false;

    public Transform WorldCenter
    {
        get
        {
            return worldCenter != null ? worldCenter : transform;
        }
    }

    public bool ActiveOnStart
    {
        get
        {
            return activeOnStart;
        }
    }

    public int Priority
    {
        get
        {
            return priority;
        }
    }

    public bool IsActiveWorld
    {
        get
        {
            return isActiveWorld;
        }
    }

    public bool EnableRotationRecord
    {
        get
        {
            return isActiveWorld;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Update()
    {
        EnsureInitialized();

        if (!isActiveWorld)
        {
            return;
        }

        bool angleChanged = HandleRotateInput();

        if (angleChanged)
        {
            UpdateChildObstacles();
        }
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        worldCollider = GetComponent<Collider2D>();
        worldCollider.isTrigger = true;

        if (worldCenter == null)
        {
            worldCenter = transform;
        }

        CacheChildObstacles();

        initialized = true;
    }

    private bool HandleRotateInput()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            input += 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            input -= 1f;
        }

        if (Mathf.Approximately(input, 0f))
        {
            return false;
        }

        currentGlobalAngle += input * rotateSpeed * Time.deltaTime;

        return true;
    }

    private void UpdateChildObstacles()
    {
        if (!isActiveWorld)
        {
            return;
        }

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            ObstacleMono obstacle = obstacles[i];

            if (obstacle == null)
            {
                obstacles.RemoveAt(i);
                continue;
            }

            obstacle.FollowWorldAngle(currentGlobalAngle);
        }
    }

    private void CacheChildObstacles()
    {
        obstacles.Clear();

        ObstacleMono[] childObstacles = GetComponentsInChildren<ObstacleMono>(true);

        for (int i = 0; i < childObstacles.Length; i++)
        {
            ObstacleMono obstacle = childObstacles[i];

            if (obstacle == null)
            {
                continue;
            }

            obstacles.Add(obstacle);
            obstacle.BindWorld(this);
        }
    }

    public void RefreshChildObstacles()
    {
        EnsureInitialized();

        CacheChildObstacles();

        if (isActiveWorld)
        {
            UpdateChildObstacles();
        }
    }

    public void SetWorldActive(bool active)
    {
        EnsureInitialized();

        if (isActiveWorld == active)
        {
            if (isActiveWorld)
            {
                UpdateChildObstacles();
            }

            return;
        }

        isActiveWorld = active;

        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            ObstacleMono obstacle = obstacles[i];

            if (obstacle == null)
            {
                obstacles.RemoveAt(i);
                continue;
            }

            obstacle.OnOwnerWorldActiveChanged(active);
        }

        if (isActiveWorld)
        {
            UpdateChildObstacles();
        }
    }

    public void SetEnableRotationRecord(bool enable)
    {
        SetWorldActive(enable);
    }

    public float GetCurrentGlobalAngle()
    {
        return currentGlobalAngle;
    }

    public float GetCurrentGlobelAngle()
    {
        return currentGlobalAngle;
    }

    public bool ContainsPoint(Vector2 point)
    {
        EnsureInitialized();

        if (worldCollider == null)
        {
            return false;
        }

        if (!worldCollider.enabled)
        {
            return false;
        }

        if (!gameObject.activeInHierarchy)
        {
            return false;
        }

        return worldCollider.OverlapPoint(point);
    }

    private void OnValidate()
    {
        rotateSpeed = Mathf.Max(0f, rotateSpeed);
    }
}