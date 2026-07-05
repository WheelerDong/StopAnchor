using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FailedPage : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button restart;
    [SerializeField] private Button returnMenu;

    [Header("Selector")]
    [SerializeField] private RectTransform selector;
    [SerializeField] private float restartY;
    [SerializeField] private float returnMenuY;

    [Header("Animation")]
    [SerializeField] private float selectorMoveDuration = 0.12f;
    [SerializeField] private float buttonScaleDuration = 0.08f;
    [SerializeField] private float hoverScale = 1.08f;

    private Coroutine selectorMoveCoroutine;

    private Coroutine restartScaleCoroutine;
    private Coroutine returnMenuScaleCoroutine;

    private Vector3 restartDefaultScale;
    private Vector3 returnMenuDefaultScale;

    private void Awake()
    {
        restart.onClick.AddListener(OnRestartClicked);
        returnMenu.onClick.AddListener(OnReturnMenuClicked);

        restartDefaultScale = restart.transform.localScale;
        returnMenuDefaultScale = returnMenu.transform.localScale;

        AddHoverEvent(
            restart,
            restartY,
            () => restartScaleCoroutine,
            c => restartScaleCoroutine = c,
            restartDefaultScale
        );

        AddHoverEvent(
            returnMenu,
            returnMenuY,
            () => returnMenuScaleCoroutine,
            c => returnMenuScaleCoroutine = c,
            returnMenuDefaultScale
        );
    }

    private void OnEnable()
    {
        SetSelectorYImmediately(restartY);

        if (restart != null)
        {
            restart.transform.localScale = restartDefaultScale;
        }

        if (returnMenu != null)
        {
            returnMenu.transform.localScale = returnMenuDefaultScale;
        }
    }

    private void AddHoverEvent(
        Button button,
        float selectorTargetY,
        Func<Coroutine> getScaleCoroutine,
        Action<Coroutine> setScaleCoroutine,
        Vector3 defaultScale)
    {
        if (button == null)
        {
            return;
        }

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        enterEntry.callback.AddListener(_ =>
        {
            MoveSelectorToY(selectorTargetY);
            ScaleButton(button.transform, defaultScale * hoverScale, getScaleCoroutine, setScaleCoroutine);
        });

        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };

        exitEntry.callback.AddListener(_ =>
        {
            ScaleButton(button.transform, defaultScale, getScaleCoroutine, setScaleCoroutine);
        });

        trigger.triggers.Add(exitEntry);
    }

    private void MoveSelectorToY(float targetY)
    {
        if (selector == null)
        {
            return;
        }

        if (selectorMoveCoroutine != null)
        {
            StopCoroutine(selectorMoveCoroutine);
        }

        selectorMoveCoroutine = StartCoroutine(MoveSelectorCoroutine(targetY));
    }

    private IEnumerator MoveSelectorCoroutine(float targetY)
    {
        Vector2 startPos = selector.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        float timer = 0f;

        while (timer < selectorMoveDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / selectorMoveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            selector.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        selector.anchoredPosition = targetPos;
        selectorMoveCoroutine = null;
    }

    private void SetSelectorYImmediately(float targetY)
    {
        if (selector == null)
        {
            return;
        }

        Vector2 pos = selector.anchoredPosition;
        pos.y = targetY;
        selector.anchoredPosition = pos;
    }

    private void ScaleButton(
        Transform buttonTransform,
        Vector3 targetScale,
        Func<Coroutine> getScaleCoroutine,
        Action<Coroutine> setScaleCoroutine)
    {
        if (buttonTransform == null)
        {
            return;
        }

        Coroutine currentCoroutine = getScaleCoroutine();

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        Coroutine newCoroutine = StartCoroutine(ScaleButtonCoroutine(
            buttonTransform,
            targetScale,
            setScaleCoroutine
        ));

        setScaleCoroutine(newCoroutine);
    }

    private IEnumerator ScaleButtonCoroutine(
        Transform buttonTransform,
        Vector3 targetScale,
        Action<Coroutine> setScaleCoroutine)
    {
        Vector3 startScale = buttonTransform.localScale;

        float timer = 0f;

        while (timer < buttonScaleDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / buttonScaleDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        buttonTransform.localScale = targetScale;
        setScaleCoroutine(null);
    }

    private void OnReturnMenuClicked()
    {
        GameFlowManager.Instance.GoIntoLobby();
    }

    private void OnRestartClicked()
    {
        GameplayManager.Instance.RestartLevel();
    }
}