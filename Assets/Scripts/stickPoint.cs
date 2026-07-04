using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class stickPoint : MonoBehaviour
{
    private Collider2D col;

    public Transform StickTransform => transform;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null)
        {
            c.isTrigger = true;
        }
    }
#endif
}