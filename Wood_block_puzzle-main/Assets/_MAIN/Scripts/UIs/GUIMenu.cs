using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public class GUIMenu : BaseBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private TMPro.TextMeshProUGUI m_HighscoreText;
        [SerializeField]
        private TMPro.TextMeshProUGUI m_ScoreText;
        #endregion

        #region Properties
        private Canvas m_Canvas;
        private float _currentScore;
        private float _targetScore;
        private float _targetHighscore;

        float _movingDuration;
        float _currentMovingTime;
        float _startMovingScore;
        #endregion

        #region Unity Events
        private void Update()
        {
            if (_currentScore == _targetScore)
                return;

            if (_currentMovingTime < _movingDuration)
            {
                _currentMovingTime += Time.deltaTime;

                _currentScore = Mathf.Lerp(_startMovingScore, _targetScore, _currentMovingTime / _movingDuration);
            }
            else
            {
                _currentScore = _targetScore;
            }

            m_ScoreText.text = ((int)_currentScore).ToString();
            if (_targetHighscore == _targetScore)
                m_HighscoreText.text = ((int)_currentScore).ToString();
        }
        #endregion

        #region Methods
        public void Configure()
        {
            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            // Trick for using WorldSpace and you don't want to take time for configure size, scale. 
            // Just use RenderMode.Camera before change to RenderMode.WorldSpace
            // We need WorldSpace cause of shaking camera doesn't affect to RenderMode.Camera
            // But we don't need it anymore, because shaking camera is not used in Brick 1010
            m_Canvas.renderMode = RenderMode.WorldSpace;
        }

        public void PressedPause()
        {
            SoundManager.Instance.PlayClick();

            if (!PrefsUtils.GetBool(Consts.PREFS_SHOWN_TUTORIAL, false))
                return;
            
            // Show pause popup
            GameManager.Instance.ShowPausePopup();
        }

        public void UpdateScore(int score, int highscore, bool forceUpdate)
        {
            _targetScore = score;
            _targetHighscore = highscore;

            if (forceUpdate)
            {
                _currentScore = _targetScore;
                _targetHighscore = highscore;

                m_ScoreText.text = score.ToString();
                m_HighscoreText.text = highscore.ToString();
            }
            else
            {
                float change = _targetScore - _currentScore;
                if (change > 5000) _movingDuration = 2f;
                else if (change > 1000) _movingDuration = 2f;
                else if (change > 600) _movingDuration = 1.5f;
                else if (change > 90) _movingDuration = 1.2f;
                else _movingDuration = 0.5f;

                _currentMovingTime = 0f;
                _startMovingScore = _currentScore;
            }
        }

        public void Show()
        {
            m_Canvas.enabled = true;
        }

        public void Hide()
        {
            m_Canvas.enabled = false;
        }
        #endregion
    }
}