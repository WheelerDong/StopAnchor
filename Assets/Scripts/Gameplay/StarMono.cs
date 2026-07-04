using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StarMono : MonoBehaviour
{
    [Header("Rotation Target")]
    [SerializeField] private Transform rotateTarget;

    [Header("Rotation Center")]
    [SerializeField] private Transform rotationCenter;

    private WorldMono ownerWorld;

    private Vector3 defaultOffsetFromCenter;
    private Quaternion defaultRotation;

    private float currentAngle = 0f;
    private bool initialized = false;

    private bool hasCollected = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (rotateTarget == null)
        {
            rotateTarget = transform;
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
        // 星星没有锁定状态，World 激活 / 失活时不需要额外处理。
    }

    public void FollowWorldAngle(float worldAngle)
    {
        if (!CanReceiveWorldControl())
        {
            return;
        }

        SnapToAngle(worldAngle);
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
            rotateTarget = transform;
        }

        if (rotationCenter == null)
        {
            if (ownerWorld == null)
            {
                ownerWorld = GetComponentInParent<WorldMono>();
            }

            if (ownerWorld != null)
            {
                rotationCenter = ownerWorld.WorldCenter;
            }
        }

        if (rotationCenter == null)
        {
            Debug.LogWarning($"{name} 没有设置 rotationCenter，也没有找到父级 WorldMono", this);
            return false;
        }

        defaultOffsetFromCenter =
            rotateTarget.position - rotationCenter.position;

        defaultRotation = rotateTarget.rotation;

        currentAngle = 0f;
        initialized = true;

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollected)
        {
            return;
        }

        PlayerMono player = other.GetComponentInParent<PlayerMono>();

        if (player == null)
        {
            return;
        }

        hasCollected = true;

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.AddStar();
        }

        Destroy(gameObject);
    }
}