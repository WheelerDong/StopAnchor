using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class LevelMono : MonoBehaviour
{
    [SerializeField] public int anchorCount;

    [Header("Player Spawn")]
    [SerializeField] private PlayerMono playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform playerParent;

    [Header("Options")]
    [SerializeField] private bool autoInitOnStart = true;

    [Tooltip("玩家暂时不在任何 World Collider 内时，是否保持上一个激活 World。")]
    [SerializeField] private bool keepCurrentWorldWhenPlayerOutsideAllWorlds = true;

    private readonly List<WorldMono> worlds = new List<WorldMono>();

    private PlayerMono playerInstance;

    public WorldMono CurrentActiveWorld { get; private set; }

    public bool IsInitialized { get; private set; }

    public PlayerMono PlayerInstance => playerInstance;

    public Transform PlayerTransform =>
        playerInstance != null ? playerInstance.transform : null;

    public void Init()
    {
        if (IsInitialized)
        {
            return;
        }

        //Debug.Log("levelMono.Init");
        
        SpawnPlayerIfNeeded();
        CacheChildWorlds();

        GameplayManager.Instance.Init(this);
        IsInitialized = true;
        RefreshCurrentActiveWorld(true);
    }

    private void Start()
    {
        // if (autoInitOnStart && !IsInitialized)
        // {
        //     Init();
        // }
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        RefreshCurrentActiveWorld(false);
    }

    private void SpawnPlayerIfNeeded()
    {
        if (playerInstance != null)
        {
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError($"{nameof(LevelMono)} 缺少 Player Prefab。", this);
            return;
        }

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = Quaternion.identity;

        if (playerSpawnPoint != null)
        {
            spawnPosition = playerSpawnPoint.position;
            spawnRotation = playerSpawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning($"{nameof(LevelMono)} 没有配置 Player Spawn Point，将使用 LevelMono 自身位置生成玩家。", this);
        }

        Transform parent = playerParent != null ? playerParent : transform;

        Debug.Log("玩家生成");
        playerInstance = Instantiate(
            playerPrefab,
            spawnPosition,
            spawnRotation,
            parent
        );
        
    }

    public void RefreshCurrentActiveWorld(bool forceRefresh = false)
    {
        WorldMono targetWorld = FindWorldByPlayerPosition();

        if (targetWorld == null)
        {
            if (keepCurrentWorldWhenPlayerOutsideAllWorlds && CurrentActiveWorld != null)
            {
                targetWorld = CurrentActiveWorld;
            }
            else
            {
                targetWorld = FindDefaultWorld();
            }
        }

        SetCurrentActiveWorld(targetWorld, forceRefresh);
    }

    public void RefreshWorldCacheAndState()
    {
        CacheChildWorlds();
        RefreshCurrentActiveWorld(true);
    }

    private void SetCurrentActiveWorld(WorldMono targetWorld, bool forceRefresh)
    {
        if (!forceRefresh && CurrentActiveWorld == targetWorld)
        {
            return;
        }

        CurrentActiveWorld = targetWorld;

        for (int i = worlds.Count - 1; i >= 0; i--)
        {
            WorldMono world = worlds[i];

            if (world == null)
            {
                worlds.RemoveAt(i);
                continue;
            }

            world.SetWorldActive(world == CurrentActiveWorld);
        }
    }

    private WorldMono FindWorldByPlayerPosition()
    {
        Transform playerTransform = PlayerTransform;

        if (playerTransform == null)
        {
            return null;
        }

        WorldMono bestWorld = null;

        int bestPriority = int.MinValue;
        float bestDistanceSqr = float.MaxValue;

        Vector2 playerPosition = playerTransform.position;

        for (int i = worlds.Count - 1; i >= 0; i--)
        {
            WorldMono world = worlds[i];

            if (world == null)
            {
                worlds.RemoveAt(i);
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

    private WorldMono FindDefaultWorld()
    {
        WorldMono bestWorld = null;

        int bestPriority = int.MinValue;
        float bestDistanceSqr = float.MaxValue;

        Transform playerTransform = PlayerTransform;
        Vector2 playerPosition = playerTransform != null
            ? (Vector2)playerTransform.position
            : Vector2.zero;

        for (int i = worlds.Count - 1; i >= 0; i--)
        {
            WorldMono world = worlds[i];

            if (world == null)
            {
                worlds.RemoveAt(i);
                continue;
            }

            if (!world.ActiveOnStart)
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

    private void CacheChildWorlds()
    {
        worlds.Clear();

        WorldMono[] childWorlds = GetComponentsInChildren<WorldMono>(true);

        for (int i = 0; i < childWorlds.Length; i++)
        {
            WorldMono world = childWorlds[i];

            if (world == null)
            {
                continue;
            }

            worlds.Add(world);
            world.RefreshChildObstacles();
            world.SetWorldActive(false);
        }
    }
    
    private void OnDestroy()
    {
        if (playerInstance == null)
        {
            return;
        }

        if (!playerInstance.transform.IsChildOf(transform))
        {
            Destroy(playerInstance.gameObject);
        }

        playerInstance = null;
    }
}
