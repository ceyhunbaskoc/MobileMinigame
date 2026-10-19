using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Core.Minigame
{
    public abstract class MinigameBase : MonoBehaviour
{
    [SerializeField] private float _minigameDuration = 30f;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _beginningWaitText;
    [SerializeField] protected int _minScore = 0;
    [SerializeField] protected int _beginningWaitTime = 3;

    public static event Action<int> OnFinished;
    public static MinigameBase Current { get; private set; }

    public bool IsRunning { get; private set; }
    protected float TimeLeft { get; private set; }
    protected float Duration => _minigameDuration;
    protected bool HasTimer => _minigameDuration > 0f;

    private bool _isFinished;
    private bool _isCountingDown;
    private int _shownSeconds = -1;

    protected virtual void Awake()
    {
        Current = this;
        TimeLeft = _minigameDuration;
        RefreshTimerText();

        if (_beginningWaitText != null)
            _beginningWaitText.gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
#if UNITY_EDITOR
        if (FindAnyObjectByType<GameBootstrapper>() == null)
        {
            Debug.Log($"[{GetType().Name}] Running standalone, starting immediately.");
            Begin();
        }
#endif
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        OnTick();
        if (!IsRunning || !HasTimer)
            return;

        TimeLeft = Mathf.Max(0f, TimeLeft - Time.deltaTime);
        RefreshTimerText();

        if (TimeLeft <= 0f)
            Finish();
    }

    protected virtual void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }

    public void Begin()
    {
        if (IsRunning || _isFinished || _isCountingDown)
            return;

        TimeLeft = _minigameDuration;
        RefreshTimerText();
        StartCoroutine(BeginRoutine());
    }

    private IEnumerator BeginRoutine()
    {
        _isCountingDown = true;

        for (int i = _beginningWaitTime; i > 0; i--)
        {
            ShowBeginningWaitText(i.ToString(), false);
            yield return new WaitForSeconds(1f);
        }

        _isCountingDown = false;
        IsRunning = true;
        OnBegin();

        if (_beginningWaitTime > 0)
            ShowBeginningWaitText("GO!", true);
    }

    private void ShowBeginningWaitText(string text, bool hideAfterShown)
    {
        if (_beginningWaitText == null)
            return;

        var textTransform = _beginningWaitText.transform;
        _beginningWaitText.text = text;
        _beginningWaitText.gameObject.SetActive(true);

        textTransform.DOKill();
        textTransform.localScale = Vector3.one * 1.6f;

        var sequence = DOTween.Sequence()
            .Append(textTransform.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .SetTarget(textTransform)
            .SetLink(gameObject);

        if (hideAfterShown)
        {
            sequence.AppendInterval(0.3f)
                .AppendCallback(() => _beginningWaitText.gameObject.SetActive(false));
        }
    }

    protected void Finish() => Finish(CalculateScore());

    protected void Finish(int score)
    {
        if (_isFinished)
            return;

        _isFinished = true;
        IsRunning = false;
        OnEnd();

        Debug.Log($"[{GetType().Name}] Finished with score {score}");
        OnFinished?.Invoke(score);
    }

    private void RefreshTimerText()
    {
        if (_timerText == null || !HasTimer)
            return;

        int seconds = Mathf.CeilToInt(TimeLeft);
        if (seconds == _shownSeconds)
            return;

        _shownSeconds = seconds;
        _timerText.text = seconds.ToString();
    }

    protected abstract int CalculateScore();
    protected virtual void OnBegin() { }
    protected virtual void OnTick() { }
    protected virtual void OnEnd() { }
}
}
