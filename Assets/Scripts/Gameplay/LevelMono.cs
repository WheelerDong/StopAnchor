using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class LevelMono : MonoBehaviour
{
    [SerializeField] private int anchorCount;
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Options")]
    [SerializeField] private bool autoInitOnStart = true;

    [Tooltip("玩家暂时不在任何 World Collider 内时，是否保持上一个激活 World。")]
    [SerializeField] private bool keepCurrentWorldWhenPlayerOutsideAllWorlds = true;

    private readonly List<WorldMono> worlds = new List<WorldMono>();

    public WorldMono CurrentActiveWorld { get; private set; }

    public bool IsInitialized { get; private set; }

    public void Init()
    {
        ResolvePlayer();
        CacheChildWorlds();
        GameplayManager.Instance.Init(anchorCount);

        IsInitialized = true;

        RefreshCurrentActiveWorld(true);
    }

    private void Start()
    {
        if (autoInitOnStart && !IsInitialized)
        {
            Init();
        }
    }

    private void Update()
    {
        if (!IsInitialized)
        {
            return;
        }

        RefreshCurrentActiveWorld(false);
    }

    public void RefreshCurrentActiveWorld(bool forceRefresh = false)
    {
        ResolvePlayer();

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
        if (player == null)
        {
            return null;
        }

        WorldMono bestWorld = null;

        int bestPriority = int.MinValue;
        float bestDistanceSqr = float.MaxValue;

        Vector2 playerPosition = player.position;

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

        Vector2 playerPosition = player != null ? (Vector2)player.position : Vector2.zero;

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

    private void ResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        PlayerMono playerMono = FindObjectOfType<PlayerMono>();

        if (playerMono != null)
        {
            player = playerMono.transform;
        }
    }
}