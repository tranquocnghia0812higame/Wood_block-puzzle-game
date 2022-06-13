using System.Collections;
using UnityEngine;

namespace BP
{
    public class RotateShopBar : MonoBehaviour
    {
        [SerializeField]
        private Transform _rotateIconTransform;
        public Transform rotateIconTransform => _rotateIconTransform;

        [SerializeField]
        private TMPro.TextMeshPro _rotateCountText;

        [SerializeField]
        private GameObject _receivedRotateIconObject;

        public void Init()
        {
            _receivedRotateIconObject.SetActive(false);
            UpdateRotateCount();
        }

        public void AnimateReceivingRotateIcon()
        {
            StartCoroutine(YieldAnimateReceivingRotateIcon());
        }

        private IEnumerator YieldAnimateReceivingRotateIcon()
        {
            SoundManager.Instance.PlayGotSpin();
            
            _receivedRotateIconObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            _receivedRotateIconObject.SetActive(false);
        }

        public void UpdateRotateCount()
        {
            _rotateCountText.text = PrefsUtils.GetInt(BoosterController.PREFS_ROTATE_COUNT, 0).ToString();
        }
    }
}
