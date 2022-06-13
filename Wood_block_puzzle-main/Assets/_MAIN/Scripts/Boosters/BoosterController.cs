using System.Collections;
using System.Collections.Generic;
using DarkTonic.PoolBoss;
using DG.Tweening;
using UnityEngine;

namespace BP
{
    public enum BoosterType
    {
        UNKNOWN = -1,
        ROTATE = 0,
        SWITCH = 1,
        BOMB = 2
    }

    public class BoosterController : MonoBehaviour
    {
        public static readonly int STAR_AMOUNT_PER_CHEST = 6;
        public static readonly string PREFS_CHEST_COLLECTED_STARS = "prefs_chest_collected_stars";
        public static readonly string PREFS_ROTATE_COUNT = "prefs_rotate_count";
        public static readonly string PREFS_SWITCH_COUNT = "prefs_switch_count";
        public static readonly string PREFS_BOMB_COUNT = "prefs_bomb_count";

        [SerializeField]
        BoosterControllerDefinition _definition;
        [SerializeField]
        StarProgress _starProgress;
        [SerializeField]
        GUIRewardedVideoPopup _rewardedVideoPopup;

        [Header("Chest")]
        [SerializeField]
        private Transform _chestRewardingPoint;
        [SerializeField]
        private Animator _chestAnimator;
        [SerializeField]
        private ChestPopup _chestPopup;

        [Header("Buttons")]
        [SerializeField]
        ButtonBooster _boosterRotateButton;
        [SerializeField]
        ButtonBooster _boosterSwitchButton;
        [SerializeField]
        ButtonBooster _boosterBombButton;

        int _rotateRemainingCount;
        int _switchRemainingCount;
        int _bombRemainingCount;

        public bool enabledRotate { get; protected set; }
        public bool enabledBomb { get; protected set; }

        public void Init()
        {
            enabledRotate = false;

            _rotateRemainingCount = PrefsUtils.GetInt(PREFS_ROTATE_COUNT, 3);
            _switchRemainingCount = PrefsUtils.GetInt(PREFS_SWITCH_COUNT, 3);
            _bombRemainingCount = PrefsUtils.GetInt(PREFS_BOMB_COUNT, 1);

            _starProgress.Init();
            _starProgress.onRewardingBooster += HandleOnRewardingBooster;
            _rewardedVideoPopup.Configure();

            _chestAnimator.Play("chest_idle");
            _chestPopup.Init();
            _chestPopup.UpdateInfo(_starProgress.currentBoosterType, 1);

            _boosterRotateButton.Init();
            _boosterRotateButton.UpdateBoosterRemainingCount(_rotateRemainingCount);
            _boosterRotateButton.Enable();
            _boosterRotateButton.CancelRotate();
            _boosterSwitchButton.Init();
            _boosterSwitchButton.UpdateBoosterRemainingCount(_switchRemainingCount);
            _boosterSwitchButton.Enable();
            _boosterBombButton.Init();
            _boosterBombButton.UpdateBoosterRemainingCount(_bombRemainingCount);
            _boosterBombButton.CancelBomb();
        }

        public void DisableAllRotate()
        {
            _boosterRotateButton.Disable();
            _boosterSwitchButton.Disable();
            _boosterBombButton.Disable();
        }

        public void EnableAllRotate()
        {
            _boosterRotateButton.Enable();
            _boosterSwitchButton.Enable();
            _boosterBombButton.Enable();
        }

        #region Booster Rotate
        public void HandleJustUsedRotatedBlock()
        {
            _rotateRemainingCount--;
            if (_rotateRemainingCount < 0) _rotateRemainingCount = 0;
            PrefsUtils.SetInt(PREFS_ROTATE_COUNT, _rotateRemainingCount);

            _boosterRotateButton.UpdateBoosterRemainingCount(_rotateRemainingCount);

            if (_rotateRemainingCount == 0)
                DisableRotate();
        }

        public void SelectBoosterRotate()
        {
            if (!enabledRotate && _rotateRemainingCount > 0)
            {
                EnableRotate();
            }
            else
            {
                DisableRotate();
            }
        }

        private void EnableRotate()
        {
            enabledRotate = true;
            _boosterRotateButton.UseRotate();
        }

        private void DisableRotate()
        {
            enabledRotate = false;
            _boosterRotateButton.CancelRotate();
        }
        #endregion

