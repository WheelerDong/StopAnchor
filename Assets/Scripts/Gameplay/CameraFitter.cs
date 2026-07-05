using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    private Transform target;
    [Tooltip("额外留边比例，1 表示刚好贴边，1.1 表示多留 10% 边距")]
    [SerializeField] private float padding = 1.05f;
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    public void Init(Transform target)
    {
        if (target == null)
        {
            Debug.Log("未找到LevelBackground");
            return;
        }
        this.target = target;
        FitToTarget();
    }
    
    
    public void FitToTarget()
    {
        if (target == null)
            return;

        Bounds bounds;
        if (!TryGetTargetBounds(target, out bounds))
            return;

        float targetHeightSize = bounds.extents.y;
        float targetWidthSize = bounds.extents.x / cam.aspect;

        cam.orthographicSize = Mathf.Max(targetHeightSize, targetWidthSize) * padding;
    }

    private bool TryGetTargetBounds(Transform target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        Collider2D[] colliders2D = target.GetComponentsInChildren<Collider2D>();

        if (colliders2D.Length > 0)
        {
            bounds = colliders2D[0].bounds;

            for (int i = 1; i < colliders2D.Length; i++)
            {
                bounds.Encapsulate(colliders2D[i].bounds);
            }

            return true;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            return true;
        }

        bounds = default;
        return false;
    }

    
}
