using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldMono : MonoBehaviour
{
    private static readonly List<WorldMono> registeredWorlds = new List<WorldMono>();

    public static WorldMono CurrentActiveWorld { get; private set; }

    [Header("World Settings")]
    [SerializeField] private Transform worldCenter;

    [Tooltip("游戏开始时是否默认激活这个 World。通常只给初始 World 勾选。")]
    [SerializeField] private bool activeOnStart = false;

    [Header("Rotation Settings")]
    [SerializeField] private float rotateSpeed = 180f;

    // 当前这个 World 自己记录的旋转角度
    [SerializeField] private float currentGlobalAngle = 0f;

    [Header("Select Priority")]
    [SerializeField] private int priority = 0;

    [Header("Runtime State")]
    [SerializeField] private bool isActiveWorld = false;

    private readonly List<ObstacleMono> obstacles = new List<ObstacleMono>();

    private Collider2D worldCollider;

    public Transform WorldCenter
    {
        get
        {
            return worldCenter != null ? worldCenter : transform;
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

    // 兼容旧接口命名
    public bool EnableRotationRecord
    {
        get
        {
            return isActiveWorld;
        }
    }

    private void Awake()
    {
        worldCollider = GetComponent<Collider2D>();
        worldCollider.isTrigger = true;

        if (worldCenter == null)
        {
            worldCenter = transform;
        }

        CacheChildObstacles();
    }

    private void OnEnable()
    {
        if (!registeredWorlds.Contains(this))
        {
            registeredWorlds.Add(this);
        }
    }

    private void Start()
    {
        if (activeOnStart)
        {
            SetCurrentActiveWorld(this);
        }
    }

    private void OnDisable()
    {
        registeredWorlds.Remove(this);

        if (CurrentActiveWorld == this)
        {
            SetCurrentActiveWorld(null);
        }
    }

    private void Update()
    {
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

    private bool HandleRotateInput()
    {
        float input = 0f;

        // A：逆时针
        if (Input.GetKey(KeyCode.A))
        {
            input += 1f;
        }

        // D：顺时针
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
        CacheChildObstacles();

        if (isActiveWorld)
        {
            UpdateChildObstacles();
        }
    }

    public void SetWorldActive(bool active)
    {
        if (active)
        {
            SetCurrentActiveWorld(this);
        }
        else
        {
            if (CurrentActiveWorld == this)
            {
                SetCurrentActiveWorld(null);
            }
            else
            {
                SetInternalActiveState(false);
            }
        }
    }

    public static void SetCurrentActiveWorld(WorldMono world)
    {
        if (world != null)
        {
            if (!world.isActiveAndEnabled || !world.gameObject.activeInHierarchy)
            {
                world = null;
            }
        }

        if (CurrentActiveWorld == world)
        {
            if (CurrentActiveWorld != null && !CurrentActiveWorld.isActiveWorld)
            {
                CurrentActiveWorld.SetInternalActiveState(true);
            }

            return;
        }

        if (CurrentActiveWorld != null)
        {
            CurrentActiveWorld.SetInternalActiveState(false);
        }

        CurrentActiveWorld = world;

        if (CurrentActiveWorld != null)
        {
            CurrentActiveWorld.SetInternalActiveState(true);
        }
    }

    private void SetInternalActiveState(bool active)
    {
        if (isActiveWorld == active)
        {
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

    // 兼容旧接口：以前你可能在其他脚本里调用了这个
    public void SetEnableRotationRecord(bool enable)
    {
        SetWorldActive(enable);
    }

    public float GetCurrentGlobalAngle()
    {
        return currentGlobalAngle;
    }

    // 保留旧拼写接口，避免其他脚本里已经写了 Globel 导致报错
    public float GetCurrentGlobelAngle()
    {
        return currentGlobalAngle;
    }

    public bool ContainsPoint(Vector2 point)
    {
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

    public static WorldMono FindWorldByPosition(Vector2 playerPosition)
    {
        WorldMono bestWorld = null;

        int bestPriority = int.MinValue;
        float bestDistanceSqr = float.MaxValue;

        for (int i = registeredWorlds.Count - 1; i >= 0; i--)
        {
            WorldMono world = registeredWorlds[i];

            if (world == null)
            {
                registeredWorlds.RemoveAt(i);
                continue;
            }

            if (!world.ContainsPoint(playerPosition))
            {
                continue;
            }

            int currentPriority = world.Priority;

            float currentDistanceSqr =
                ((Vector2)world.WorldCenter.position - playerPosition).sqrMagnitude;

            bool shouldSelect = false;

            if (bestWorld == null)
            {
                shouldSelect = true;
            }
            else if (currentPriority > bestPriority)
            {
                shouldSelect = true;
            }
            else if (currentPriority == bestPriority && currentDistanceSqr < bestDistanceSqr)
            {
                shouldSelect = true;
            }

            if (shouldSelect)
            {
                bestWorld = world;
                bestPriority = currentPriority;
                bestDistanceSqr = currentDistanceSqr;
            }
        }

        return bestWorld;
    }

    private void OnValidate()
    {
        rotateSpeed = Mathf.Max(0f, rotateSpeed);
    }
}