        #region Switch
        public void UseBoosterSwitch()
        {
            _switchRemainingCount = _switchRemainingCount <= 0 ? 0 : _switchRemainingCount - 1;
            PrefsUtils.SetInt(PREFS_SWITCH_COUNT, _switchRemainingCount);

            _boosterSwitchButton.UpdateBoosterRemainingCount(_switchRemainingCount);
        }

        public bool IsAvailableSwitchTurn()
        {
            return _switchRemainingCount > 0;
        }
        #endregion

        #region Bomb
        public void UseBoosterBomb()
        {
            enabledBomb = true;
            _boosterBombButton.UseBomb();

            _boosterRotateButton.Disable();
            _boosterSwitchButton.Disable();
        }

        public void CancelBoosterBomb()
        {
            enabledBomb = false;
            _boosterBombButton.CancelBomb();

            _boosterRotateButton.Enable();
            _boosterSwitchButton.Enable();
        }

        public void ConfirmUsedBomb()
        {
            _bombRemainingCount = _bombRemainingCount <= 0 ? 0 : _bombRemainingCount - 1;
            PrefsUtils.SetInt(PREFS_BOMB_COUNT, _bombRemainingCount);

            _boosterBombButton.UpdateBoosterRemainingCount(_bombRemainingCount);

            enabledBomb = false;
            _boosterBombButton.CancelBomb();

            _boosterRotateButton.Enable();
            _boosterSwitchButton.Enable();
        }

        public bool IsAvailableBombTurn()
        {
            return _bombRemainingCount > 0;
        }
        #endregion

        bool _collecting = false;
        public void CollectStars(List<Vector3> starPositions)
        {
            StartCoroutine(YieldCollectStars(starPositions));
        }

        private IEnumerator YieldCollectStars(List<Vector3> starPositions)
        {
            Transform[] starTransformArray = new Transform[starPositions.Count];
            Ease[] starMovingEaseArray = new Ease[] { Ease.InCubic, Ease.InBack };
            for (int i = 0; i < starPositions.Count; i++)
            {
                Transform starTransform = PoolBoss.SpawnInPool(_definition.starPrefab.transform, Vector3.zero, default(Quaternion));
                starTransform.position = starPositions[i];
                starTransform.localScale = Vector3.one;

                Sequence starSequence = DOTween.Sequence();
                starSequence.Append(
                    starTransform
                        .DORotate(new Vector3(0f, 0f, 360f), 1.2f, RotateMode.FastBeyond360));
                starSequence.Insert(
                    0,
                    starTransform
                        .DOMoveX(_starProgress.currentStarPosition.x, 1.2f)
                        .SetEase(starMovingEaseArray[Random.Range(0, 2)]));
                starSequence.Insert(
                    0,
                    starTransform
                        .DOMoveY(_starProgress.currentStarPosition.y, 1.2f)
                        .SetEase(starMovingEaseArray[Random.Range(0, 2)]));

                starTransformArray[i] = starTransform;
            }

            yield return new WaitForSeconds(1.2f);

            _starProgress.CollectedStars(starPositions.Count);

            for (int i = 0; i < starTransformArray.Length; i++)
            {
                PoolBoss.Despawn(starTransformArray[i]);
            }

            yield return null;
        }

        private void HandleOnRewardingBooster(BoosterType type)
        {
            StartCoroutine(YieldCollectBoosterIcon(type));
        }

