using UnityEngine;
using UnityEngine.UI;

namespace UI.Minigame
{
    public class MinigameBeginningSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Color _playedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        public RectTransform RectTransform => _rectTransform;

        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void SetPlayed(bool isPlayed)
        {
            _image.color = isPlayed ? _playedColor : Color.white;
        }
    }
}
