using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using UnityEngine;

namespace BP
{
    public class ScoreController : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [Header("Best Score")]
        [SerializeField]
        private Animator m_NewBestAnimator;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject m_ScorePrefab;
        [SerializeField]
        private GameObject m_HighlightTextPrefab;

        [Header("Textures")]
        [SerializeField]
        private Sprite m_GoodSprite;
        [SerializeField]
        private Sprite m_GreatSprite;
        [SerializeField]
        private Sprite m_PerfectSprite;
        #endregion

        #region Properties
        private int m_CurrentScore;
        private int m_HighScore;

        public int currentScore
        {
            get
            {
                return m_CurrentScore;
            }
        }

        public int highScore
        {
            get
            {
                return m_HighScore;
            }
        }

        private bool m_HasShownNewBestScore = false;
        #endregion

        #region Unity Events
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                PrefsUtils.SetInt(Consts.PREFS_LAST_SCORE, m_CurrentScore);
                PrefsUtils.SetInt(Consts.PREFS_HIGH_SCORE, m_HighScore);
            }
        }
        #endregion

        #region Methods
        public void LoadSavedData()
        {
            m_CurrentScore = PrefsUtils.GetInt(Consts.PREFS_LAST_SCORE, 0);
            m_HighScore = PrefsUtils.GetInt(Consts.PREFS_HIGH_SCORE, 0);

            UpdateGUI(forceUpdate: true);

            if (m_CurrentScore == m_HighScore)
                m_HasShownNewBestScore = true;
        }

        public void Reset()
        {
            m_CurrentScore = 0;
            PrefsUtils.SetInt(Consts.PREFS_LAST_SCORE, m_CurrentScore);

            m_HighScore = PrefsUtils.GetInt(Consts.PREFS_HIGH_SCORE, 0);
            UpdateGUI(forceUpdate: true);

            m_HasShownNewBestScore = false;
        }

        public void HandleLose()
        {
            m_CurrentScore = 0;
            PrefsUtils.SetInt(Consts.PREFS_LAST_SCORE, m_CurrentScore);
            PrefsUtils.SetInt(Consts.PREFS_HIGH_SCORE, m_HighScore);
        }

        public void AddScore(int bonusScore)
        {
            m_CurrentScore += bonusScore;
            if (m_CurrentScore > m_HighScore)
            {
                if (!m_HasShownNewBestScore && PrefsUtils.GetInt(Consts.PREFS_HIGH_SCORE, 0) > 0)
                {
                    m_HasShownNewBestScore = true;

                    m_NewBestAnimator.Play("new_best_show");
                    SoundManager.Instance.PlayHighscore();
                }

                m_HighScore = m_CurrentScore;
            }

            UpdateGUI(forceUpdate: false);
        }

        public void AddScoreText(int brokenRowColCount, Vector3 position, GridElementType type)
        {
            int bonusScore = GetScoreByBrokenRowColCount(brokenRowColCount);
            m_CurrentScore += bonusScore;

            if (m_CurrentScore > m_HighScore)
            {
                m_HighScore = m_CurrentScore;
            }

            FloatingScore floatingScore = PoolBoss
                .SpawnOutsidePool(m_ScorePrefab.transform, position, default(Quaternion))
                .GetComponent<FloatingScore>();

            floatingScore.Show(bonusScore, type);

            UpdateGUI(forceUpdate: false);
        }

        public void AddHighlightText(int brokenRowColCount, Vector3 position)
        {
            if (brokenRowColCount < 2)
                return;

            Sprite textSprite = m_GoodSprite;
            if (brokenRowColCount == 2)
            {
                textSprite = m_GoodSprite;
                SoundManager.Instance.PlayVoiceGood();
            }
            else if (brokenRowColCount == 3)
            {
                textSprite = m_GreatSprite;
                SoundManager.Instance.PlayVoiceGreat();
            }
            else
            {
                textSprite = m_PerfectSprite;
                SoundManager.Instance.PlayVoicePerfect();
            }

            HighlightText highlightText = PoolBoss
                .SpawnInPool(m_HighlightTextPrefab.transform, Vector3.zero, default(Quaternion))
                .GetComponent<HighlightText>();

            highlightText.VTransform.position = new Vector3(Mathf.Clamp(position.x, -3.5f, 3.5f), position.y - 1.6f, position.z);
            highlightText.Show(textSprite);
        }

        private int GetScoreByBrokenRowColCount(int brokenRowColCount)
        {
            if (brokenRowColCount == 0)
                return 0;
            if (brokenRowColCount == 1)
                return 100;
            else if (brokenRowColCount == 2)
                return 300;
            else if (brokenRowColCount == 3)
                return 600;
            else if (brokenRowColCount == 4)
                return 1000;
            else if (brokenRowColCount == 5)
                return 1500;
            else
                return 400 * brokenRowColCount;
        }

        private void UpdateGUI(bool forceUpdate)
        {
            GameManager.Instance.header.UpdateScore(m_CurrentScore, m_HighScore, forceUpdate);
        }
        #endregion
    }
}