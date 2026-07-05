using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_WinPage : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button returnToMainMenuButton;

    [Header("Selector")]
    [SerializeField] private RectTransform selector;
    [SerializeField] private float nextLevelY;
    [SerializeField] private float mainMenuY;

    [Header("Animation")]
    [SerializeField] private float selectorMoveDuration = 0.12f;
    [SerializeField] private float buttonScaleDuration = 0.08f;
    [SerializeField] private float hoverScale = 1.08f;

    private Coroutine selectorMoveCoroutine;

    private Coroutine nextLevelScaleCoroutine;
    private Coroutine mainMenuScaleCoroutine;

    private Vector3 nextLevelDefaultScale;
    private Vector3 mainMenuDefaultScale;

    private void Awake()
    {
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClick);
        returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuButtonClick);

        nextLevelDefaultScale = nextLevelButton.transform.localScale;
        mainMenuDefaultScale = returnToMainMenuButton.transform.localScale;

        AddHoverEvent(
            nextLevelButton,
            nextLevelY,
            () => nextLevelScaleCoroutine,
            c => nextLevelScaleCoroutine = c,
            nextLevelDefaultScale
        );

        AddHoverEvent(
            returnToMainMenuButton,
            mainMenuY,
            () => mainMenuScaleCoroutine,
            c => mainMenuScaleCoroutine = c,
            mainMenuDefaultScale
        );
    }

    private void OnEnable()
    {
        SetSelectorYImmediately(nextLevelY);

        if (nextLevelButton != null)
        {
            nextLevelButton.transform.localScale = nextLevelDefaultScale;
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.transform.localScale = mainMenuDefaultScale;
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

    private void OnReturnToMainMenuButtonClick()
    {
        GameFlowManager.Instance.GoIntoLobby();
    }

    private void OnNextLevelButtonClick()
    {
        GameplayManager.Instance.NextLevel();
    }
}