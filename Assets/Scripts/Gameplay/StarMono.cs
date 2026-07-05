using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StarMono : MonoBehaviour
{
    private bool hasCollected = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
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