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
    private readonly List<StarMono> stars = new List<StarMono>();
    private readonly List<LevelBackgroundMono> levelBackgrounds = new List<LevelBackgroundMono>();

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
        if (GameplayManager.Instance.IsPaused)
            return;
            
        EnsureInitialized();

        if (!isActiveWorld)
        {
            return;
        }

        bool angleChanged = HandleRotateInput();

        if (angleChanged)
        {
            UpdateChildRotatables();
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

        CacheChildRotatables();

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

    private void UpdateChildRotatables()
    {
        if (!isActiveWorld)
        {
            return;
        }

        UpdateChildObstacles();
        UpdateChildStars();
        UpdateChildLevelBackgrounds();
    }

    private void UpdateChildObstacles()
    {
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

    private void UpdateChildStars()
    {
        for (int i = stars.Count - 1; i >= 0; i--)
        {
            StarMono star = stars[i];

            if (star == null)
            {
                stars.RemoveAt(i);
                continue;
            }

            star.FollowWorldAngle(currentGlobalAngle);
        }
    }

    private void UpdateChildLevelBackgrounds()
    {
        for (int i = levelBackgrounds.Count - 1; i >= 0; i--)
        {
            LevelBackgroundMono levelBackground = levelBackgrounds[i];

            if (levelBackground == null)
            {
                levelBackgrounds.RemoveAt(i);
                continue;
            }

            levelBackground.FollowWorldAngle(currentGlobalAngle);
        }
    }

    private void CacheChildRotatables()
    {
        CacheChildObstacles();
        CacheChildStars();
        CacheChildLevelBackgrounds();
    }

    private void CacheChildObstacles()
    {
        obstacles.Clear();

        ObstacleMono[] childObstacles =
            GetComponentsInChildren<ObstacleMono>(true);

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

    private void CacheChildStars()
    {
        stars.Clear();

        StarMono[] childStars =
            GetComponentsInChildren<StarMono>(true);

        for (int i = 0; i < childStars.Length; i++)
        {
            StarMono star = childStars[i];

            if (star == null)
            {
                continue;
            }

            stars.Add(star);
            star.BindWorld(this);
        }
    }

    private void CacheChildLevelBackgrounds()
    {
        levelBackgrounds.Clear();

        LevelBackgroundMono[] childLevelBackgrounds =
            GetComponentsInChildren<LevelBackgroundMono>(true);

        for (int i = 0; i < childLevelBackgrounds.Length; i++)
        {
            LevelBackgroundMono levelBackground = childLevelBackgrounds[i];

            if (levelBackground == null)
            {
                continue;
            }

            levelBackgrounds.Add(levelBackground);
            levelBackground.BindWorld(this);
        }
    }

    public void RefreshChildObstacles()
    {
        EnsureInitialized();

        CacheChildRotatables();

        if (isActiveWorld)
        {
            UpdateChildRotatables();
        }
    }

    public void RefreshChildStars()
    {
        EnsureInitialized();

        CacheChildRotatables();

        if (isActiveWorld)
        {
            UpdateChildRotatables();
        }
    }

    public void RefreshChildLevelBackgrounds()
    {
        EnsureInitialized();

        CacheChildRotatables();

        if (isActiveWorld)
        {
            UpdateChildRotatables();
        }
    }

    public void RefreshChildRotatables()
    {
        EnsureInitialized();

        CacheChildRotatables();

        if (isActiveWorld)
        {
            UpdateChildRotatables();
        }
    }

    public void SetWorldActive(bool active)
    {
        EnsureInitialized();

        if (isActiveWorld == active)
        {
            if (isActiveWorld)
            {
                UpdateChildRotatables();
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

        for (int i = stars.Count - 1; i >= 0; i--)
        {
            StarMono star = stars[i];

            if (star == null)
            {
                stars.RemoveAt(i);
                continue;
            }

            star.OnOwnerWorldActiveChanged(active);
        }

        for (int i = levelBackgrounds.Count - 1; i >= 0; i--)
        {
            LevelBackgroundMono levelBackground = levelBackgrounds[i];

            if (levelBackground == null)
            {
                levelBackgrounds.RemoveAt(i);
                continue;
            }

            levelBackground.OnOwnerWorldActiveChanged(active);
        }

        if (isActiveWorld)
        {
            UpdateChildRotatables();
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