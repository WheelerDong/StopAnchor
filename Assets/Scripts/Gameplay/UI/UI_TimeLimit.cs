using TMPro;
using UnityEngine;

public class UI_TimeLimit : MonoBehaviour
{
    [SerializeField] private TMP_Text timeLimitText;

    public void UpdateTimeText(float time)
    {
        if (timeLimitText == null)
        {
            return;
        }

        if (time < 0f)
        {
            timeLimitText.text = "--:--";
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(time));

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeLimitText.text = $"{minutes:00}:{seconds:00}";
    }
}