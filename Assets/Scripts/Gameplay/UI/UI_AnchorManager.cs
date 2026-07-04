using System.Collections.Generic;
using UnityEngine;

public class UI_AnchorManager : MonoBehaviour
{
    [SerializeField] private UI_Anchor anchorPrefab;

    private readonly List<UI_Anchor> anchors = new();
    private bool hasInit = false;

    public void Init(int currentAnchorCount)
    {
        ClearAnchors();

        if (anchorPrefab == null)
        {
            Debug.LogError($"{nameof(UI_AnchorManager)}: anchorPrefab is null.", this);
            return;
        }

        currentAnchorCount = Mathf.Max(0, currentAnchorCount);

        for (int i = 0; i < currentAnchorCount; i++)
        {
            UI_Anchor anchor = Instantiate(anchorPrefab, transform);
            anchor.gameObject.SetActive(true);

            anchors.Add(anchor);
        }
        hasInit = true;
    }
    

    private void ClearAnchors()
    {
        for (int i = anchors.Count - 1; i >= 0; i--)
        {
            if (anchors[i] != null)
            {
                Destroy(anchors[i].gameObject);
            }
        }

        anchors.Clear();
    }

    public void UpdateUI(int currentAnchorCount)
    {
        if (!hasInit)
        {
            Init(currentAnchorCount);
        }
    }
}