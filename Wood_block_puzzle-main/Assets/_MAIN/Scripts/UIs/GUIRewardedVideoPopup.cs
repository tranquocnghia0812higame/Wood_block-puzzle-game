using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Tidi.Ads;
using UnityEngine;

namespace BP
{
    public class GUIRewardedVideoPopup : BasePopup
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        Animator _animator;
        [SerializeField]
        TMPro.TextMeshProUGUI _messageText;
        #endregion

        #region Properties
        BoosterType _boosterType;
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public override void Show()
        {
            base.Show();


        }

        public void InitBoosterType(BoosterType type)
        {
            _boosterType = type;

            if (_boosterType == BoosterType.ROTATE)
            {
                _animator.Play("rewards_tutorial_idle");
                _messageText.text = "Watch a video to get a Spin Bonus!";
            }
            else if (_boosterType == BoosterType.SWITCH)
            {
                _animator.Play("rewards_tutorial_switch");
                _messageText.text = "Watch a video to get a Switch Bonus!";
            }
            else if (_boosterType == BoosterType.BOMB)
            {
                _animator.Play("rewards_tutorial_bomb");
                _messageText.text = "Watch a video to get a Bomb Bonus!";
            }
        }

        public void WatchVideo()
        {
            SoundManager.Instance.PlayClick();
            if (AdManager.Instance.IsRewardedVideoLoaded())
            {
                AdManager.Instance.ShowRewardedVideo();
                AdManager.Instance.onWatchVideoReward += HandleOnWatchVideoReward;
            }
        }

        private void HandleOnWatchVideoReward(bool succeed)
        {
            AdManager.Instance.onWatchVideoReward -= HandleOnWatchVideoReward;
            Hide();
            if (succeed)
            {
                if (_boosterType == BoosterType.ROTATE)
                    GameManager.Instance.RewardRotateByWatchingVideo();
                else if (_boosterType == BoosterType.SWITCH)
                    GameManager.Instance.RewardSwitchByWatchingVideo();
                else if (_boosterType == BoosterType.BOMB)
                    GameManager.Instance.RewardBombByWatchingVideo();
            }
        }

        public void Close()
        {
            SoundManager.Instance.PlayClick();
            Hide();
        }
        #endregion
    }
}
