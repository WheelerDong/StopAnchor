using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitPoint : MonoBehaviour
{
    private bool hasTriggered = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
        {
            return;
        }

        PlayerMono player = other.GetComponentInParent<PlayerMono>();

        if (player == null)
        {
            return;
        }

        hasTriggered = true;

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.WinThisLevel();
        }
    }
}