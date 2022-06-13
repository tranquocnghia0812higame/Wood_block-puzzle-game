using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Tidi.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public class GUIGameOver : MonoBehaviour
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private Canvas m_Canvas;

        [SerializeField]
        private TMPro.TextMeshProUGUI m_HighscoreText;
        [SerializeField]
        private TMPro.TextMeshProUGUI m_CurrentScoreText;

        [SerializeField]
        private Animator m_Animator;
        #endregion

        #region Properties
        private int m_Score;
        private int m_Highscore;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public void Configure()
        {
            m_Canvas.enabled = false;
            m_Animator.Play("game_over_hided");
        }

        public void Show(int score, int highscore)
        {
            m_Canvas.enabled = true;

            m_Animator.Play("game_over_show");

            m_Score = score;
            m_Highscore = highscore;

            m_CurrentScoreText.text = "0";
            m_HighscoreText.text = m_Highscore.ToString();

            StartCoroutine(DoAnimateScore());
        }

        private IEnumerator DoAnimateScore()
        {
            var delay = new WaitForSeconds(0.02f);
            float currentScore = 0;
            float duration = (float)m_Score / 1000f;
            duration = Mathf.Clamp(duration, 0f, 2f);
            float factor = (float)m_Score / duration * 0.02f;
            int count = 0;
            while (currentScore <= m_Score)
            {
                m_CurrentScoreText.text = ((int) currentScore).ToString();
                currentScore += factor;
                count++;
                if (count % 2 == 0)
                    SoundManager.Instance.PlaySpin();

                yield return delay;
            }

            m_CurrentScoreText.text = m_Score.ToString();
        }

        public void Hide()
        {
            m_Animator.Play("game_over_hide");
        }

        public void Replay()
        {
            SoundManager.Instance.PlayClick();

            Hide();

            GameManager.Instance.ReplayAfterGameOver();
        }

        public void OnHided()
        {
            StopCoroutine("DoAnimateScore");
            m_Canvas.enabled = false;
        }
        #endregion
    }
}