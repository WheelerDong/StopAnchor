using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelFailCollider2D : MonoBehaviour
{
    private LevelMono ownerLevel;

    public void Init(LevelMono levelMono)
    {
        ownerLevel = levelMono;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ownerLevel == null)
        {
            return;
        }

        PlayerMono player = other.GetComponentInParent<PlayerMono>();

        if (player == null)
        {
            return;
        }

        ownerLevel.NotifyPlayerEnterFailCollider(player);
    }
}