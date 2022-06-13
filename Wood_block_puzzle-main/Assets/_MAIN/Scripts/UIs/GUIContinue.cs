using System.Collections;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Tidi.Ads;

namespace BP
{
    public class GUIContinue : MonoBehaviour
    {
        #region Constants
        private const int WAITING_DURATION = 8;
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Image m_ShadowImage;

        [SerializeField]
        private TMPro.TextMeshProUGUI m_ScoreText;
        [SerializeField]
        private TMPro.TextMeshProUGUI m_TimeCountText;
        #endregion

        #region Properties
        private Canvas m_Canvas;

        private System.Action<bool> m_OnGotRewarded;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure()
        {
            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            m_Canvas.enabled = false;
        }

        public void Show(int currentScore, System.Action<bool> onGotRewarded)
        {
            m_OnGotRewarded = onGotRewarded;

            m_ScoreText.text = $"Score {currentScore}";

            m_Canvas.enabled = true;
            m_ShadowImage.color = new Color(0, 0, 0, 0.65f);
            m_ShadowImage.enabled = true;

            StartCoroutine(CountDown());
        }

        private IEnumerator CountDown()
        {
            SoundManager.Instance.PlayTickTimer();

            m_TimeCountText.text = WAITING_DURATION.ToString();
            int currentCount = WAITING_DURATION;
            var delay = new WaitForSeconds(1f);

            while (currentCount > 0)
            {
                yield return delay;
                
                currentCount--;
                m_TimeCountText.text = currentCount.ToString();
            }

            m_TimeCountText.text = "0";
            
            m_OnGotRewarded?.Invoke(false);
            Hide();
        }

        public void WatchVideo()
        {
            AdManager.Instance.ShowRewardedVideo();
            AdManager.Instance.onWatchVideoReward += HandleOnWatchVideoReward;
        }

        private void HandleOnWatchVideoReward(bool succeed)
        {
            AdManager.Instance.onWatchVideoReward -= HandleOnWatchVideoReward;
            Hide();
            m_OnGotRewarded?.Invoke(succeed);
        }

        public void NoThanks()
        {
            Hide();
            m_OnGotRewarded?.Invoke(false);
        }

        public void Hide()
        {
            SoundManager.Instance.StopTickTimer();
            
            StopAllCoroutines();
            m_Canvas.enabled = false;
        }
        #endregion
    }
}
