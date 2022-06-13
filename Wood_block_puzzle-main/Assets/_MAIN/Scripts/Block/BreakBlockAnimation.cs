using UnityEngine;
using System.Collections;
using DG.Tweening;
using DarkTonic.PoolBoss;

namespace BP
{
    public class BreakBlockAnimation : MonoBehaviour
    {
        Transform _transform;
        public Transform VTransform
        {
            get 
            {
                if (_transform == null)
                    _transform = transform;

                return _transform;
            }
        }

        public void Break()
        {
            StartCoroutine(YieldBreak());
        }

        private IEnumerator YieldBreak()
        {
            VTransform.rotation = default(Quaternion);
            float currentScale = VTransform.localScale.x;

            float randomTargetRotateZ = Random.Range(-270, 270);
            float randomTargetPositionX = Random.Range(-4f, 4f);
            float randomHeightY = Random.Range(2f, 3f);

            Sequence breakSequence = DOTween.Sequence();
            breakSequence.Insert(0, VTransform.DOScale(currentScale * 1.1f, 0.2f));
            breakSequence.Insert(0, VTransform.DORotate(new Vector3(0f, 0f, randomTargetRotateZ), 1.2f, RotateMode.FastBeyond360));
            breakSequence.Insert(0, VTransform.DOMoveY(randomHeightY + VTransform.position.y, 0.4f).SetEase(Ease.OutCubic));
            breakSequence.Insert(0.4f, VTransform.DOMoveY(-15f + VTransform.position.y, 0.8f).SetEase(Ease.InQuad));
            breakSequence.Insert(0, VTransform.DOMoveX(randomTargetPositionX + VTransform.position.x, 1.2f).SetEase(Ease.Linear));
            
            yield return breakSequence.WaitForCompletion();

            VTransform.localScale = Vector3.one;
            PoolBoss.Despawn(VTransform);
        }

        public void BreakByBomb(Vector3 explodingPoint)
        {
            StartCoroutine(YieldBreakByBomb(explodingPoint));
        }

        private IEnumerator YieldBreakByBomb(Vector3 explodingPoint)
        {
            VTransform.rotation = default(Quaternion);
            float currentScale = VTransform.localScale.x;
            var direction = (VTransform.position - explodingPoint).normalized * 15;

            float randomTargetRotateZ = Random.Range(-270, 270);
            float randomTargetPositionX = Random.Range(-1f, 1f) + direction.x;
            float randomHeightY = Random.Range(2f, 3f) + direction.y;

            Sequence breakSequence = DOTween.Sequence();
            breakSequence.Insert(0, VTransform.DOScale(currentScale * 1.1f, 0.2f));
            breakSequence.Insert(0, VTransform.DORotate(new Vector3(0f, 0f, randomTargetRotateZ), 1.2f, RotateMode.FastBeyond360));
            breakSequence.Insert(0, VTransform.DOMoveY(randomHeightY + VTransform.position.y, 0.8f).SetEase(Ease.OutCubic));
            breakSequence.Insert(0, VTransform.DOMoveX(randomTargetPositionX + VTransform.position.x, 1.0f).SetEase(Ease.Linear));
            
            yield return breakSequence.WaitForCompletion();

            VTransform.localScale = Vector3.one;
            PoolBoss.Despawn(VTransform);
        }
    }
}
