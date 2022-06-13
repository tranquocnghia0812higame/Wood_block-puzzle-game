using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public class SoundManager : Singleton<SoundManager>
    {
        #region Constants
        #endregion

        #region Events
        #endregion

        #region Fields
        [SerializeField]
        private AudioSource m_BackgroundSource;
        [SerializeField]
        private AudioSource m_SFXSource;
        [SerializeField]
        private AudioSource m_BackupSFXSource;

        [SerializeField]
        private AudioClip m_BackgroundClip;
        [SerializeField]
        private AudioClip m_ClickClip;
        [SerializeField]
        private AudioClip m_PlacedBlockClip;
        [SerializeField]
        private AudioClip m_PlacedFailedClip;
        [SerializeField]
        private AudioClip m_BreakBlockClip;
        [SerializeField]
        private AudioClip m_HighscoreClip;
        [SerializeField]
        private AudioClip m_LoseClip;
        [SerializeField]
        private AudioClip m_BrokenBlockCollisionClip;
        [SerializeField]
        private AudioClip m_SpinClip;
        [SerializeField]
        private AudioClip m_FireworkClip;
        [SerializeField]
        private AudioClip m_TickTimerClip;

        [SerializeField]
        private AudioClip[] m_AddStarClips;

        [SerializeField]
        private AudioClip m_GotSpinClip;

        [SerializeField]
        private AudioClip m_BombClip;

        [SerializeField]
        private AudioClip m_BoxOpeningClip;
        [SerializeField]
        private AudioClip m_BoosterIconFlyingClip;

        [SerializeField]
        private AudioClip m_StartGameClip;

        [Header("Voices")]
        [SerializeField]
        private AudioClip m_GoodClip;
        [SerializeField]
        private AudioClip m_GreatClip;
        [SerializeField]
        private AudioClip m_PerfectClip;
        #endregion

        #region Properties
        #endregion

        #region Unity Events
        #endregion

        #region Methods
        public bool IsSoundOn()
        {
            return PrefsUtils.GetBool(Consts.PREFS_SOUND_SWITCH, true);
        }

        public bool IsMusicOn()
        {
            return PrefsUtils.GetBool(Consts.PREFS_MUSIC_SWITCH, true);
        }

        public void SwitchSound()
        {
            PrefsUtils.SetBool(Consts.PREFS_SOUND_SWITCH, !IsSoundOn());
        }

        public void SwitchMusic()
        {
            PrefsUtils.SetBool(Consts.PREFS_MUSIC_SWITCH, !IsMusicOn());
            
            if (!IsMusicOn())
                PauseBackground();
            else 
                PlayBackground();
        }

        public void PlayBackground()
        {
            if (!IsMusicOn())
                return;

            // m_BackgroundSource.clip = m_BackgroundClip;
            // m_BackgroundSource.Play();
        }

        public void PauseBackground()
        {
            // m_BackgroundSource.Stop();
        }

        public void PlayClick()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_ClickClip);
        }

        public void PlayPlacedBlock()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_PlacedBlockClip);
        }

        public void PlayPlacedFailed()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_PlacedFailedClip);
        }

        public void PlayBreakSingleBlock()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_BreakBlockClip);
        }

        public void PlayLose()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_LoseClip);
        }

        public void PlayChooseBlock()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_BrokenBlockCollisionClip);
        }

        public void PlayHighscore()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_HighscoreClip);
        }

        public void PlayParticleCollision()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_BrokenBlockCollisionClip);
        }

        public void PlaySpin()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_SpinClip);
        }

        public void PlayFireWork()
        {
            if (!IsSoundOn())
                return;

            m_SFXSource.PlayOneShot(m_FireworkClip);
        }

        public void PlayTickTimer()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.clip = m_TickTimerClip;
            m_BackupSFXSource.loop = true;
            m_BackupSFXSource.Play();
        }

        public void StopTickTimer()
        {
            m_BackupSFXSource.Stop();
            m_BackupSFXSource.loop = false;
        }

        public void PlayAddStar(int index)
        {
            if (!IsSoundOn())
                return;

            index = Mathf.Clamp(index, 0, m_AddStarClips.Length - 1);
            m_BackupSFXSource.PlayOneShot(m_AddStarClips[index]);
        }

        public void PlayGotSpin()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_GotSpinClip);
        }

        public void PlayBomb()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_BombClip);
        }

        public void PlayBoxOpening()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_BoxOpeningClip, 0.5f);
        }

        public void PlayBoosterIconFlying()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_BoosterIconFlyingClip);
        }

        public void PlayVoiceGreat()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_GreatClip);
        }

        public void PlayVoicePerfect()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_PerfectClip);
        }

        public void PlayVoiceGood()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_GoodClip);
        }

        public void PlayStartGame()
        {
            if (!IsSoundOn())
                return;

            m_BackupSFXSource.PlayOneShot(m_StartGameClip, 0.3f);
        }
        #endregion
    }
}