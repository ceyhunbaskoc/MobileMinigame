using System.Collections;
using System.Collections.Generic;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Minigame
{
    public class MinigameBeginningUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _beginningUI;
        [SerializeField] private MinigameBeginningSlotUI _minigameBeginningSlotPrefab;
        [SerializeField] private RectTransform _slotsContainer;
        [SerializeField] private Button _finishButton;

        [Header("Pointer")]
        [SerializeField] private RectTransform _pointerVisual;
        [SerializeField] private Vector2 _pointerOffset = new Vector2(0f, 120f);
        [SerializeField] private float _pointerMoveDuration = 0.5f;

        [Header("Info Panel")]
        [SerializeField] private RectTransform _minigameInfoPanel;
        [SerializeField] private TextMeshProUGUI _tutorialText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _minigameImage;

        private readonly List<MinigameBeginningSlotUI> _minigameSlots = new List<MinigameBeginningSlotUI>();
        private List<MinigameDefinition> _sessionOrder;
        private int _currentIndex;
        private bool _isFinishClicked;

        public void Initialize()
        {
            _finishButton.onClick.AddListener(_handleFinishClicked);
            Hide();
        }

        public void SetupSession(List<MinigameDefinition> sessionOrder)
        {
            _sessionOrder = sessionOrder;

            foreach (var slot in _minigameSlots)
                Destroy(slot.gameObject);
            _minigameSlots.Clear();

            foreach (var minigame in _sessionOrder)
            {
                var slot = Instantiate(_minigameBeginningSlotPrefab, _slotsContainer);
                slot.SetImage(minigame.Icon);
                _minigameSlots.Add(slot);
            }
        }

        public void Show(int index)
        {
            _currentIndex = index;

            _beginningUI.DOKill();
            _beginningUI.alpha = 1f;
            _beginningUI.blocksRaycasts = true;
            _beginningUI.gameObject.SetActive(true);

            _minigameInfoPanel.gameObject.SetActive(false);
            _finishButton.interactable = false;

            for (int i = 0; i < _minigameSlots.Count; i++)
                _minigameSlots[i].SetPlayed(i < _currentIndex);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_slotsContainer);
            _placePointerAtStart();
        }

        public IEnumerator Play()
        {
            yield return _playPointerAnimation().WaitForCompletion();

            _setMinigameInfo();
            yield return _openMinigameInfo().WaitForCompletion();

            _isFinishClicked = false;
            _finishButton.interactable = true;
            yield return new WaitUntil(() => _isFinishClicked);

            yield return _hideMinigameInfo().WaitForCompletion();
            yield return _beginningUI.DOFade(0f, 0.25f).SetUpdate(true).WaitForCompletion();
            Hide();
        }

        public void Hide()
        {
            _beginningUI.DOKill();
            _beginningUI.blocksRaycasts = false;
            _beginningUI.gameObject.SetActive(false);
        }

        private void _placePointerAtStart()
        {
            _pointerVisual.DOKill();

            if (_currentIndex == 0)
            {
                _pointerVisual.position = _getPointerTarget(0);
                _pointerVisual.localScale = Vector3.zero;
            }
            else
            {
                _pointerVisual.position = _getPointerTarget(_currentIndex - 1);
                _pointerVisual.localScale = Vector3.one;
            }
        }

        private Tween _playPointerAnimation()
        {
            var slot = _minigameSlots[_currentIndex].RectTransform;
            slot.DOKill(true);

            var sequence = DOTween.Sequence().SetUpdate(true);

            if (_currentIndex == 0)
                sequence.Append(_pointerVisual.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            else
                sequence.Append(_pointerVisual.DOMove(_getPointerTarget(_currentIndex), _pointerMoveDuration).SetEase(Ease.InOutQuad));

            sequence.Append(slot.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1, 0f));
            return sequence;
        }

        private Vector3 _getPointerTarget(int index)
        {
            return _minigameSlots[index].RectTransform.TransformPoint(_pointerOffset);
        }

        private void _setMinigameInfo()
        {
            var minigameDefinition = _sessionOrder[_currentIndex];
            _titleText.text = minigameDefinition.TitleText;
            _tutorialText.text = minigameDefinition.TutorialText;
            _minigameImage.sprite = minigameDefinition.Image;
        }

        private Tween _openMinigameInfo()
        {
            _minigameInfoPanel.DOKill();
            _minigameInfoPanel.localScale = Vector3.zero;
            _minigameInfoPanel.gameObject.SetActive(true);

            return _minigameInfoPanel.DOScale(Vector3.one, 0.4f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        private Tween _hideMinigameInfo()
        {
            _minigameInfoPanel.DOKill();

            return _minigameInfoPanel.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => _minigameInfoPanel.gameObject.SetActive(false));
        }

        private void _handleFinishClicked()
        {
            _finishButton.interactable = false;
            _isFinishClicked = true;
        }

        private void OnDestroy()
        {
            _finishButton.onClick.RemoveListener(_handleFinishClicked);
        }
    }
}
