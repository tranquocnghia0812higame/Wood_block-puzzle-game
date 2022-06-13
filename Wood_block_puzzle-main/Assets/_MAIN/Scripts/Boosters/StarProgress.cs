using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BP
{
    public struct CollectingStarCommand
    {

    }

    public class StarProgress : MonoBehaviour
    {
        public static readonly int STAR_AMOUNT_PER_CHEST = 6;
        public static readonly string PREFS_CHEST_COLLECTED_STARS = "prefs_chest_collected_stars";
        public static readonly string PREFS_CHEST_REWARD_BOOSTER_TYPE = "prefs_chest_reward_booster_type";
        private static readonly float PROGRESS_SPEED = 4f;

        public delegate void OnRewardingBooster(BoosterType type);
        public OnRewardingBooster onRewardingBooster;

        [Header("General")]
        [SerializeField]
        private SpriteRenderer _containerRenderer;
        [SerializeField]
        private SpriteRenderer _progressRenderer;

        [Header("Star")]
        [SerializeField]
        private Transform _starTransform;
        [SerializeField]
        private GameObject _receivedStarEffectObject;

        [Header("Indicators")]
        [SerializeField]
        private Transform _indicatorContainer;

        List<Transform> _indicatorTransformList;

        public Vector3 currentStarPosition => _starTransform.position;

        float _containerWidth;

        Coroutine _progressCoroutine;

        int _chestCollectedStars;

        BoosterType _currentBoosterType;
        public BoosterType currentBoosterType => _currentBoosterType;

        Queue<CollectingStarCommand> _collectingStarCommandQueue;

        public void Init()
        {
            _containerWidth = _containerRenderer.sprite.rect.width / _containerRenderer.sprite.pixelsPerUnit;

            _receivedStarEffectObject.SetActive(false);

            SpawnIndicators();

            _currentBoosterType = (BoosterType)PrefsUtils.GetInt(PREFS_CHEST_REWARD_BOOSTER_TYPE, -1);
            if (_currentBoosterType == BoosterType.UNKNOWN)
            {
                _currentBoosterType = GetRandomBoosterType();
                PrefsUtils.SetInt(PREFS_CHEST_REWARD_BOOSTER_TYPE, (int)_currentBoosterType);
            }

            _collectingStarCommandQueue = new Queue<CollectingStarCommand>();

            _chestCollectedStars = PrefsUtils.GetInt(PREFS_CHEST_COLLECTED_STARS, 0);
            UpdateProgress((float)_chestCollectedStars / (float)STAR_AMOUNT_PER_CHEST);
        }

        private void SpawnIndicators()
        {
            _indicatorTransformList = new List<Transform>();
            for (int i = 0; i < _indicatorContainer.childCount; i++)
            {
                var child = _indicatorContainer.GetChild(i);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    _indicatorTransformList.Add(child);
                }
            }

            float chunk = _containerWidth / (float)STAR_AMOUNT_PER_CHEST;

            for (int i = 1; i < STAR_AMOUNT_PER_CHEST; i++)
            {
                if (i >= _indicatorTransformList.Count)
                {
                    var child = Instantiate(_indicatorTransformList[0], Vector3.zero, default(Quaternion));
                    child.SetParent(_indicatorContainer);
                    child.localScale = Vector3.one;
                    child.gameObject.SetActive(false);
                    _indicatorTransformList.Add(child);
                }

                _indicatorTransformList[i].gameObject.SetActive(true);
                _indicatorTransformList[i].localPosition = new Vector2(i * chunk, 0);
            }
        }

        public void UpdateProgress(float progress)
        {
            var size = _progressRenderer.size;
            size.x = progress * _containerWidth;
            _progressRenderer.size = size;

            if (progress == 0)
            {
                _starTransform.localPosition = Vector3.zero;
                _starTransform.gameObject.SetActive(false);
            }
            else
            {
                _starTransform.localPosition = new Vector3(size.x, 0, 0);
                _starTransform.gameObject.SetActive(true);
            }
        }

        public void CollectedStars(int count)
        {
            for (int i = 0; i < count; i++)
                _collectingStarCommandQueue.Enqueue(new CollectingStarCommand());

            StartCoroutine(YieldPlayCollectingStarEffect());

            SoundManager.Instance.PlayAddStar(count);

            if (!_isAnimating)
                StartCoroutine(YieldCollectedStars());
        }

        bool _isAnimating = false;
        private IEnumerator YieldCollectedStars()
        {
            _isAnimating = true;

            _starTransform.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.8f);

            while (_collectingStarCommandQueue.Count > 0)
            {
                var command = _collectingStarCommandQueue.Dequeue();
                _chestCollectedStars++;
                PrefsUtils.SetInt(PREFS_CHEST_COLLECTED_STARS, _chestCollectedStars);

                _starTransform.gameObject.SetActive(true);

                yield return AnimateProgress((float)_chestCollectedStars / (float)STAR_AMOUNT_PER_CHEST);

                if (_chestCollectedStars >= STAR_AMOUNT_PER_CHEST)
                {
                    _chestCollectedStars = 0;
                    PrefsUtils.SetInt(PREFS_CHEST_COLLECTED_STARS, _chestCollectedStars);

                    onRewardingBooster?.Invoke(_currentBoosterType);

                    _currentBoosterType = GetRandomBoosterType();
                    PrefsUtils.SetInt(PREFS_CHEST_REWARD_BOOSTER_TYPE, (int)_currentBoosterType);
                    yield return new WaitForSeconds(0.5f);

                    UpdateProgress(0);
                }
            }

            _isAnimating = false;
        }

        private IEnumerator YieldPlayCollectingStarEffect()
        {
            _receivedStarEffectObject.SetActive(true);

            yield return new WaitForSeconds(1.5f);
            _receivedStarEffectObject.SetActive(false);
        }

        private Coroutine AnimateProgress(float progress)
        {
            if (_progressCoroutine != null)
                StopCoroutine(_progressCoroutine);

            _progressCoroutine = StartCoroutine(YieldAnimateProgress(progress));
            return _progressCoroutine;
        }

        private IEnumerator YieldAnimateProgress(float progress)
        {
            if (progress > 1)
                progress = 1f;

            _starTransform.gameObject.SetActive(true);

            var progressSize = _progressRenderer.size;
            float targetWidth = progress * _containerWidth;

            while (progressSize.x < targetWidth)
            {
                progressSize.x = Mathf.Clamp(progressSize.x + PROGRESS_SPEED * Time.deltaTime, 0, targetWidth);
                _progressRenderer.size = progressSize;
                _starTransform.localPosition = new Vector3(progressSize.x, 0, 0);
                yield return null;
            }

            progressSize.x = targetWidth;
            _progressRenderer.size = progressSize;
            _starTransform.localPosition = new Vector3(progressSize.x, 0, 0);
        }

        private BoosterType GetRandomBoosterType()
        {
            return (BoosterType)Random.Range(0, 3);
        }
    }
}
