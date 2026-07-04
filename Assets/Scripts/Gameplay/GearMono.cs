using UnityEngine;

public class GearMono : MonoBehaviour
{
    [SerializeField] private GameObject anchor;

    public void Pin()
    {
        if (anchor == null)
        {
            Debug.LogWarning($"{name} 没有设置 anchor", this);
            return;
        }

        anchor.SetActive(true);
    }

    public void Unpin()
    {
        if (anchor == null)
        {
            Debug.LogWarning($"{name} 没有设置 anchor", this);
            return;
        }

        anchor.SetActive(false);
    }
}