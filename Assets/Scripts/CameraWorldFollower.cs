using UnityEngine;

public class CameraWorldFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float arriveDistance = 0.01f;

    [Header("Options")]
    [SerializeField] private bool snapOnStart = true;

    private WorldMono currentWorld;

    private void Awake()
    {
        if (player == null)
        {
            PlayerMono playerMono = FindObjectOfType<PlayerMono>();

            if (playerMono != null)
            {
                player = playerMono.transform;
            }
        }
    }

    private void Start()
    {
        RefreshCurrentWorld();

        if (snapOnStart)
        {
            SnapCameraToCurrentWorld();
        }
    }

    private void LateUpdate()
    {
        RefreshCurrentWorld();
        MoveCameraToCurrentWorld();
    }

    private void RefreshCurrentWorld()
    {
        if (player == null)
        {
            return;
        }

        WorldMono world = WorldMono.FindWorldByPosition(player.position);

        if (world != null)
        {
            currentWorld = world;
        }
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

        float distance = Vector2.Distance(currentPosition, targetPosition);

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