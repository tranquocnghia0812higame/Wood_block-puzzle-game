using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class ButtonRotate : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _buttonRenderer;
        [SerializeField]
        private SpriteRenderer _rotateIconRenderer;
        [SerializeField]
        private Transform _rotateIconTransform;

        [Header("Textures")]
        [SerializeField]
        private Sprite _normalButtonSprite;
        [SerializeField]
        private Sprite _pressedButtonSprite;
        [SerializeField]
        private Sprite _disableButtonSprite;

        [SerializeField]
        private Sprite _rotateOnSprite;
        [SerializeField]
        private Sprite _rotateOffSprite;

        bool _enabledRotate;

        public void Init()
        {
            _buttonRenderer.sprite = PrefsUtils.GetInt(BoosterController.PREFS_ROTATE_COUNT, 0) > 0 ? _normalButtonSprite : _disableButtonSprite;
            _rotateIconRenderer.sprite = _rotateOffSprite;
            _rotateIconTransform.localRotation = default(Quaternion);

            _enabledRotate = false;
        }

        public void UpdateButtonState()
        {
            _buttonRenderer.sprite = PrefsUtils.GetInt(BoosterController.PREFS_ROTATE_COUNT, 0) > 0 ? _normalButtonSprite : _disableButtonSprite;
        }

        public void EnableRotate()
        {
            if (_enabledRotate)
                return;

            _enabledRotate = true;
            _rotateIconTransform.DORotate(new Vector3(0, 0, -180), 0.6f, RotateMode.LocalAxisAdd).OnComplete(() => 
            {
                _rotateIconRenderer.sprite = _rotateOnSprite;
                _rotateIconTransform.localEulerAngles = new Vector3(0, 0, -180);

                UpdateButtonState();
            });
        }

        public void DisableRotate()
        {
            if (!_enabledRotate)
                return;

            _enabledRotate = false;
            _rotateIconTransform.DORotate(new Vector3(0, 0, 180), 0.6f, RotateMode.LocalAxisAdd).OnComplete(() => 
            {
                _rotateIconRenderer.sprite = _rotateOffSprite;
                _rotateIconTransform.localEulerAngles = new Vector3(0, 0, 180);

                UpdateButtonState();
            });
        }
    }
}