        private IEnumerator YieldCollectBoosterIcon(BoosterType type)
        {
            _chestAnimator.Play("chest_open");

            yield return new WaitForSeconds(1.2f);
            SoundManager.Instance.PlayBoxOpening();

            yield return new WaitForSeconds(0.3f);

            var prefab = _definition.rotateIconPrefab.transform;
            var boosterRewardPoint = _boosterRotateButton.pBoosterIconTransform;
            if (type == BoosterType.ROTATE)
            {
                prefab = _definition.rotateIconPrefab.transform;
                boosterRewardPoint = _boosterRotateButton.pBoosterIconTransform;
            }
            else if (type == BoosterType.SWITCH)
            {
                prefab = _definition.switchIconPrefab.transform;
                boosterRewardPoint = _boosterSwitchButton.pBoosterIconTransform;
            }
            else if (type == BoosterType.BOMB)
            {
                prefab = _definition.bombIconPrefab.transform;
                boosterRewardPoint = _boosterBombButton.pBoosterIconTransform;
            }

            FlyingBoosterIcon flyingIcon = PoolBoss.SpawnInPool(prefab, Vector3.zero, default(Quaternion)).GetComponent<FlyingBoosterIcon>();
            flyingIcon.VTransform.localScale = Vector3.zero;
            flyingIcon.VTransform.position = _chestRewardingPoint.position;
            flyingIcon.VTransform.rotation = default(Quaternion);
            flyingIcon.EnableTrail(false);

            Sequence movingSeq = DOTween.Sequence();
            movingSeq.Insert(0, flyingIcon.VTransform.DOScale(1.2f, 1f).SetEase(Ease.OutBack));
            movingSeq.Insert(0, flyingIcon.VTransform.DOMoveY(_chestRewardingPoint.position.y + 0.5f, 1f).SetEase(Ease.OutBack));
            movingSeq.InsertCallback(1.5f, () => 
            {
                flyingIcon.EnableTrail(true);

                SoundManager.Instance.PlayBoosterIconFlying();
            });
            movingSeq.Insert(1.5f, flyingIcon.VTransform.DOMove(boosterRewardPoint.position, 1.5f).SetEase(Ease.Linear));
            movingSeq.Insert(1.5f, flyingIcon.VTransform.DOScale(1f, 1f));
            yield return movingSeq.WaitForCompletion();

            PoolBoss.Despawn(flyingIcon.VTransform);

            if (type == BoosterType.ROTATE)
            {
                _boosterRotateButton.AnimateReceivingBoosterIcon();
                AddOneRotateTurn();
            }
            else if (type == BoosterType.SWITCH)
            {
                _switchRemainingCount++;
                PrefsUtils.SetInt(PREFS_SWITCH_COUNT, _switchRemainingCount);

                _boosterSwitchButton.UpdateBoosterRemainingCount(_switchRemainingCount);
                _boosterSwitchButton.AnimateReceivingBoosterIcon();
            }
            else if (type == BoosterType.BOMB)
            {
                _bombRemainingCount++;
                PrefsUtils.SetInt(PREFS_BOMB_COUNT, _bombRemainingCount);

                _boosterBombButton.UpdateBoosterRemainingCount(_bombRemainingCount);
                _boosterBombButton.AnimateReceivingBoosterIcon();
            }

            _chestPopup.UpdateInfo(_starProgress.currentBoosterType, 1);
            _chestPopup.ShowThenAutomaticHide();
        }

        private void AddOneRotateTurn()
        {
            _rotateRemainingCount++;
            PrefsUtils.SetInt(PREFS_ROTATE_COUNT, _rotateRemainingCount);

            _boosterRotateButton.UpdateBoosterRemainingCount(_rotateRemainingCount);
        }

        private void AddOneSwitchTurn()
        {
            _switchRemainingCount++;
            PrefsUtils.SetInt(PREFS_SWITCH_COUNT, _switchRemainingCount);

            _boosterSwitchButton.UpdateBoosterRemainingCount(_switchRemainingCount);
        }

        private void AddOneBombTurn()
        {
            _bombRemainingCount++;
            PrefsUtils.SetInt(PREFS_BOMB_COUNT, _bombRemainingCount);

            _boosterBombButton.UpdateBoosterRemainingCount(_bombRemainingCount);
        }

        public void ShowRewardedVideoPopup(BoosterType boosterType)
        {
            _rewardedVideoPopup.Show();
            _rewardedVideoPopup.InitBoosterType(boosterType);
        }

        public void RewardRotateByWatchingVideo()
        {
            _boosterRotateButton.AnimateReceivingBoosterIcon();
            AddOneRotateTurn();
        }

        public void RewardSwitchByWatchingVideo()
        {
            _boosterSwitchButton.AnimateReceivingBoosterIcon();
            AddOneSwitchTurn();
        }

        public void RewardBombByWatchingVideo()
        {
            _boosterBombButton.AnimateReceivingBoosterIcon();
            AddOneBombTurn();
        }

        public Vector3 GetButtonRotatePosition()
        {
            return _boosterRotateButton.pBoosterIconTransform.position;
        }

        public bool IsAvailableRotateTurn()
        {
            return PrefsUtils.GetInt(PREFS_ROTATE_COUNT, 0) > 0;
        }

        public void OnChestPressed()
        {
            if (_chestPopup.pIsShowing)
                _chestPopup.Hide();
            else 
                _chestPopup.Show();
        }
    }
}
