using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraWorldFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelMono level;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float arriveDistance = 0.01f;

    [Header("Options")]
    [SerializeField] private bool snapOnStart = true;

    private WorldMono currentWorld;

    private void Awake()
    {
        ResolveLevel();
    }

    private void Start()
    {
        EnsureLevelReady();

        RefreshCurrentWorld();

        if (snapOnStart)
        {
            SnapCameraToCurrentWorld();
        }
    }

    private void Update()
    {
        EnsureLevelReady();
        RefreshCurrentWorld();
    }

    private void LateUpdate()
    {
        MoveCameraToCurrentWorld();
    }

    private void EnsureLevelReady()
    {
        ResolveLevel();

        if (level != null && !level.IsInitialized)
        {
            Debug.Log("CameraWorldFollower is initializing level");
            level.Init();
        }
    }

    private void ResolveLevel()
    {
        if (level != null)
        {
            return;
        }

        level = FindObjectOfType<LevelMono>();
    }

    private void RefreshCurrentWorld()
    {
        currentWorld = level != null ? level.CurrentActiveWorld : null;
    }

    private void MoveCameraToCurrentWorld()
    {
        if (currentWorld == null)
        {
            return;
        }

        Transform center = currentWorld.WorldCenter;

        if (center == null)
        {
            return;
        }

        Vector3 currentPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            center.position.x,
            center.position.y,
            currentPosition.z
        );

        if (moveSpeed <= 0f)
        {
            transform.position = targetPosition;
            return;
        }

        float distance = Vector2.Distance(
            new Vector2(currentPosition.x, currentPosition.y),
            new Vector2(targetPosition.x, targetPosition.y)
        );

        if (distance <= arriveDistance)
        {
            transform.position = targetPosition;
            return;
        }

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void SnapCameraToCurrentWorld()
    {
        if (currentWorld == null)
        {
            return;
        }

        Transform center = currentWorld.WorldCenter;

        if (center == null)
        {
            return;
        }

        Vector3 currentPosition = transform.position;

        transform.position = new Vector3(
            center.position.x,
            center.position.y,
            currentPosition.z
        );
    }
}