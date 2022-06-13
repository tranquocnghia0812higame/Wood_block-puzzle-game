using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class GUIRating : BasePopup
    {
        #region Constants
        #endregion
        
        #region Events
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public override void Show()
        {
            base.Show();
        }

        public void RemindLaterPressed()
        {
            SoundManager.Instance.PlayClick();

            Hide();
        }

        public void Rate()
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.screenType = ScreenType.RATING;

            Application.OpenURL("market://details?id=" + Application.identifier);
            
            PrefsUtils.SetBool(Consts.PREFS_RATED_5_STARS, true);

            Hide();
        }
        #endregion
    }
}
