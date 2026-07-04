using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldMono : MonoBehaviour
{
    private static readonly List<WorldMono> activeWorlds = new List<WorldMono>();

    [Header("World Settings")]
    [SerializeField] private Transform worldCenter;

    [Header("Select Priority")]
    [SerializeField] private int priority = 0;

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

    private void Awake()
    {
        worldCollider = GetComponent<Collider2D>();
        worldCollider.isTrigger = true;

        if (worldCenter == null)
        {
            worldCenter = transform;
        }
    }

    private void OnEnable()
    {
        if (!activeWorlds.Contains(this))
        {
            activeWorlds.Add(this);
        }
    }

    private void OnDisable()
    {
        activeWorlds.Remove(this);
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

        for (int i = 0; i < activeWorlds.Count; i++)
        {
            WorldMono world = activeWorlds[i];

            if (world == null)
            {
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
}