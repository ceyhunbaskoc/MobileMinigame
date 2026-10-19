using System.Collections.Generic;
using Core.Minigame;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.FuckTech
{
    public class FuckTechMinigame : MinigameBase
    {
        private enum ItemType
        {
            Phone,
            Scrap
        }

        private enum ChoiceType
        {
            RedButton,
            GreenButton
        }

        private class BeltItem
        {
            public GameObject GameObject;
            public ItemType Type;
        }

        [Header("Hand and button references")]
        [SerializeField] private HandReachIK _handReach;
        [SerializeField] private PhysicalButton _redButton;
        [SerializeField] private PhysicalButton _greenButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject _phonePrefab;
        [SerializeField] private GameObject _scrapPrefab;
        [SerializeField] private int _poolCountPerObjectType = 5;
        [SerializeField] private Transform _poolContainer;
        [SerializeField, Range(0f, 1f)] private float _phoneChance = 0.5f;

        [Header("Belt")]
        [SerializeField] private Transform _itemsRoot;
        [SerializeField] private Transform _stopPoint;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Vector3 _moveDirection = Vector3.left;
        [SerializeField] private float _itemSpacing = 1.5f;
        [SerializeField] private int _queuedItemCount = 3;
        [SerializeField] private float _beltStepDuration = 0.45f;
        [SerializeField] private Renderer _beltRenderer;
        [SerializeField] private Vector2 _beltScrollAxis = new Vector2(1f, 0f);
        [SerializeField] private float _beltScrollPerUnit = 0.25f;

        [Header("Piston")]
        [SerializeField] private Transform _piston;
        [SerializeField] private Transform _pistonCrushPoint;
        [SerializeField] private float _pistonDownDuration = 0.12f;
        [SerializeField] private float _pistonUpDuration = 0.2f;
        [SerializeField] private ParticleSystem _crushEffect;

        [Header("UI")]
        [SerializeField] private Button _redButtonUI;
        [SerializeField] private Button _greenButtonUI;

        private const int REWARD_PER_CORRECT_CHOICE = 10;
        private const int PENALTY_PER_WRONG_CHOICE = 5;

        private int _correctChoiceCount;
        private int _wrongChoiceCount;

        private readonly Dictionary<ItemType, Stack<GameObject>> _pool = new Dictionary<ItemType, Stack<GameObject>>();
        private readonly List<BeltItem> _queue = new List<BeltItem>();
        private readonly List<BeltItem> _leavingItems = new List<BeltItem>();

        private Material _beltMaterial;
        private Vector3 _itemsRootStartPosition;
        private Vector3 _pistonRestLocalPosition;
        private Tween _beltTween;
        private Sequence _pistonSequence;
        private bool _isWaitingForInput;

        protected override void Awake()
        {
            base.Awake();

            _redButtonUI.onClick.AddListener(() => _onButtonClicked(ChoiceType.RedButton));
            _greenButtonUI.onClick.AddListener(() => _onButtonClicked(ChoiceType.GreenButton));

            _moveDirection = _moveDirection.normalized;
            _itemsRootStartPosition = _itemsRoot.position;
            _pistonRestLocalPosition = _piston.localPosition;

            if (_beltRenderer != null)
                _beltMaterial = _beltRenderer.material;

            _createPool();
        }

        protected override void OnBegin()
        {
            _correctChoiceCount = 0;
            _wrongChoiceCount = 0;

            _clearBelt();
            _advanceBelt();
        }

        protected override void OnEnd()
        {
            _isWaitingForInput = false;
        }

        private void _onButtonClicked(ChoiceType choice)
        {
            if (!IsRunning || !_isWaitingForInput)
                return;

            _isWaitingForInput = false;
            _evaluateChoice(choice);

            if (choice == ChoiceType.RedButton)
            {
                _handReach.Press(_redButton.PressPoint, AvatarIKGoal.LeftHand, () =>
                {
                    _redButton.PlayPress();
                    _crushCurrentItem();
                });
            }
            else
            {
                _handReach.Press(_greenButton.PressPoint, AvatarIKGoal.RightHand, () =>
                {
                    _greenButton.PlayPress();
                    _passCurrentItem();
                });
            }
        }

        private void _evaluateChoice(ChoiceType choice)
        {
            bool isPhone = _queue[0].Type == ItemType.Phone;
            bool isCorrect = isPhone == (choice == ChoiceType.RedButton);

            if (isCorrect)
                _correctChoiceCount++;
            else
                _wrongChoiceCount++;
        }

        #region Belt

        private void _advanceBelt()
        {
            while (_queue.Count < _queuedItemCount)
                _spawnItem(_queue.Count + 1);

            _beltTween?.Kill();
            Vector3 targetPosition = _itemsRoot.position + _moveDirection * _itemSpacing;

            _beltTween = _itemsRoot.DOMove(targetPosition, _beltStepDuration)
                .SetEase(Ease.InOutSine)
                .OnUpdate(_syncBeltTexture)
                .OnComplete(_onBeltStopped)
                .SetLink(gameObject);
        }

        private void _onBeltStopped()
        {
            _despawnExitedItems();
            _isWaitingForInput = IsRunning;
        }

        private void _syncBeltTexture()
        {
            if (_beltMaterial == null)
                return;

            float traveled = Vector3.Dot(_itemsRoot.position - _itemsRootStartPosition, _moveDirection);
            _beltMaterial.mainTextureOffset = _beltScrollAxis * Mathf.Repeat(traveled * _beltScrollPerUnit, 1f);
        }

        private void _passCurrentItem()
        {
            var item = _queue[0];
            _queue.RemoveAt(0);
            _leavingItems.Add(item);

            _advanceBelt();
        }

        private void _crushCurrentItem()
        {
            var item = _queue[0];
            _queue.RemoveAt(0);

            _pistonSequence?.Kill();
            _pistonSequence = DOTween.Sequence()
                .Append(_piston.DOMove(_pistonCrushPoint.position, _pistonDownDuration).SetEase(Ease.InQuad))
                .AppendCallback(() =>
                {
                    _playCrushEffect(item.GameObject.transform.position);
                    _despawn(item);

                    if (IsRunning)
                        _advanceBelt();
                })
                .Append(_piston.DOLocalMove(_pistonRestLocalPosition, _pistonUpDuration).SetEase(Ease.OutQuad))
                .SetLink(gameObject);
        }

        private void _playCrushEffect(Vector3 position)
        {
            if (_crushEffect == null)
                return;

            _crushEffect.transform.position = position;
            _crushEffect.Play();
        }

        private void _spawnItem(int slot)
        {
            var type = Random.value < _phoneChance ? ItemType.Phone : ItemType.Scrap;
            var itemObject = _getFromPool(type);

            itemObject.transform.SetParent(_itemsRoot, true);
            itemObject.transform.position = _stopPoint.position - _moveDirection * (_itemSpacing * slot);
            itemObject.SetActive(true);

            _queue.Add(new BeltItem { GameObject = itemObject, Type = type });
        }

        private void _despawnExitedItems()
        {
            for (int i = _leavingItems.Count - 1; i >= 0; i--)
            {
                var item = _leavingItems[i];
                float passed = Vector3.Dot(item.GameObject.transform.position - _exitPoint.position, _moveDirection);
                if (passed < 0f)
                    continue;

                _despawn(item);
                _leavingItems.RemoveAt(i);
            }
        }

        private void _clearBelt()
        {
            foreach (var item in _queue)
                _despawn(item);
            foreach (var item in _leavingItems)
                _despawn(item);

            _queue.Clear();
            _leavingItems.Clear();
        }

        #endregion

        #region Pool

        private void _createPool()
        {
            _pool[ItemType.Phone] = new Stack<GameObject>();
            _pool[ItemType.Scrap] = new Stack<GameObject>();

            for (int i = 0; i < _poolCountPerObjectType; i++)
            {
                _pool[ItemType.Phone].Push(_createPooledObject(ItemType.Phone));
                _pool[ItemType.Scrap].Push(_createPooledObject(ItemType.Scrap));
            }
        }

        private GameObject _createPooledObject(ItemType type)
        {
            var prefab = type == ItemType.Phone ? _phonePrefab : _scrapPrefab;
            var itemObject = Instantiate(prefab, _poolContainer);
            itemObject.SetActive(false);
            return itemObject;
        }

        private GameObject _getFromPool(ItemType type)
        {
            var stack = _pool[type];
            return stack.Count > 0 ? stack.Pop() : _createPooledObject(type);
        }

        private void _despawn(BeltItem item)
        {
            item.GameObject.SetActive(false);
            item.GameObject.transform.SetParent(_poolContainer, false);
            _pool[item.Type].Push(item.GameObject);
        }

        #endregion

        protected override int CalculateScore()
        {
            int score = (_correctChoiceCount * REWARD_PER_CORRECT_CHOICE) - (_wrongChoiceCount * PENALTY_PER_WRONG_CHOICE);
            return Mathf.Max(_minScore, score);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _redButtonUI.onClick.RemoveAllListeners();
            _greenButtonUI.onClick.RemoveAllListeners();

            if (_beltMaterial != null)
                Destroy(_beltMaterial);
        }
    }
}
