using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MainPage : MonoBehaviour
{
    [Header("Selector")]
    [SerializeField] private RectTransform selector;
    [SerializeField] private float playY;
    [SerializeField] private float levelY;
    [SerializeField] private float exitY;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelsButton;
    [SerializeField] private Button exitButton;

    [Header("Animation")]
    [SerializeField] private float selectorMoveDuration = 0.12f;
    [SerializeField] private float buttonScaleDuration = 0.08f;
    [SerializeField] private float hoverScale = 1.08f;

    private Coroutine selectorMoveCoroutine;

    private Coroutine playScaleCoroutine;
    private Coroutine levelsScaleCoroutine;
    private Coroutine exitScaleCoroutine;

    private Vector3 playDefaultScale;
    private Vector3 levelsDefaultScale;
    private Vector3 exitDefaultScale;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonClick);
        levelsButton.onClick.AddListener(OnLevelsButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);

        playDefaultScale = playButton.transform.localScale;
        levelsDefaultScale = levelsButton.transform.localScale;
        exitDefaultScale = exitButton.transform.localScale;

        AddHoverEvent(
            playButton,
            playY,
            () => playScaleCoroutine,
            c => playScaleCoroutine = c,
            playDefaultScale
        );

        AddHoverEvent(
            levelsButton,
            levelY,
            () => levelsScaleCoroutine,
            c => levelsScaleCoroutine = c,
            levelsDefaultScale
        );

        AddHoverEvent(
            exitButton,
            exitY,
            () => exitScaleCoroutine,
            c => exitScaleCoroutine = c,
            exitDefaultScale
        );
    }

    private void Start()
    {
        // 默认让 selector 停在 Play 的 y 位置
        SetSelectorYImmediately(playY);
    }

    private void AddHoverEvent(
        Button button,
        float selectorTargetY,
        Func<Coroutine> getScaleCoroutine,
        Action<Coroutine> setScaleCoroutine,
        Vector3 defaultScale)
    {
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

    private void OnPlayButtonClick()
    {
        GameFlowManager.Instance.PlayCurrentLevel();
    }

    private void OnLevelsButtonClick()
    {
        GameUIManager.Instance.ShowLevelsPage();
    }

    private void OnExitButtonClick()
    {
        GameFlowManager.Instance.QuitGame();
    }
}