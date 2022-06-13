using Tidi.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public class GUIPausePopup : BasePopup
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Image m_SoundImage;

        [Header("Textures")]
        [SerializeField]
        private Sprite m_SoundOnSprite;
        [SerializeField]
        private Sprite m_SoundOffSprite;
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public override void Show()
        {
            base.Show();

            m_SoundImage.sprite = SoundManager.Instance.IsSoundOn() ? m_SoundOnSprite : m_SoundOffSprite;
        }

        public void SwitchSound()
        {
            SoundManager.Instance.PlayClick();
            
            SoundManager.Instance.SwitchSound();
            m_SoundImage.sprite = SoundManager.Instance.IsSoundOn() ? m_SoundOnSprite : m_SoundOffSprite;
        }

        public void Replay()
        {
            SoundManager.Instance.PlayClick();

            Hide();

            GameManager.Instance.Replay();
        }

        public void Resume()
        {
            SoundManager.Instance.PlayClick();
            Hide();
        }

        public void OpenLeaderboard()
        {
            SoundManager.Instance.PlayClick();

            int highscore = PrefsUtils.GetInt(Consts.PREFS_HIGH_SCORE, 0);
            LeaderboardManager.Instance.ShowLeaderboard(highscore);
        }

        public void OpenRating()
        {
            SoundManager.Instance.PlayClick();

            GameManager.Instance.ShowRating();
        }

        public void OpenPolicy()
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.screenType = ScreenType.POLICY;
            Application.OpenURL("https://sites.google.com/view/policyblockpuzzle");
        }
        #endregion
    }
}
